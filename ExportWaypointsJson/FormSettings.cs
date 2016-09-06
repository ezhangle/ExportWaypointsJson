﻿using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace ExportWaypointsJson
{
  public partial class FormSettings : Form
  {
    Settings _settings;

    public FormSettings()
    {
      _settings = Settings.Load();

      InitializeComponent();

      txtIpAddress.Text = Properties.Settings.Default
        .IpAddress;

      txtDistance.Text = Properties.Settings.Default
        .DistanceInMeters.ToString( "0.##" );

      txtIpAddress.Validating += TxtIpAddress_Validating;
      txtIpAddress.Validated += TxtIpAddress_Validated;
      txtDistance.Validating += TxtDistance_Validating;
      txtDistance.Validated += TxtDistance_Validated;
    }

    private void TxtIpAddress_Validating( 
      object sender,
      CancelEventArgs e )
    {
      string s = txtIpAddress.Text;
      IPAddress address;
      try
      {
        address = IPAddress.Parse( s );
      }
      catch( System.FormatException )
      {
        // Cancel the event.
        e.Cancel = true;
        // Select the text to be corrected by the user.
        txtIpAddress.Select( 0, txtIpAddress.Text.Length );
        // Report error.
        this.errorProvider1.SetError( txtIpAddress,
          "Invalid IP address: " + s );
      }
    }

    private void TxtIpAddress_Validated(
      object sender,
      EventArgs e )
    {
      errorProvider1.SetError( txtIpAddress, "" );
    }

    private void TxtDistance_Validating(
      object sender,
      CancelEventArgs e )
    {
      string s = txtDistance.Text;
      double d;
      try
      {
        d = double.Parse( s );
      }
      catch( System.FormatException )
      {
        // Cancel the event.
        e.Cancel = true;
        // Select the text to be corrected by the user.
        txtDistance.Select( 0, txtDistance.Text.Length );
        // Report error.
        this.errorProvider1.SetError( txtDistance, 
          "Invalid distance in metres: " + s );
      }
    }

    private void TxtDistance_Validated(
      object sender,
      EventArgs e )
    {
      errorProvider1.SetError( txtDistance, "" );
    }

    private void btnSave_Click(
      object sender,
      EventArgs e )
    {
      Properties.Settings.Default.IpAddress 
        = txtIpAddress.Text;

      Properties.Settings.Default.DistanceInMeters 
        = double.Parse( txtDistance.Text );

      Properties.Settings.Default.Save();

      _settings.IpAddress = txtIpAddress.Text;
      _settings.DistanceInMetres = double.Parse( txtDistance.Text );
      _settings.Save();
    }
  }
}
