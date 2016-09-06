#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace ExportWaypointsJson
{
  [Transaction( TransactionMode.ReadOnly )]
  public class Command : IExternalCommand
  {
    const string _exit_path_filename = "exitpath.json";

    const string _please_select_model_curve = "Please select a single model curve representing the exist path";

    class CurveSelectionFilter : ISelectionFilter
    {
      public bool AllowElement( Element e )
      {
        return e is ModelCurve;
      }

      public bool AllowReference( Reference r, XYZ p )
      {
        return true;
      }
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      // Select model curve representing exit path.

      ModelCurve model_curve = null;
      Selection sel = uidoc.Selection;
      ICollection<ElementId> ids = sel.GetElementIds();
      int n = ids.Count;

      if( 0 == n )
      {
        try
        {
          Reference r = sel.PickObject( ObjectType.Element,
            new CurveSelectionFilter(),
            "Please pick a model curve to represent the exit path" );

          model_curve = doc.GetElement( r.ElementId ) as ModelCurve;
        }
        catch( Autodesk.Revit.Exceptions.OperationCanceledException )
        {
          message = "No element selected";
          return Result.Cancelled;
        }
      }
      else
      {
        if( 1 == n )
        {
          model_curve = doc.GetElement(
            ids.First<ElementId>() )
              as ModelCurve;
        }
        if( null == model_curve )
        {
          message = _please_select_model_curve;
          return Result.Failed;
        }
      }

      Curve curve = model_curve.GeometryCurve;

      // Tessellate returns 377 points for our sample spline curve.
      //IList<XYZ> pts = curve.Tessellate();

      double length = curve.ApproximateLength;

      double convert_inch_to_cm = 2.54;
      double convert_feet_to_metre = 0.01 * 12 * convert_inch_to_cm;

      double length_m = length * convert_feet_to_metre;
      double d = Settings.Load().DistanceInMetres;

      int nPoints = (int) ( ( length_m / d ) + 1 );

      List<XYZ> pts = new List<XYZ>( nPoints );

      double t = curve.GetEndParameter( 0 );
      double tend = curve.GetEndParameter( 1 );
      d = ( tend - t ) / nPoints;

      for( ; t < tend; t += d )
      {
        pts.Add( curve.Evaluate( t, false ) );
      }

      string path = Path.Combine( App.Path, _exit_path_filename );
      File.WriteAllText( path, 
        ( new JavaScriptSerializer() ).Serialize( 
          pts ) );

      TaskDialog.Show( App.Caption, string.Format( 
        "Exported {0} waypoints to {1}.",
        pts.Count, path ) );

      return Result.Succeeded;
    }
  }
}
