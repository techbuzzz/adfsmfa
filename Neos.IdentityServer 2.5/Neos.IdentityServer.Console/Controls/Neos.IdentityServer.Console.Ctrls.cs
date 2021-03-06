﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_Ctrls;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using Neos.IdentityServer.Console.Resources;
using System.Text.RegularExpressions;
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;

namespace Neos.IdentityServer.Console.Controls
{

    public partial class ADFSServerControl : Panel, IMMCRefreshData
    {
        private Panel _panel;
        private Panel _txtpanel;
        private LinkLabel tblstartstop;
        private ADFSServerHost _host;
        private Label lblFQDN;
        private Label lblBehavior;
        private Label lblNodetype;
        private Label lblOsversion;
        private Label lblcurrentversion;
        private Label lblBuild;
        private LinkLabel tblRestart;
        private LinkLabel tblrestartfarm;
        private ServiceViewControl _view;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public ADFSServerControl(ServiceViewControl view, ADFSServerHost server, bool isrunning = true)
        {
            _host = server;
            _view = view;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }


        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ServiceStatusChanged += ServersStatusChanged;
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        private void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();

        }

        /// <summary>
        /// ServersStatusChanged method implementation
        /// </summary>
        private void ServersStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            if ((servername.ToLower() == _host.FQDN.ToLower()) || (servername.ToLower() == _host.MachineName.ToLower()))
            {
                UpdateLayoutServerStatus(status);
                if (_view.IsNotifsEnabled())
                   DoRefreshData();
                else
                   _panel.Refresh();
            }
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implmentation
        /// </summary>
        private void UpdateLayoutServerStatus(ServiceOperationStatus status)
        {
            switch (status)
            {
                case ServiceOperationStatus.OperationInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ServiceOperationStatus.OperationPending:
                    _panel.BackColor = Color.Orange;
                    break;
                case ServiceOperationStatus.OperationRunning:
                    if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ServiceOperationStatus.OperationStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            UpdateLabels(status);
            this._panel.Refresh();
        }

                /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
        }

        /// <summary>
        /// Host property implementation
        /// </summary>
        public ADFSServerHost Host
        {
            get { return _host; }
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 125;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 75;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 75;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblFQDN = new Label
                {
                    Text = _host.FQDN,
                    Left = 10,
                    Top = 10,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblFQDN);

                lblBehavior = new Label
                {
                    Text = "Behavior Level : " + _host.BehaviorLevel.ToString(),
                    Left = 10,
                    Top = 32,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblBehavior);

                lblNodetype = new Label
                {
                    Text = "Node Type : " + _host.NodeType,
                    Left = 10,
                    Top = 54,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblNodetype);

                // Second col
                lblOsversion = new Label();
                if (_host.CurrentMajorVersionNumber!=0)
                    lblOsversion.Text = _host.ProductName + " ("+_host.CurrentMajorVersionNumber.ToString()+"."+_host.CurrentMinorVersionNumber.ToString()+")";
                else
                    lblOsversion.Text = _host.ProductName;
                lblOsversion.Left = 210;
                lblOsversion.Top = 10;
                lblOsversion.Width = 300;
                _txtpanel.Controls.Add(lblOsversion);

                // Second col
                lblcurrentversion = new Label
                {
                    Text = "Version : " + _host.CurrentVersion,
                    Left = 210,
                    Top = 32,
                    Width = 300
                };
                _txtpanel.Controls.Add(lblcurrentversion);

                lblBuild = new Label
                {
                    Text = "Build : " + _host.CurrentBuild.ToString(),
                    Left = 210,
                    Top = 54,
                    Width = 300
                };
                _txtpanel.Controls.Add(lblBuild);

                tblRestart = new LinkLabel
                {
                    Text = res.CRTLADFSRESTARTSERVICES,
                    Left = 20,
                    Top = 80,
                    Width = 200
                };
                tblRestart.LinkClicked += TblRestartLinkClicked;
                tblRestart.TabIndex = 0;
                tblRestart.TabStop = true;
                this.Controls.Add(tblRestart);

                tblstartstop = new LinkLabel();
                if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
                    tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                else
                    tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblstartstop.Left = 230;
                tblstartstop.Top = 80;
                tblstartstop.Width = 200;
                tblstartstop.LinkClicked += TblstartstopLinkClicked;
                tblRestart.TabIndex = 1;
                tblRestart.TabStop = true;
                this.Controls.Add(tblstartstop);

                tblrestartfarm = new LinkLabel
                {
                    Text = res.CRTLADFSRESTARTFARMSERVICES,
                    Left = 20,
                    Top = 105,
                    Width = 400
                };
                tblrestartfarm.LinkClicked += TblrestartfarmLinkClicked;
                tblrestartfarm.TabStop = true;
                this.Controls.Add(tblrestartfarm);
            }
            finally
            {
                UpdateLayoutServerStatus(ManagementService.ADFSManager.ServicesStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }

        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {

                lblFQDN.Text = _host.FQDN;
                lblBehavior.Text = "Behavior Level : " + _host.BehaviorLevel.ToString();
                lblNodetype.Text = "Node Type : " + _host.NodeType;
                if (_host.CurrentMajorVersionNumber != 0)
                    lblOsversion.Text = _host.ProductName + " (" + _host.CurrentMajorVersionNumber.ToString() + "." + _host.CurrentMinorVersionNumber.ToString() + ")";
                else
                    lblOsversion.Text = _host.ProductName;
                lblcurrentversion.Text = "Version : " + _host.CurrentVersion;
                lblBuild.Text = "Build : " + _host.CurrentBuild.ToString();
                tblRestart.Text = res.CRTLADFSRESTARTSERVICES;
                if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
                    tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                else
                    tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblrestartfarm.Text = res.CRTLADFSRESTARTFARMSERVICES;
                UpdateLayoutServerStatus(ManagementService.ADFSManager.ServicesStatus);
            }
            finally
            {
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateLabels method implmentation
        /// </summary>
        private void UpdateLabels(ServiceOperationStatus status)
        {
            if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
            {
                tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                tblstartstop.Tag = true;
            }
            else
            {
                tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblstartstop.Tag = false;
            }
        }

        /// <summary>
        /// tblstartstopLinkClicked event implmentation
        /// </summary>
        private void TblstartstopLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ManagementService.ADFSManager.IsRunning(this._host.FQDN))
            {
                ManagementService.ADFSManager.StopService(_host.FQDN);
                (sender as LinkLabel).Text = res.CRTLADFSSTARTSERVICES;
            }
            else
            {
                ManagementService.ADFSManager.StartService(_host.FQDN);
                (sender as LinkLabel).Text = res.CRTLADFSSTOPSERVICES;
            }
        }

        /// <summary>
        /// tblRestartLinkClicked event implementation
        /// </summary>
        private void TblRestartLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementService.ADFSManager.RestartServer(null, _host.FQDN);
        }

        /// <summary>
        /// tblrestartfarmLinkClicked ebvent implmentation
        /// </summary>
        private void TblrestartfarmLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementService.ADFSManager.RestartFarm(null);
        }   
    }

    public partial class MFAProvidersControl : Panel, IMMCRefreshData
    {
        private Panel _panel;
        private Panel _txtpanel;
        private NamespaceSnapInBase _snapin;
        private ProvidersViewControl _view;
        private PreferredMethod _kind;
        private IExternalProvider _provider;
        private ErrorProvider errors;
        private CheckBox chkProviderEnabled;
        private CheckBox chkProviderEnroll;
        private CheckBox chkProviderRequired;
        private CheckBox chkProviderPin;
        private Label lblProviderDesc;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public MFAProvidersControl(ProvidersViewControl view, NamespaceSnapInBase snap, PreferredMethod kind)
        {
            _view = view;
            _snapin = snap;
            _provider = null;
            _kind =kind;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        private void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception Ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implmentation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    _provider = RuntimeAuthProvider.GetProviderInstance(this._kind);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.Red;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            this._panel.Refresh();
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _provider = RuntimeAuthProvider.GetProviderInstance(this._kind);

                this.Dock = DockStyle.Top;
                this.Height = 95;
                this.Width = 760;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 95;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 100;
                _txtpanel.Height = 105;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblProviderDesc = new Label
                {
                    Text = _provider.Description,
                    Left = 10,
                    Top = 10,
                    Width = 500,
                    Height = 30
                };
                lblProviderDesc.Font = new System.Drawing.Font(lblProviderDesc.Font.Name, 16F, FontStyle.Bold);
                _txtpanel.Controls.Add(lblProviderDesc);

                // Second col
                chkProviderEnabled = new CheckBox
                {
                    Text = res.CTRLPROVACTIVE,

                    Checked = _provider.Enabled,
                    Enabled = (_provider.AllowDisable),
                    Left = 510,
                    Top = 10,
                    Width = 300
                };
                chkProviderEnabled.CheckedChanged += ChkProviderChanged;
                _txtpanel.Controls.Add(chkProviderEnabled);

                chkProviderRequired = new CheckBox
                {
                    Text = res.CTRLPROVREQUIRED,
                    Checked = _provider.IsRequired,
                    Enabled = (_provider.AllowDisable),
                    Left = 510,
                    Top = 30,
                    Width = 300
                };
                chkProviderRequired.CheckedChanged += ChkProviderRequiredChanged;
                _txtpanel.Controls.Add(chkProviderRequired);

                chkProviderEnroll = new CheckBox
                {
                    Text = res.CTRLPROVWIZARD
                };
                if (_provider.AllowEnrollment)
                    chkProviderEnroll.Checked = _provider.WizardEnabled;
                else
                    chkProviderEnroll.Checked = false;
                chkProviderEnroll.Enabled = (_provider.Enabled && _provider.AllowEnrollment);
                if (_kind==PreferredMethod.Azure)
                {
                   // chkProviderEnroll.Checked = false;
                   // chkProviderEnroll.Enabled = false;
                    chkProviderRequired.Checked = false;
                    chkProviderRequired.Enabled = false;
                }
                chkProviderEnroll.Left = 510;
                chkProviderEnroll.Top = 50;
                chkProviderEnroll.Width = 300;
                chkProviderEnroll.CheckedChanged += ChkProviderEnrollChanged;
                _txtpanel.Controls.Add(chkProviderEnroll);

                chkProviderPin = new CheckBox
                {
                    Text = res.CTRLPROVPIN,
                    Checked = _provider.PinRequired,
                    Enabled = _provider.Enabled,
                    Left = 510,
                    Top = 70,
                    Width = 300
                };
                chkProviderPin.CheckedChanged += ChkProviderPinChanged;
                _txtpanel.Controls.Add(chkProviderPin);

                errors = new ErrorProvider(_view)
                {
                    BlinkStyle = ErrorBlinkStyle.NeverBlink
                };
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _provider = RuntimeAuthProvider.GetProviderInstance(this._kind);

                lblProviderDesc.Text = _provider.Description;
                chkProviderEnabled.Text = res.CTRLPROVACTIVE;
                chkProviderRequired.Text = res.CTRLPROVREQUIRED;
                chkProviderEnroll.Text = res.CTRLPROVWIZARD;
                chkProviderPin.Text = res.CTRLPROVPIN;

                chkProviderEnabled.Checked = _provider.Enabled;
                chkProviderEnabled.Enabled = (_provider.AllowDisable);
                chkProviderRequired.Enabled = (_provider.AllowDisable);

                if (_provider.AllowEnrollment)
                    chkProviderEnroll.Checked = _provider.WizardEnabled;
                else
                    chkProviderEnroll.Checked = false;
                chkProviderEnroll.Enabled = (_provider.Enabled && _provider.AllowEnrollment);

                if (_kind == PreferredMethod.Azure)
                {
                    chkProviderRequired.Checked = false;
                    chkProviderRequired.Enabled = false;
                }
                else if (_provider is NeosPlugExternalProvider)
                {
                    chkProviderEnroll.Checked = false;
                    chkProviderEnroll.Enabled = false;
                    chkProviderRequired.Checked = false;
                    chkProviderRequired.Enabled = false;
                    chkProviderPin.Checked = false;
                    chkProviderPin.Enabled = false;
                }
                else
                { 
                    chkProviderPin.Checked = _provider.PinRequired;
                    chkProviderPin.Enabled = _provider.Enabled;
                }
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// chkProviderPinChanged method implementation
        /// </summary>
        private void ChkProviderPinChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.Biometrics:
                        Config.WebAuthNProvider.PinRequired = chkProviderPin.Checked;
                        break;

                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderPin, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// chkProviderEnrollChanged method implementation
        /// </summary>
        private void ChkProviderEnrollChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.Biometrics:
                        Config.WebAuthNProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderEnroll, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// chkProviderChanged method implementation
        /// </summary>
        private void ChkProviderChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.Enabled = chkProviderEnabled.Checked;
                        chkProviderEnroll.Enabled = Config.OTPProvider.Enabled;
                        chkProviderPin.Enabled = Config.OTPProvider.Enabled;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.Enabled = chkProviderEnabled.Checked;
                        chkProviderEnroll.Enabled = Config.MailProvider.Enabled;
                        chkProviderPin.Enabled = Config.MailProvider.Enabled;

                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.Enabled = chkProviderEnabled.Checked;
                        chkProviderEnroll.Enabled = Config.ExternalProvider.Enabled;
                        chkProviderPin.Enabled = Config.ExternalProvider.Enabled;

                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.Enabled = chkProviderEnabled.Checked;
                        chkProviderEnroll.Enabled = Config.AzureProvider.Enabled;
                        chkProviderPin.Enabled = Config.AzureProvider.Enabled;
                        break;
                    case PreferredMethod.Biometrics:
                        Config.WebAuthNProvider.Enabled = chkProviderEnabled.Checked;
                        chkProviderEnroll.Enabled = Config.WebAuthNProvider.Enabled;
                        chkProviderPin.Enabled = Config.WebAuthNProvider.Enabled;
                        break;

                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderEnabled, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// chkProviderRequiredChanged method implementation
        /// </summary>
        private void ChkProviderRequiredChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.IsRequired = chkProviderRequired.Checked;
                        chkProviderEnroll.Enabled = Config.OTPProvider.Enabled;
                        chkProviderPin.Enabled = Config.OTPProvider.Enabled;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.IsRequired = chkProviderRequired.Checked;
                        chkProviderEnroll.Enabled = Config.MailProvider.Enabled;
                        chkProviderPin.Enabled = Config.MailProvider.Enabled;

                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.IsRequired = chkProviderRequired.Checked;
                        chkProviderEnroll.Enabled = Config.ExternalProvider.Enabled;
                        chkProviderPin.Enabled = Config.ExternalProvider.Enabled;

                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.IsRequired = chkProviderRequired.Checked;
                        chkProviderEnroll.Enabled = false;
                        chkProviderPin.Enabled = Config.AzureProvider.Enabled;
                        break;
                    case PreferredMethod.Biometrics:
                        Config.WebAuthNProvider.IsRequired = chkProviderRequired.Checked;
                        chkProviderEnroll.Enabled = Config.WebAuthNProvider.Enabled;
                        chkProviderPin.Enabled = Config.WebAuthNProvider.Enabled;
                        break;
                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderEnabled, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }

    public partial class MFAProvidersValidationControl : Panel, IMMCRefreshData
    {
        private ProvidersViewControl _view;
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;

        /// <summary>
        /// MFAProvidersValidationControl Constructor
        /// </summary>
        public MFAProvidersValidationControl(ProvidersViewControl view, NamespaceSnapInBase snap)
        {
            _panel = new Panel();
            _txtpanel = new Panel();
            _view = view;
            _snapin = snap;
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            DoCreateControls();
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 40;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 30;

                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 20;
                _txtpanel.BackColor = System.Drawing.SystemColors.Window;
                this.Controls.Add(_txtpanel);


                tblSaveConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE,
                    Left = 0,
                    Top = 0,
                    Width = 80
                };
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                _txtpanel.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL,
                    Left = 90,
                    Top = 0,
                    Width = 80
                };
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                _txtpanel.Controls.Add(tblCancelConfig);

            }
            finally
            {
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshtta method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked method implementation
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }

        /// <summary>
        /// SaveConfigLinkClicked method implementation
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }
    }

    public partial class ConfigurationControl : Panel, IMMCRefreshData
    {
        private ServiceViewControl _view;
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private Label lblFarmActive;
        private Label lblstorageMode;
        private Label lbladmincontact;
        private Label lblIdentifier;
        private Label lblIsInitialized;
        private Label lblFarmBehavior;
        private Label lbloptions;
        private Label lblSecurity;
        private Label lblcutompwd;
        private LinkLabel tblconfigure;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ConfigurationControl(ServiceViewControl view, NamespaceSnapInBase snap)
        {
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            _view = view;
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    if (ManagementService.ADFSManager.IsMFAProviderEnabled(null))
                    {
                        _panel.BackColor = Color.Green;
                        lblFarmActive.Text = "Active : True";
                    }
                    else
                    {
                        _panel.BackColor = Color.DarkGray;
                        lblFarmActive.Text = "Active : False";
                    }
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 95;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 75;

                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 75;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblIsInitialized = new Label
                {
                    Text = "Initialized : " + ManagementService.ADFSManager.IsFarmConfigured().ToString(),
                    Left = 10,
                    Top = 10,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblIsInitialized);

                lblFarmActive = new Label
                {
                    Text = "Active : " + ManagementService.ADFSManager.IsMFAProviderEnabled(null).ToString(),
                    Left = 10,
                    Top = 32,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblFarmActive);

                lblIdentifier = new Label
                {
                    Text = "Identifier : " + Config.Hosts.ADFSFarm.FarmIdentifier,
                    Left = 10,
                    Top = 54,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblIdentifier);

                // Second col
                lbladmincontact = new Label
                {
                    Text = "Administrative contact : " + Config.AdminContact,
                    Left = 230,
                    Top = 10,
                    Width = 300
                };
                _txtpanel.Controls.Add(lbladmincontact);

                lblstorageMode = new Label();
                if (Config.UseActiveDirectory)
                    lblstorageMode.Text = "Mode : Active Directory";
                else
                    lblstorageMode.Text = "Mode : Sql Server Database";
                lblstorageMode.Left = 230;
                lblstorageMode.Top = 32;
                lblstorageMode.Width = 300;
                _txtpanel.Controls.Add(lblstorageMode);

                lblFarmBehavior = new Label
                {
                    Text = "Behavior : " + Config.Hosts.ADFSFarm.CurrentFarmBehavior.ToString(),
                    Left = 230,
                    Top = 54,
                    Width = 300
                };
                _txtpanel.Controls.Add(lblFarmBehavior);

                // third col
                lbloptions = new Label
                {
                    Text = "Options : "
                };
                if (Config.OTPProvider.Enabled)
                    lbloptions.Text += "TOPT ";
                if (Config.MailProvider.Enabled)
                    lbloptions.Text += "EMAILS ";
                if (Config.ExternalProvider.Enabled)
                    lbloptions.Text += "SMS ";
                if (Config.AzureProvider.Enabled)
                    lbloptions.Text += "AZURE ";

                lbloptions.Left = 550;
                lbloptions.Top = 10;
                lbloptions.Width = 300;
                _txtpanel.Controls.Add(lbloptions);

                lblSecurity = new Label();
                switch (Config.KeysConfig.KeyFormat)
                {
                    case SecretKeyFormat.RSA:
                        lblSecurity.Text = "Security : RSA   " + Config.KeysConfig.CertificateThumbprint;
                        break;
                    case SecretKeyFormat.CUSTOM:
                        lblSecurity.Text = "Security : RSA CUSTOM";
                        break;
                    default:
                        lblSecurity.Text = "Security : RNG";
                        break;
                }
                lblSecurity.Left = 550;
                lblSecurity.Top = 32;
                lblSecurity.Width = 300;
                _txtpanel.Controls.Add(lblSecurity);

                lblcutompwd = new Label
                {
                    Left = 550,
                    Top = 54,
                    Width = 300
                };
                _txtpanel.Controls.Add(lblcutompwd);

                if (Config.CustomUpdatePassword)
                    lblcutompwd.Text = "Use custom change password feature";
                else
                    lblcutompwd.Text = "";

                tblconfigure = new LinkLabel();
                if (ManagementService.ADFSManager.IsMFAProviderEnabled(null))
                    tblconfigure.Text = res.CTRLADFSDEACTIVATEMFA;
                else
                    tblconfigure.Text = res.CTRLADFSACTIVATEMFA;
                tblconfigure.Left = 20;
                tblconfigure.Top = 80;
                tblconfigure.Width = 400;
                tblconfigure.LinkClicked += TblconfigureLinkClicked;
                tblconfigure.TabIndex = 0;
                tblconfigure.TabStop = true;
                this.Controls.Add(tblconfigure);
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData()
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                lblIsInitialized.Text = "Initialized : " + ManagementService.ADFSManager.IsFarmConfigured().ToString();
                lblFarmActive.Text = "Active : " + ManagementService.ADFSManager.IsMFAProviderEnabled(null).ToString();
                lblIdentifier.Text = "Identifier : " + Config.Hosts.ADFSFarm.FarmIdentifier;
                lbladmincontact.Text = "Administrative contact : " + Config.AdminContact;
                if (Config.UseActiveDirectory)
                    lblstorageMode.Text = "Mode : Active Directory";
                else
                    lblstorageMode.Text = "Mode : Sql Server Database";

                lblFarmBehavior.Text = "Behavior : " + Config.Hosts.ADFSFarm.CurrentFarmBehavior.ToString();

                // third col
                lbloptions.Text = "Options : ";
                if (Config.OTPProvider.Enabled)
                    lbloptions.Text += "TOPT ";
                if (Config.MailProvider.Enabled)
                    lbloptions.Text += "EMAILS ";
                if (Config.ExternalProvider.Enabled)
                    lbloptions.Text += "SMS ";
                if (Config.AzureProvider.Enabled)
                    lbloptions.Text += "AZURE ";

                switch (Config.KeysConfig.KeyFormat)
                {
                    case SecretKeyFormat.RSA:
                        lblSecurity.Text = "Security : RSA   " + Config.KeysConfig.CertificateThumbprint;
                        break;
                    case SecretKeyFormat.CUSTOM:
                        lblSecurity.Text = "Security : RSA CUSTOM";
                        break;
                    default:
                        lblSecurity.Text = "Security : RNG";
                        break;
                }
                if (Config.CustomUpdatePassword)
                    lblcutompwd.Text = "Use custom change password feature";
                else
                    lblcutompwd.Text = "";

                if (ManagementService.ADFSManager.IsMFAProviderEnabled(null))
                    tblconfigure.Text = res.CTRLADFSDEACTIVATEMFA;
                else
                    tblconfigure.Text = res.CTRLADFSACTIVATEMFA;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// tblconfigureLinkClicked method implmentation
        /// </summary>
        private void TblconfigureLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (!ManagementService.ADFSManager.IsRunning())
                    throw new Exception(res.CTRLMUSTACTIVATESVC);
                if (ManagementService.ADFSManager.IsMFAProviderEnabled(null))
                {
                    ManagementService.ADFSManager.DisableMFAProvider(null);
                    (sender as LinkLabel).Text = res.CTRLADFSACTIVATEMFA;
                }
                else
                {
                    ManagementService.ADFSManager.EnableMFAProvider(null);
                    (sender as LinkLabel).Text = res.CTRLADFSDEACTIVATEMFA;
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }
    }

    public partial class GeneralConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private bool iscustom = false;

        // Controls
        private TextBox txtIssuer;
        private TextBox txtAdminContact;
        private TextBox txtCountryCode;
        private ComboBox cbConfigTemplate;

        private RadioButton rdioMFARequired;
        private RadioButton rdioMFAAllowed;
        private RadioButton rdioMFANotRequired;
        private RadioButton rdioREGAdmin;
        private RadioButton rdioREGUser;
        private RadioButton rdioREGUnManaged;
        private RadioButton rdioREGREquired;
        private CheckBox chkAllowManageOptions;
        private CheckBox chkAllowChangePassword;
        private CheckBox chkAllowEnrollment;
        private CheckBox chkAllowKMSOO;
        private CheckBox chkAllowNotifications;
        private NumericUpDown txtADVStart;
        private NumericUpDown txtADVEnd;

        private Panel _paneloptmfa;
        private Panel _panelstmfa;
        private Panel _panelregmfa;
        private Panel _paneladvmfa;
        private GeneralViewControl _view;

        private ErrorProvider errors;
        private Label lblIssuer;
        private Label lblAdminContact;
        private Label lblContryCode;
        private Label lblConfigTemplate;
        private Label rdioMFALabel;
        private Label rdioREGLabel;
        private Label optCFGLabel;
        private Label optADVLabel;
        private Label beginADVLabel;
        private Label endADVLabel;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private ComboBox cbConfigProvider;
        private Label lblConfigProvider;
        private bool lockevents;
        private CheckBox chkUseUserLanguages;



        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public GeneralConfigurationControl(GeneralViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                     _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 590;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 470;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 470;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblIssuer = new Label
                {
                    Text = res.CTRLGLCOMANYNAME + " : ",
                    Left = 10,
                    Top = 19,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblIssuer);

                txtIssuer = new TextBox
                {
                    Text = Config.Issuer,
                    Left = 210,
                    Top = 15,
                    Width = 250
                };
                txtIssuer.Validating += IssuerValidating;
                txtIssuer.Validated += IssuerValidated;
                _txtpanel.Controls.Add(txtIssuer);

                lblAdminContact = new Label
                {
                    Text = res.CTRLGLCONTACT + " : ",
                    Left = 10,
                    Top = 51,
                    Width = 200
                };
                _txtpanel.Controls.Add(lblAdminContact);

                txtAdminContact = new TextBox
                {
                    Text = Config.AdminContact,
                    Left = 210,
                    Top = 47,
                    Width = 250
                };
                txtAdminContact.Validating += AdminContactValidating;
                txtAdminContact.Validated += AdminContactValidated;
                _txtpanel.Controls.Add(txtAdminContact);

                lblContryCode = new Label
                {
                    Text = res.CTRLGLCONTRYCODE + " : ",
                    Left = 10,
                    Top = 83,
                    Width = 130
                };
                _txtpanel.Controls.Add(lblContryCode);

                txtCountryCode = new TextBox
                {
                    Text = Config.DefaultCountryCode,
                    Left = 210,
                    Top = 79,
                    Width = 20,
                    TextAlign = HorizontalAlignment.Center,
                    MaxLength = 2,
                    CharacterCasing = CharacterCasing.Lower
                };
                txtCountryCode.Validating += CountryCodeValidating;
                txtCountryCode.Validated += CountryCodeValidated;
                _txtpanel.Controls.Add(txtCountryCode);

                lblConfigProvider = new Label
                {
                    Text = res.CTRLGLPROVIDER + " : ",
                    Left = 530,
                    Top = 83,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblConfigProvider);

                MMCProviersList xlst = new MMCProviersList();
                cbConfigProvider = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 690,
                    Top = 79,
                    Width = 80
                };
                _txtpanel.Controls.Add(cbConfigProvider);

                cbConfigProvider.DataSource = xlst;
                cbConfigProvider.ValueMember = "ID";
                cbConfigProvider.DisplayMember = "Label";
                cbConfigProvider.SelectedIndexChanged += SelectedProviderChanged;

                lblConfigTemplate = new Label
                {
                    Text = res.CTRLGLPOLICY + " : ",
                    Left = 10,
                    Top = 168,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblConfigTemplate);

                MMCTemplateModeList lst = new MMCTemplateModeList();
                cbConfigTemplate = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 210,
                    Top = 164,
                    Width = 250
                };
                _txtpanel.Controls.Add(cbConfigTemplate);

                cbConfigTemplate.DataSource = lst;
                cbConfigTemplate.ValueMember = "ID";
                cbConfigTemplate.DisplayMember = "Label";
                cbConfigTemplate.SelectedIndexChanged += SelectedPolicyTemplateChanged;

                _panelstmfa = new Panel
                {
                    Left = 0,
                    Top = 198,
                    Height = 125,
                    Width = 300
                };
                _txtpanel.Controls.Add(_panelstmfa);

                rdioMFALabel = new Label
                {
                    Text = res.CTRLGLMFASTATUS,
                    Left = 10,
                    Top = 10,
                    Width = 180
                };
                _panelstmfa.Controls.Add(rdioMFALabel);

                rdioMFARequired = new RadioButton
                {
                    Text = res.CTRLGLMFASTATUS1,
                    Left = 30,
                    Top = 29,
                    Width = 300
                };
                rdioMFARequired.CheckedChanged += MFARequiredCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFARequired);

                rdioMFAAllowed = new RadioButton
                {
                    Text = res.CTRLGLMFASTATUS2,
                    Left = 30,
                    Top = 54,
                    Width = 300
                };
                rdioMFAAllowed.CheckedChanged += MFAAllowedCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFAAllowed);

                rdioMFANotRequired = new RadioButton
                {
                    Text = res.CTRLGLMFASTATUS3,
                    Left = 30,
                    Top = 79,
                    Width = 300
                };
                rdioMFANotRequired.CheckedChanged += MFANotRequiredCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFANotRequired);

                _panelregmfa = new Panel
                {
                    Left = 0,
                    Top = 300,
                    Height = 135,
                    Width = 320
                };
                _txtpanel.Controls.Add(_panelregmfa);

                rdioREGLabel = new Label
                {
                    Text = res.CTRLGLMFAREGISTER,
                    Left = 10,
                    Top = 10,
                    Width = 180
                };
                _panelregmfa.Controls.Add(rdioREGLabel);

                rdioREGAdmin = new RadioButton
                {
                    Text = res.CTRLGLMFAREGISTER1,
                    Left = 30,
                    Top = 29,
                    Width = 300
                };
                rdioREGAdmin.CheckedChanged += REGAdminCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGAdmin);

                rdioREGUser = new RadioButton
                {
                    Text = res.CTRLGLMFAREGISTER2,
                    Left = 30,
                    Top = 54,
                    Width = 300
                };
                rdioREGUser.CheckedChanged += REGUserCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGUser);

                rdioREGUnManaged = new RadioButton
                {
                    Text = res.CTRLGLMFAREGISTER3,
                    Left = 30,
                    Top = 79,
                    Width = 300
                };
                rdioREGUnManaged.CheckedChanged += REGUnManagedCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGUnManaged);

                rdioREGREquired = new RadioButton
                {
                    Text = res.CTRLGLMFAREGISTER4,
                    Left = 30,
                    Top = 104,
                    Width = 300
                };
                rdioREGREquired.CheckedChanged += REGUAdministrativeCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGREquired);

                _paneloptmfa = new Panel
                {
                    Left = 530,
                    Top = 198,
                    Height = 190,
                    Width = 400
                };
                _txtpanel.Controls.Add(_paneloptmfa);

                optCFGLabel = new Label
                {
                    Text = res.CTRLGLMANAGEOPTS,
                    Left = 0,
                    Top = 10,
                    Width = 180
                };
                _paneloptmfa.Controls.Add(optCFGLabel);

                chkAllowManageOptions = new CheckBox
                {
                    Text = res.CTRLGLMANAGEOPTIONS,
                    Left = 20,
                    Top = 29,
                    Width = 300
                };
                chkAllowManageOptions.CheckedChanged += AllowManageOptionsCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowManageOptions);

                chkAllowEnrollment = new CheckBox
                {
                    Text = res.CTRLGLENROLLWIZ,
                    Left = 20,
                    Top = 54,
                    Width = 300
                };
                chkAllowEnrollment.CheckedChanged += AllowEnrollmentWizardCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowEnrollment);

                chkAllowChangePassword = new CheckBox
                {
                    Text = res.CTRLGLMANAGEPWD,
                    Left = 20,
                    Top = 79,
                    Width = 300
                };
                chkAllowChangePassword.CheckedChanged += AllowChangePasswordCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowChangePassword);

                chkAllowKMSOO = new CheckBox
                {
                    Text = res.CTRLGLMANAGEKMSOO,
                    Left = 20,
                    Top = 104,
                    Width = 400
                };
                chkAllowKMSOO.CheckedChanged += AllowKMSOOCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowKMSOO);

                chkAllowNotifications = new CheckBox
                {
                    Text = res.CTRLGLMANAGENOTIFS,
                    Left = 20,
                    Top = 129,
                    Width = 400
                };
                chkAllowNotifications.CheckedChanged += AllowNotificationsCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowNotifications);

                chkUseUserLanguages = new CheckBox
                {
                    Text = res.CTRLGLUSEUSERLANGS,
                    Checked = Config.UseOfUserLanguages,
                    Left = 20,
                    Top = 154,
                    Width = 400
                };
                chkUseUserLanguages.CheckedChanged += UseUserlanguagesCheckedChanged;
                _paneloptmfa.Controls.Add(chkUseUserLanguages);

                _paneladvmfa = new Panel
                {
                    Left = 530,
                    Top = 385,
                    Height = 100,
                    Width = 325
                };
                _txtpanel.Controls.Add(_paneladvmfa);

                optADVLabel = new Label
                {
                    Text = res.CTRGLMANAGEREG,
                    Left = 0,
                    Top = 10,
                    Width = 160
                };
                _paneladvmfa.Controls.Add(optADVLabel);

                beginADVLabel = new Label
                {
                    Text = res.CTRGLMANAGEREGSTART + " :",
                    Left = 20,
                    Top = 34,
                    Width = 50,
                    TextAlign = ContentAlignment.MiddleRight
                };
                _paneladvmfa.Controls.Add(beginADVLabel);

                endADVLabel = new Label
                {
                    Text = res.CTRGLMANAGEREGEND + " :",
                    Left = 150,
                    Top = 34,
                    Width = 50,
                    TextAlign = ContentAlignment.MiddleRight
                };
                _paneladvmfa.Controls.Add(endADVLabel);

                txtADVStart = new NumericUpDown
                {
                    Left = 70,
                    Top = 34,
                    Width = 50,
                    TextAlign = HorizontalAlignment.Center,
                    Value = Config.AdvertisingDays.FirstDay,
                    Minimum = new decimal(new int[] { 1, 0, 0, 0 }),
                    Maximum = new decimal(new int[] { 31, 0, 0, 0 })
                };
                txtADVStart.ValueChanged += ADVStartValueChanged;
                _paneladvmfa.Controls.Add(txtADVStart);

                txtADVEnd = new NumericUpDown
                {
                    Left = 200,
                    Top = 34,
                    Width = 50,
                    TextAlign = HorizontalAlignment.Center,
                    Value = Config.AdvertisingDays.LastDay,
                    Minimum = new decimal(new int[] { 1, 0, 0, 0 }),
                    Maximum = new decimal(new int[] { 31, 0, 0, 0 })
                };
                txtADVEnd.ValueChanged += ADVEndValueChanged;
                _paneladvmfa.Controls.Add(txtADVEnd);


                tblSaveConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE,
                    Left = 20,
                    Top = 485,
                    Width = 80,
                    TabStop = true
                };
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL,
                    Left = 110,
                    Top = 485,
                    Width = 80,
                    TabStop = true
                };
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view)
                {
                    BlinkStyle = ErrorBlinkStyle.NeverBlink
                };
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                AdjustComboboxTemplates();
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtIssuer.Text = Config.Issuer;
                txtAdminContact.Text = Config.AdminContact;
                txtCountryCode.Text = Config.DefaultCountryCode;
                txtADVStart.Value = Config.AdvertisingDays.FirstDay;
                txtADVEnd.Value = Config.AdvertisingDays.LastDay;
                chkUseUserLanguages.Checked = Config.UseOfUserLanguages;

                lblIssuer.Text = res.CTRLGLCOMANYNAME + " : ";
                lblAdminContact.Text = res.CTRLGLCONTACT + " : ";
                lblContryCode.Text = res.CTRLGLCONTRYCODE + " : ";
                lblConfigProvider.Text = res.CTRLGLPROVIDER + " : ";
                lblConfigTemplate.Text = res.CTRLGLPOLICY + " : ";
                rdioMFALabel.Text = res.CTRLGLMFASTATUS;
                rdioMFARequired.Text = res.CTRLGLMFASTATUS1;
                rdioMFAAllowed.Text = res.CTRLGLMFASTATUS2;
                rdioMFANotRequired.Text = res.CTRLGLMFASTATUS3;
                rdioREGLabel.Text = res.CTRLGLMFAREGISTER;
                rdioREGAdmin.Text = res.CTRLGLMFAREGISTER1;
                rdioREGUser.Text = res.CTRLGLMFAREGISTER2;
                rdioREGUnManaged.Text = res.CTRLGLMFAREGISTER3;
                rdioREGREquired.Text = res.CTRLGLMFAREGISTER4;  
                optCFGLabel.Text = res.CTRLGLMANAGEOPTS;
                chkAllowManageOptions.Text = res.CTRLGLMANAGEOPTIONS;
                chkAllowEnrollment.Text = res.CTRLGLENROLLWIZ;
                chkAllowChangePassword.Text = res.CTRLGLMANAGEPWD;
                chkAllowKMSOO.Text = res.CTRLGLMANAGEKMSOO;
                chkAllowNotifications.Text = res.CTRLGLMANAGENOTIFS;
                chkUseUserLanguages.Text = res.CTRLGLUSEUSERLANGS;
                optADVLabel.Text = res.CTRGLMANAGEREG;
                beginADVLabel.Text = res.CTRGLMANAGEREGSTART + " :";
                endADVLabel.Text = res.CTRGLMANAGEREGEND + " :";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                AdjustComboboxTemplates();
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implementation
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtCountryCode.Text))
                errors.SetError(txtCountryCode, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCountryCode, "");

            if (string.IsNullOrEmpty(txtAdminContact.Text))
                errors.SetError(txtAdminContact, res.CTRLNULLOREMPTYERROR);
            else if (!MMCService.IsValidEmail(txtAdminContact.Text))
                errors.SetError(txtAdminContact, res.CTRLGLINVALIDEMAILCONTACT);
            else
                errors.SetError(txtAdminContact, "");

            if (string.IsNullOrEmpty(txtIssuer.Text))
                errors.SetError(txtIssuer, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtIssuer, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
           if (_txtpanel != null)
               _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// SelectedTemplateChanged method implementation
        /// </summary>
        private void SelectedPolicyTemplateChanged(object sender, EventArgs e)
        {
            SetPolicyTemplate();
        }

        /// <summary>
        /// SelectedProviderChanged method implementation
        /// </summary>
        private void SelectedProviderChanged(object sender, EventArgs e)
        {
            MMCProviderItem itm = (MMCProviderItem)cbConfigProvider.SelectedItem;
            Config.DefaultProviderMethod = itm.ID;
            if (_view.AutoValidate != AutoValidate.Disable)
                ManagementService.ADFSManager.SetDirty(true);
        }

        /// <summary>
        /// SetPolicyTemplate method implmentation
        /// </summary>
        private void SetPolicyTemplate()
        {
            MMCTemplateModeItem itm = (MMCTemplateModeItem)cbConfigTemplate.SelectedItem;
            Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(itm.ID);
            iscustom = (itm.ID == UserTemplateMode.Custom);
            if ((itm.ID != UserTemplateMode.Administrative) && (itm.ID != UserTemplateMode.Custom))
                Config.KeepMySelectedOptionOn = true;
            else
                Config.KeepMySelectedOptionOn = false;
            UpdateLayoutPolicyComponents(itm.ID);
            if (_view.AutoValidate != AutoValidate.Disable)
                ManagementService.ADFSManager.SetDirty(true);
        }

        /// <summary>
        /// GetPolicyTemplate method implementation
        /// </summary>
        private UserTemplateMode GetPolicyTemplate()
        {
            if (iscustom)
                return UserTemplateMode.Custom;
            else
                return Config.UserFeatures.GetPolicyTemplate();
        }

        /// <summary>
        /// AdjustComboboxTemplates method implmentation
        /// </summary>
        private void AdjustComboboxTemplates()
        {
            UserTemplateMode template = Config.UserFeatures.GetPolicyTemplate();
            cbConfigTemplate.SelectedIndex = cbConfigTemplate.Items.IndexOf(template);
            UpdateLayoutPolicyComponents(template);

            PreferredMethod method = Config.DefaultProviderMethod;
            cbConfigProvider.SelectedIndex = cbConfigProvider.Items.IndexOf(method);
        }
        
        /// <summary>
        /// UpdateLayoutPolicyComponents method implmentation
        /// </summary>
        private void UpdateLayoutPolicyComponents(UserTemplateMode mode)
        {
            lockevents = true;
            try
            {
                rdioMFARequired.Enabled = false;
                rdioMFAAllowed.Enabled = false;
                rdioMFANotRequired.Enabled = false;

                rdioREGAdmin.Enabled = false;
                rdioREGUnManaged.Enabled = false;
                rdioREGUser.Enabled = false;
                rdioREGREquired.Enabled = false;

                switch (mode)
                {
                    case UserTemplateMode.Free:
                        rdioMFARequired.Checked = false;
                        rdioMFAAllowed.Checked = false;
                        rdioMFANotRequired.Checked = true;

                        rdioREGAdmin.Checked = false;
                        rdioREGUser.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioREGUnManaged.Checked = true;

                        chkAllowManageOptions.Checked = true;
                        chkAllowEnrollment.Checked = true;
                        chkAllowChangePassword.Checked = true;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = false;
                        txtADVEnd.Enabled = false;

                        break;
                    case UserTemplateMode.Open:
                        rdioMFARequired.Checked = false;
                        rdioMFAAllowed.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioMFANotRequired.Checked = true;

                        rdioREGAdmin.Checked = false;
                        rdioREGUnManaged.Checked = false;
                        rdioREGUser.Checked = true;

                        chkAllowManageOptions.Checked = true;
                        chkAllowEnrollment.Checked = true;
                        chkAllowChangePassword.Checked = true;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = true;
                        txtADVEnd.Enabled = true;

                        break;
                    case UserTemplateMode.Default:
                        rdioMFARequired.Checked = false;
                        rdioMFANotRequired.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioMFAAllowed.Checked = true;

                        rdioREGAdmin.Checked = false;
                        rdioREGUnManaged.Checked = false;
                        rdioREGUser.Checked = true;

                        chkAllowManageOptions.Checked = true;
                        chkAllowEnrollment.Checked = true;
                        chkAllowChangePassword.Checked = true;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = true;
                        txtADVEnd.Enabled = true;

                        break;
                    case UserTemplateMode.Mixed:
                        rdioMFANotRequired.Checked = false;
                        rdioMFAAllowed.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioMFARequired.Checked = true;

                        rdioREGAdmin.Checked = false;
                        rdioREGUnManaged.Checked = false;
                        rdioREGUser.Checked = true;

                        chkAllowManageOptions.Checked = true;
                        chkAllowEnrollment.Checked = true;
                        chkAllowChangePassword.Checked = true;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = false;
                        txtADVEnd.Enabled = false;
                        break;
                    case UserTemplateMode.Managed:
                        rdioMFAAllowed.Checked = false;
                        rdioMFARequired.Checked = false;
                        rdioMFANotRequired.Checked = true;

                        rdioREGUnManaged.Checked = false;
                        rdioREGUser.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioREGAdmin.Checked = true;

                        chkAllowManageOptions.Checked = false;
                        chkAllowEnrollment.Checked = false;
                        chkAllowChangePassword.Checked = true;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = false;
                        txtADVEnd.Enabled = false;

                        break;
                    case UserTemplateMode.Strict:
                        rdioMFAAllowed.Checked = false;
                        rdioMFANotRequired.Checked = false;
                        rdioMFARequired.Checked = true;

                        rdioREGUnManaged.Checked = false;
                        rdioREGUser.Checked = false;
                        rdioREGREquired.Checked = true;
                        rdioREGAdmin.Checked = true;

                        chkAllowManageOptions.Checked = false;
                        chkAllowEnrollment.Checked = false;
                        chkAllowChangePassword.Checked = false;
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = false;
                        txtADVEnd.Enabled = false;

                        break;
                    case UserTemplateMode.Administrative:
                        rdioMFAAllowed.Checked = false;
                        rdioMFANotRequired.Checked = false;
                        rdioMFARequired.Checked = true;

                        rdioREGUser.Checked = false;
                        rdioREGAdmin.Checked = false;
                        rdioREGUnManaged.Checked = false;
                        rdioREGREquired.Checked = true;

                        chkAllowManageOptions.Checked = false;
                        chkAllowEnrollment.Checked = false;
                        chkAllowChangePassword.Checked = false;
                        chkAllowKMSOO.Checked = false;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        chkAllowManageOptions.Enabled = false;
                        chkAllowEnrollment.Enabled = false;
                        chkAllowChangePassword.Enabled = false;
                        chkAllowKMSOO.Enabled = false;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = false;
                        txtADVEnd.Enabled = false;

                        break;
                    default:

                        rdioMFARequired.Checked = Config.UserFeatures.IsMFARequired();
                        rdioMFANotRequired.Checked = Config.UserFeatures.IsMFANotRequired();
                        rdioMFAAllowed.Checked = Config.UserFeatures.IsMFAAllowed(); 

                        rdioREGAdmin.Checked = Config.UserFeatures.IsRegistrationRequired();
                        rdioREGUnManaged.Checked = Config.UserFeatures.IsRegistrationNotRequired();
                        rdioREGUser.Checked = Config.UserFeatures.IsRegistrationAllowed();
                        rdioREGREquired.Checked = Config.UserFeatures.IsAdministrative();

                        chkAllowManageOptions.Checked = Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions); 
                        chkAllowEnrollment.Checked = Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowEnrollment);
                        chkAllowChangePassword.Checked = Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowChangePassword);
                        chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;
                        chkAllowNotifications.Checked = Config.ChangeNotificationsOn;

                        rdioMFARequired.Enabled = true;
                        rdioMFAAllowed.Enabled = true;
                        rdioMFANotRequired.Enabled = true;

                        rdioREGAdmin.Enabled = true;
                        rdioREGUnManaged.Enabled = true;
                        rdioREGUser.Enabled = true;
                        rdioREGREquired.Enabled = true;

                        chkAllowManageOptions.Enabled = true;
                        chkAllowEnrollment.Enabled = true;
                        chkAllowChangePassword.Enabled = true;
                        chkAllowKMSOO.Enabled = true;
                        chkAllowNotifications.Enabled = true;

                        txtADVStart.Enabled = true;
                        txtADVEnd.Enabled = true;
                        break;
                }
            }
            finally
            {
                lockevents = false;
            }
        }

        #region Features mgmt
        /// <summary>
        /// REGUAdministrativeCheckedChanged event
        /// </summary>
        private void REGUAdministrativeCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGREquired.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetMandatoryRegistration();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// REGUnManagedCheckedChanged event
        /// </summary>
        private void REGUnManagedCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGUnManaged.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetUnManagedRegistration();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// REGUserCheckedChanged event
        /// </summary>
        private void REGUserCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGUser.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetSelfRegistration();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// REGAdminCheckedChanged event
        /// </summary>
        private void REGAdminCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGAdmin.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetAdministrativeRegistration();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFANotRequiredCheckedChanged event
        /// </summary>
        private void MFANotRequiredCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFANotRequired.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetMFANotRequired();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFAAllowedCheckedChanged event
        /// </summary>
        private void MFAAllowedCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFAAllowed.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetMFAAllowed();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFARequiredCheckedChanged event
        /// </summary>
        private void MFARequiredCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFARequired.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.MMCSetMFARequired();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowChangePasswordCheckedChanged method implmentation
        /// </summary>
        private void AllowChangePasswordCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowChangePassword.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowChangePassword);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowChangePassword);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowManageOptionsCheckedChanged method implmentation
        /// </summary>
        private void AllowManageOptionsCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowManageOptions.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowManageOptions);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowManageOptions);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowManageOptionsCheckedChanged method implmentation
        /// </summary>
        private void AllowEnrollmentWizardCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowEnrollment.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowEnrollment);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowEnrollment);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowKMSOOCheckedChanged method implementation
        /// </summary>
        private void AllowKMSOOCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeepMySelectedOptionOn = chkAllowKMSOO.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowNotificationsCheckedChanged method implementation
        /// </summary>
        private void AllowNotificationsCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.ChangeNotificationsOn = chkAllowNotifications.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseUserlanguagesCheckedChanged method implementation
        /// </summary>
        private void UseUserlanguagesCheckedChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.UseOfUserLanguages = chkUseUserLanguages.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Advertising
        /// <summary>
        /// ADVEndValueChanged method implementation
        /// </summary>
        private void ADVEndValueChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.AdvertisingDays.LastDay = Convert.ToUInt32(txtADVEnd.Value);
                    errors.SetError(txtADVEnd, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(txtADVEnd, ex.Message);
            }
        }

        /// <summary>
        /// ADVStartValueChanged method implementation
        /// </summary>
        private void ADVStartValueChanged(object sender, EventArgs e)
        {
            if (lockevents)
                return;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.AdvertisingDays.FirstDay = Convert.ToUInt32(txtADVStart.Value);
                    errors.SetError(txtADVStart, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(txtADVStart, ex.Message);
            }
        }
        #endregion

        #region CountryCode
        /// <summary>
        /// CountryCodeValidating event
        /// </summary>
        private void CountryCodeValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtCountryCode.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCountryCode.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                    errors.SetError(txtCountryCode, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCountryCode, ex.Message);
            }
        }

        /// <summary>
        /// CountryCodeValidated event
        /// </summary>
        private void CountryCodeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCountryCode, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region AdminContact
        /// <summary>
        /// AdminContactTextChanged event
        /// </summary>
        private void AdminContactValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtAdminContact.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtAdminContact.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!MMCService.IsValidEmail(txtAdminContact.Text))
                        throw new Exception(res.CTRLGLINVALIDEMAILCONTACT);
                    Config.AdminContact = txtAdminContact.Text;
                    errors.SetError(txtAdminContact, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtAdminContact, ex.Message);
            }
        }

        /// <summary>
        /// AdminContactValidated event
        /// </summary>
        private void AdminContactValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AdminContact = txtAdminContact.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtAdminContact, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Issuer
        /// <summary>
        /// IssuerValidating event
        /// </summary>
        private void IssuerValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtIssuer.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtIssuer.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.Issuer = txtIssuer.Text;
                    errors.SetError(txtIssuer, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtIssuer, ex.Message);
            }
        }

        /// <summary>
        /// IssuerValidated event
        /// </summary>
        private void IssuerValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Issuer = txtIssuer.Text; 
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtIssuer, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class ADDSConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ADDSViewControl _view;
        private CheckBox chkUseADDS;
        private TextBox txtDomainName;
        private bool _UpdateControlsLayouts;
        private TextBox txtUserName;
        private TextBox txtPassword;
        private TextBox txtKeyAttribute;
        private TextBox txtMailAttribute;
        private TextBox txtPhoneAttribute;
        private TextBox txtMethodAttribute;
        private TextBox txtOverrideMethodAttribute;
        private TextBox txtPinAttribute;
        private TextBox txtRSACertAttribute;
        private TextBox txtEnabledAttribute;
        private TextBox txtPublicKeyAttribute;
        private TextBox txtMaxRows;
        private Button btnConnect;
        private ErrorProvider errors;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblMaxRows;
        private Label lblEnableAttribute;
        private Label lblValiditydateAttribute;
        private Label lblCreatedateAttribute;
        private Label lblMethodAttribute;
        private Label lblPhoneAttribute;
        private Label lblMailAttribute;
        private Label lblKeyAttribute;
        private Label lblAttributes;
        private Label lblPassword;
        private Label lblUserName;
        private Label lblDomainName;
        private Label lblMultivalued;
        private Label lblPublicKeyAttribute;
        private Label lblMustMultivalued;
        private Button btnTemplateBase;
        private Button btnTemplate2016;
        private Button btnTemplateMFA;
        private Label lblRSACertAttribute;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ADDSConfigurationControl(ADDSViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    if (Config.UseActiveDirectory)
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 670;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 550;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 550;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                chkUseADDS = new CheckBox
                {
                    Text = res.CTRLADUSEADDS,
                    Checked = Config.UseActiveDirectory,
                    Left = 10,
                    Top = 19,
                    Width = 450
                };
                chkUseADDS.CheckedChanged += UseADDSCheckedChanged;
                _txtpanel.Controls.Add(chkUseADDS);

                lblDomainName = new Label
                {
                    Text = res.CTRLADDOMAIN + " : ",
                    Left = 50,
                    Top = 51,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblDomainName);

                txtDomainName = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.DomainAddress,
                    Left = 210,
                    Top = 47,
                    Width = 230,
                    Enabled = Config.UseActiveDirectory
                };
                txtDomainName.Validating += DomainNameValidating;
                txtDomainName.Validated += DomainNameValidated;
                _txtpanel.Controls.Add(txtDomainName);

                lblUserName = new Label
                {
                    Text = res.CTRLADACCOUNT + " : ",
                    Left = 480,
                    Top = 51,
                    Width = 100
                };
                _txtpanel.Controls.Add(lblUserName);

                txtUserName = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.Account,
                    Left = 580,
                    Top = 47,
                    Width = 230,
                    Enabled = Config.UseActiveDirectory
                };
                txtUserName.Validating += UserNameValidating;
                txtUserName.Validated += UserNameValidated;
                _txtpanel.Controls.Add(txtUserName);

                lblPassword = new Label
                {
                    Text = res.CTRLADPASSWORD + " : ",
                    Left = 480,
                    Top = 82,
                    Width = 85
                };
                _txtpanel.Controls.Add(lblPassword);

                txtPassword = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.Password,
                    Left = 580,
                    Top = 78,
                    Width = 230,
                    PasswordChar = '*',
                    Enabled = Config.UseActiveDirectory
                };
                txtPassword.Validating += PasswordValidating;
                txtPassword.Validated += PasswordValidated;
                _txtpanel.Controls.Add(txtPassword);

                btnConnect = new Button
                {
                    Text = res.CTRLADTEST,
                    Left = 580,
                    Top = 109,
                    Width = 230,
                    Enabled = Config.UseActiveDirectory
                };
                btnConnect.Click += BtnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                lblAttributes = new Label
                {
                    Text = res.CTRLADATTRIBUTES + " : ",
                    Left = 30,
                    Top = 119,
                    Width = 300
                };
                _txtpanel.Controls.Add(lblAttributes);

                lblKeyAttribute = new Label
                {
                    Text = res.CTRLADATTKEY + " : ",
                    Left = 50,
                    Top = 150,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblKeyAttribute);

                txtKeyAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.KeyAttribute,
                    Left = 210,
                    Top = 146,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtKeyAttribute.Validating += KeyAttributeValidating;
                txtKeyAttribute.Validated += KeyAttributeValidated;
                _txtpanel.Controls.Add(txtKeyAttribute);

                lblMailAttribute = new Label
                {
                    Text = res.CTRLADATTMAIL + " (*) : ",
                    Left = 50,
                    Top = 181,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblMailAttribute);

                txtMailAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.MailAttribute,
                    Left = 210,
                    Top = 177,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtMailAttribute.Validating += MailAttributeValidating;
                txtMailAttribute.Validated += MailAttributeValidated;
                _txtpanel.Controls.Add(txtMailAttribute);

                lblPhoneAttribute = new Label
                {
                    Text = res.CTRLADATTPHONE + " (*) : ",
                    Left = 50,
                    Top = 212,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblPhoneAttribute);

                txtPhoneAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.PhoneAttribute,
                    Left = 210,
                    Top = 208,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtPhoneAttribute.Validating += PhoneAttributeValidating;
                txtPhoneAttribute.Validated += PhoneAttributeValidated;
                _txtpanel.Controls.Add(txtPhoneAttribute);

                lblMethodAttribute = new Label
                {
                    Text = res.CTRLADATTMETHOD + " : ",
                    Left = 50,
                    Top = 243,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblMethodAttribute);

                txtMethodAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.MethodAttribute,
                    Left = 210,
                    Top = 239,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtMethodAttribute.Validating += MethodAttributeValidating;
                txtMethodAttribute.Validated += MethodAttributeValidated;
                _txtpanel.Controls.Add(txtMethodAttribute);

                lblCreatedateAttribute = new Label
                {
                    Text = res.CTRLADATTOVERRIDE + " : ",
                    Left = 50,
                    Top = 274,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblCreatedateAttribute);

                txtOverrideMethodAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.OverrideMethodAttribute,
                    Left = 210,
                    Top = 270,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtOverrideMethodAttribute.Validating += OverrideMethodAttributeValidating;
                txtOverrideMethodAttribute.Validated += OverrideMethodAttributeValidated;
                _txtpanel.Controls.Add(txtOverrideMethodAttribute);

                lblValiditydateAttribute = new Label
                {
                    Text = res.CTRLADATTPIN + " : ",
                    Left = 50,
                    Top = 305,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblValiditydateAttribute);

                txtPinAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.PinAttribute,
                    Left = 210,
                    Top = 301,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtPinAttribute.Validating += PinAttributeValidating;
                txtPinAttribute.Validated += PinAttributeValidated;
                _txtpanel.Controls.Add(txtPinAttribute);

                lblPublicKeyAttribute = new Label
                {
                    Text = res.CTRLADATTPUBLICKEY + " (**) : ",
                    Left = 50,
                    Top = 336,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblPublicKeyAttribute);

                txtPublicKeyAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.PublicKeyCredentialAttribute,
                    Left = 210,
                    Top = 332,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtPublicKeyAttribute.Validating += CheckPublicKeyAttributeValidating;
                txtPublicKeyAttribute.Validated += CheckPublicKeyAttributeValidated;
                _txtpanel.Controls.Add(txtPublicKeyAttribute);

                lblRSACertAttribute = new Label
                {
                    Text = res.CTRLADATTRSACERT + " : ",
                    Left = 50,
                    Top = 367,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblRSACertAttribute); 

                txtRSACertAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.RSACertificateAttribute,
                    Left = 210,
                    Top = 363,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtRSACertAttribute.Validating += RSACertAttributeValidating;
                txtRSACertAttribute.Validated += RSACertAttributeValidated;
                _txtpanel.Controls.Add(txtRSACertAttribute);

                lblEnableAttribute = new Label
                {
                    Text = res.CTRLADATTSTATUS + " : ",
                    Left = 50,
                    Top = 398,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblEnableAttribute);

                txtEnabledAttribute = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.TotpEnabledAttribute,
                    Left = 210,
                    Top = 394,
                    Width = 600,
                    Enabled = Config.UseActiveDirectory
                };
                txtEnabledAttribute.Validating += EnabledAttributeValidating;
                txtEnabledAttribute.Validated += EnabledAttributeValidated;
                _txtpanel.Controls.Add(txtEnabledAttribute);

                lblMaxRows = new Label
                {
                    Text = res.CTRLSQLMAXROWS + " : ",
                    Left = 50,
                    Top = 429,
                    Width = 150
                };
                _txtpanel.Controls.Add(lblMaxRows);

                txtMaxRows = new TextBox
                {
                    Text = Config.Hosts.ActiveDirectoryHost.MaxRows.ToString(),
                    Left = 210,
                    Top = 425,
                    Width = 50,
                    TextAlign = HorizontalAlignment.Right,
                    Enabled = Config.UseActiveDirectory
                };
                txtMaxRows.Validating += MaxRowsValidating;
                txtMaxRows.Validated += MaxRowsValidated;
                _txtpanel.Controls.Add(txtMaxRows);


                btnTemplateBase = new Button
                {
                    Text = res.CTRLADTEMPLATEBASE,
                    Left = 210,
                    Top = 465,
                    Width = 150,
                    Enabled = Config.UseActiveDirectory
                };
                btnTemplateBase.Click += BtnTemplateBaseClick;
                _txtpanel.Controls.Add(btnTemplateBase);

                btnTemplate2016 = new Button
                {
                    Text = res.CTRLADTEMPLATE2016,
                    Left = 400,
                    Top = 465,
                    Width = 150,
                    Enabled = Config.UseActiveDirectory
                };
                btnTemplate2016.Click += BtnTemplate2016Click;
                _txtpanel.Controls.Add(btnTemplate2016);

                btnTemplateMFA = new Button
                {
                    Text = res.CTRLADTEMPLATEMFA,
                    Left = 590,
                    Top = 465,
                    Width = 150,
                    Enabled = Config.UseActiveDirectory
                };
                btnTemplateMFA.Click += BtnTemplateMFAClick;
                _txtpanel.Controls.Add(btnTemplateMFA);

                lblMultivalued = new Label()
                {
                    Text = res.CTRLMULTVALUED,
                    Left = 50,
                    Top = 500,
                    Width = 350
                };
                _txtpanel.Controls.Add(lblMultivalued);

                lblMustMultivalued = new Label()
                {
                    Text = res.CTRLMUSTMULTVALUED,
                    Left = 50,
                    Top = 525,
                    Width = 350
                };
                _txtpanel.Controls.Add(lblMustMultivalued);

                tblSaveConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE,
                    Left = 20,
                    Top = 560,
                    Width = 80,
                    TabStop = true
                };
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL,
                    Left = 110,
                    Top = 560,
                    Width = 80,
                    TabStop = true
                };
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                chkUseADDS.Checked = Config.UseActiveDirectory;

                txtDomainName.Text = Config.Hosts.ActiveDirectoryHost.DomainAddress;
                txtDomainName.Enabled = Config.UseActiveDirectory;

                txtUserName.Text = Config.Hosts.ActiveDirectoryHost.Account;
                txtUserName.Enabled = Config.UseActiveDirectory;

                txtPassword.Text = Config.Hosts.ActiveDirectoryHost.Password;
                txtPassword.Enabled = Config.UseActiveDirectory;

                btnConnect.Enabled = Config.UseActiveDirectory;

                txtKeyAttribute.Text = Config.Hosts.ActiveDirectoryHost.KeyAttribute;
                txtKeyAttribute.Enabled = Config.UseActiveDirectory;

                txtMailAttribute.Text = Config.Hosts.ActiveDirectoryHost.MailAttribute;
                txtMailAttribute.Enabled = Config.UseActiveDirectory;

                txtPhoneAttribute.Text = Config.Hosts.ActiveDirectoryHost.PhoneAttribute;
                txtPhoneAttribute.Enabled = Config.UseActiveDirectory;

                txtMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.MethodAttribute;
                txtMethodAttribute.Enabled = Config.UseActiveDirectory;

                txtOverrideMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.OverrideMethodAttribute;
                txtOverrideMethodAttribute.Enabled = Config.UseActiveDirectory;

                txtPinAttribute.Text = Config.Hosts.ActiveDirectoryHost.PinAttribute;
                txtPinAttribute.Enabled = Config.UseActiveDirectory;

                txtPublicKeyAttribute.Text = Config.Hosts.ActiveDirectoryHost.PublicKeyCredentialAttribute;
                txtPublicKeyAttribute.Enabled = Config.UseActiveDirectory;

                txtRSACertAttribute.Text = Config.Hosts.ActiveDirectoryHost.RSACertificateAttribute;
                txtRSACertAttribute.Enabled = Config.UseActiveDirectory;

                txtEnabledAttribute.Text = Config.Hosts.ActiveDirectoryHost.TotpEnabledAttribute;
                txtEnabledAttribute.Enabled = Config.UseActiveDirectory;

                txtMaxRows.Text = Config.Hosts.ActiveDirectoryHost.MaxRows.ToString();
                txtMaxRows.Enabled = Config.UseActiveDirectory;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblEnableAttribute.Text = res.CTRLADATTSTATUS + " : ";
                lblValiditydateAttribute.Text = res.CTRLADATTPIN + " : ";
                lblCreatedateAttribute.Text = res.CTRLADATTOVERRIDE + " : ";
                lblMethodAttribute.Text = res.CTRLADATTMETHOD + " : ";
                lblPhoneAttribute.Text = res.CTRLADATTPHONE + " (*) : ";
                lblMailAttribute.Text = res.CTRLADATTMAIL + " (*) : ";
                lblKeyAttribute.Text = res.CTRLADATTKEY + " : ";
                lblAttributes.Text = res.CTRLADATTRIBUTES + " : ";
                lblPublicKeyAttribute.Text = res.CTRLADATTPUBLICKEY + " (**) : ";
                lblRSACertAttribute.Text = res.CTRLADATTRSACERT + " : ";
                lblUserName.Text = res.CTRLADACCOUNT + " : ";
                lblDomainName.Text = res.CTRLADDOMAIN + " : ";
                btnConnect.Text = res.CTRLADTEST;
                btnTemplateBase.Text = res.CTRLADTEMPLATEBASE;
                btnTemplate2016.Text = res.CTRLADTEMPLATE2016;
                btnTemplateMFA.Text = res.CTRLADTEMPLATEMFA;
                chkUseADDS.Text = res.CTRLADUSEADDS;
                lblMultivalued.Text = res.CTRLMULTVALUED;
                lblMustMultivalued.Text = res.CTRLMUSTMULTVALUED;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implementation
        /// </summary>
        private void ValidateData()
        {
            if (chkUseADDS.Checked)
            {
                if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                {
                    errors.SetError(txtDomainName, res.CTRLADATTDOMAIN);
                    errors.SetError(txtUserName, res.CTRLADATTACCOUNT);
                    errors.SetError(txtPassword, res.CTRLADATTPASSWORD);
                }
                else
                {
                    errors.SetError(txtDomainName, "");
                    errors.SetError(txtUserName, "");
                    errors.SetError(txtPassword, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtOverrideMethodAttribute.Text, 0))
                        errors.SetError(txtOverrideMethodAttribute, res.CTRLADATOVERRIDEERROR);
                    else
                        errors.SetError(txtOverrideMethodAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtEnabledAttribute.Text, 0))
                        errors.SetError(txtEnabledAttribute, res.CTRLADATENABLEDERROR);
                    else
                        errors.SetError(txtEnabledAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtKeyAttribute.Text, 0))
                        errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
                    else
                        errors.SetError(txtKeyAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMailAttribute.Text, 1))
                        errors.SetError(txtMailAttribute, res.CTRLADATTEMAILERROR);
                    else
                        errors.SetError(txtMailAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMethodAttribute.Text, 0))
                        errors.SetError(txtMethodAttribute, res.CTRLADATMETHODERROR);
                    else
                        errors.SetError(txtMethodAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPhoneAttribute.Text, 1))
                        errors.SetError(txtPhoneAttribute, res.CTRLADATTPHONEERROR);
                    else
                        errors.SetError(txtPhoneAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPinAttribute.Text, 0))
                        errors.SetError(txtPinAttribute, res.CTRLADATPINERROR);
                    else
                        errors.SetError(txtPinAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPublicKeyAttribute.Text, 2))
                        errors.SetError(txtPublicKeyAttribute, res.CTRLADATTKEYERROR);
                    else
                        errors.SetError(txtPublicKeyAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtRSACertAttribute.Text, 0))
                        errors.SetError(txtRSACertAttribute, res.CTRLADATRSACERTERROR);
                    else
                        errors.SetError(txtRSACertAttribute, "");

                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        errors.SetError(txtMaxRows, String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                    else
                        errors.SetError(txtMaxRows, "");
                }
            }
            else
            {
                errors.SetError(txtDomainName, "");
                errors.SetError(txtUserName, "");
                errors.SetError(txtPassword, "");
                errors.SetError(txtOverrideMethodAttribute, "");
                errors.SetError(txtEnabledAttribute, "");
                errors.SetError(txtKeyAttribute, "");
                errors.SetError(txtMailAttribute, "");
                errors.SetError(txtMethodAttribute, "");
                errors.SetError(txtPhoneAttribute, "");
                errors.SetError(txtPinAttribute, "");
                errors.SetError(txtRSACertAttribute, "");
                errors.SetError(txtPublicKeyAttribute, "");
                errors.SetError(txtMaxRows, "");
            }
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtDomainName.Enabled = isenabled;
                txtUserName.Enabled = isenabled;
                txtPassword.Enabled = isenabled;
                txtOverrideMethodAttribute.Enabled = isenabled;
                txtEnabledAttribute.Enabled = isenabled;
                txtKeyAttribute.Enabled = isenabled;
                txtMailAttribute.Enabled = isenabled;
                txtMethodAttribute.Enabled = isenabled;
                txtPhoneAttribute.Enabled = isenabled;
                txtRSACertAttribute.Enabled = isenabled;
                txtPinAttribute.Enabled = isenabled;
                txtPublicKeyAttribute.Enabled = isenabled;
                txtMaxRows.Enabled = isenabled;
                btnConnect.Enabled = isenabled;
                btnTemplateBase.Enabled = isenabled;
                btnTemplate2016.Enabled = isenabled;
                btnTemplateMFA.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UseAADDSCheckedChanged method implementation
        /// </summary>
        private void UseADDSCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.UseActiveDirectory = chkUseADDS.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    DoRefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #region DomainName
        /// <summary>
        /// DomainNameValidating method implementation
        /// </summary>
        private void DomainNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtDomainName.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                        throw new Exception(res.CTRLADATTDOMAIN);
                    Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                    errors.SetError(txtDomainName, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtDomainName, res.CTRLADATTDOMAIN);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DomainNameValidated method implmentation
        /// </summary>
        private void DomainNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDomainName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Username
        /// <summary>
        /// UserNameValidating method implementation
        /// </summary>
        private void UserNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtUserName.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                       throw new Exception(res.CTRLADATTACCOUNT);
                    Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                    errors.SetError(txtUserName, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtUserName, res.CTRLADATTACCOUNT);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// UserNameValidated method implementation
        /// </summary>
        private void UserNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtUserName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Password
        /// <summary>
        /// UserNameValidating method implementation
        /// </summary>
        private void PasswordValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPassword.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                        throw new Exception(res.CTRLADATTPASSWORD);
                    Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                    errors.SetError(txtPassword, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtPassword, res.CTRLADATTPASSWORD);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PasswordValidated method implementation
        /// </summary>
        private void PasswordValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPassword, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region KeyAttribute
        /// <summary>
        /// KeyAttributeValidating method implementation
        /// </summary>
        private void KeyAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtKeyAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtKeyAttribute.Text, 1))
                        throw new Exception(res.CTRLADATTKEYERROR);
                    Config.Hosts.ActiveDirectoryHost.KeyAttribute = txtKeyAttribute.Text;
                    errors.SetError(txtKeyAttribute, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// KeyAttributeValidated method implmentation
        /// </summary>
        private void KeyAttributeValidated(object sender, EventArgs e)
        {

            try
            {
                Config.Hosts.ActiveDirectoryHost.KeyAttribute = txtKeyAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtKeyAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MailAttribute
        /// <summary>
        /// MailAttributeValidating method implementation
        /// </summary>
        private void MailAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMailAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMailAttribute.Text, 1))
                       throw new Exception(res.CTRLADATTEMAILERROR);
                    Config.Hosts.ActiveDirectoryHost.MailAttribute = txtMailAttribute.Text;
                    errors.SetError(txtMailAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMailAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MailAttributeValidated method implmentation
        /// </summary>
        private void MailAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.MailAttribute = txtMailAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMailAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region PhoneAttribute
        /// <summary>
        /// PhoneAttributeValidating method implementation
        /// </summary>
        private void PhoneAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPhoneAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPhoneAttribute.Text, 1))
                        throw new Exception(res.CTRLADATTPHONEERROR);
                    Config.Hosts.ActiveDirectoryHost.PhoneAttribute = txtPhoneAttribute.Text;
                    errors.SetError(txtPhoneAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPhoneAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PhoneAttributeValidated method implementation
        /// </summary>
        private void PhoneAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.PhoneAttribute = txtPhoneAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPhoneAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MethodAttribute
        /// <summary>
        /// MethodAttributeValidating method implementation
        /// </summary>
        private void MethodAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMethodAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMethodAttribute.Text, 0))
                        throw new Exception(res.CTRLADATMETHODERROR);
                    Config.Hosts.ActiveDirectoryHost.MethodAttribute = txtMethodAttribute.Text;
                    errors.SetError(txtMethodAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMethodAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MethodAttributeValidated method implmentation
        /// </summary>
        private void MethodAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.MethodAttribute = txtMethodAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMethodAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region OverrideMethodAttribute
        /// <summary>
        /// OverrideMethodAttributeValidating method implementation
        /// </summary>
        private void OverrideMethodAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtOverrideMethodAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtOverrideMethodAttribute.Text, 0))
                        throw new Exception(res.CTRLADATTKEYERROR);
                    Config.Hosts.ActiveDirectoryHost.OverrideMethodAttribute = txtOverrideMethodAttribute.Text;
                    errors.SetError(txtOverrideMethodAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtOverrideMethodAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// OverrideMethodAttributeValidated method implmentation
        /// </summary>
        private void OverrideMethodAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.OverrideMethodAttribute = txtOverrideMethodAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtOverrideMethodAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region PinAttribute
        /// <summary>
        /// PinAttributeValidating method implementation
        /// </summary>
        private void PinAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                  if (txtPinAttribute.Modified)
                  {
                      ManagementService.ADFSManager.SetDirty(true);
                      if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPinAttribute.Text, 0))
                          throw new Exception(res.CTRLADATPINERROR);
                      Config.Hosts.ActiveDirectoryHost.PinAttribute = txtPinAttribute.Text;
                      errors.SetError(txtPinAttribute, "");
                  }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPinAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PinAttributeValidated method implmentation
        /// </summary>
        private void PinAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                 Config.Hosts.ActiveDirectoryHost.PinAttribute = txtPinAttribute.Text;
                 ManagementService.ADFSManager.SetDirty(true);
                 errors.SetError(txtPinAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #endregion

        #region PublicKeyAttribute
        /// <summary>
        /// CheckPublicKeyAttributeValidating method implementation
        /// </summary>
        private void CheckPublicKeyAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                 if (txtPublicKeyAttribute.Modified)
                 {

                    ManagementService.ADFSManager.SetDirty(true);
                    Config.Hosts.ActiveDirectoryHost.PublicKeyCredentialAttribute = txtPublicKeyAttribute.Text;
                    errors.SetError(txtPublicKeyAttribute, "");
                }
                if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPublicKeyAttribute.Text, 2))
                 {
                     e.Cancel = true;
                     errors.SetError(txtPublicKeyAttribute, res.CTRLADATTKEYERROR);
                 } 
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPublicKeyAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CheckPublicKeyAttributeValidated method implmentation
        /// </summary>
        private void CheckPublicKeyAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.PublicKeyCredentialAttribute = txtPublicKeyAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPublicKeyAttribute, ""); 
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region RSACertAttribute
        /// <summary>
        /// RSACertAttributeValidating method implementation
        /// </summary>
        private void RSACertAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtRSACertAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.RSACertificateAttribute = txtRSACertAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtRSACertAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtRSACertAttribute, res.CTRLADATRSACERTERROR);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtRSACertAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// RSACertAttributeValidated method implmentation
        /// </summary>
        private void RSACertAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                  Config.Hosts.ActiveDirectoryHost.RSACertificateAttribute = txtRSACertAttribute.Text;
                  ManagementService.ADFSManager.SetDirty(true); 
                  errors.SetError(txtRSACertAttribute, ""); 
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region EnabledAttribute
        /// <summary>
        /// EnabledAttributeValidating method implementation
        /// </summary>
        private void EnabledAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtEnabledAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtEnabledAttribute.Text, 0))
                        throw new Exception(res.CTRLADATENABLEDERROR);
                    Config.Hosts.ActiveDirectoryHost.TotpEnabledAttribute = txtEnabledAttribute.Text;
                    errors.SetError(txtEnabledAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEnabledAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EnabledAttributeValidated method implmentation
        /// </summary>
        private void EnabledAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.TotpEnabledAttribute = txtEnabledAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEnabledAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MaxRows
        /// <summary>
        /// MaxRowsValidating method
        /// </summary>
        private void MaxRowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMaxRows.Modified)
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                    errors.SetError(txtMaxRows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MaxRowsValidated method
        /// </summary>
        private void MaxRowsValidated(object sender, EventArgs e)
        {
            try
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMaxRows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void BtnConnectClick(object sender, EventArgs e)
        {
            try
            {
                if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLADCONNECTIONERROR,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLADCONNECTIONOK,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Information
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// BtnTemplateMFAClick method implementation
        /// </summary>
        private void BtnTemplateMFAClick(object sender, EventArgs e)
        {
            try
            {
                if (ManagementService.SetADDSAttributesTemplate(ADDSTemplateKind.MFASchemaVersion))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    DoRefreshData();
                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLADTEMPLATEMFAERROR,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = res.CTRLADTEMPLATEMFAERROR + "\r\n" + ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// BtnTemplate2016Click method implementation
        /// </summary>
        private void BtnTemplate2016Click(object sender, EventArgs e)
        {
            try
            {
                if (ManagementService.SetADDSAttributesTemplate(ADDSTemplateKind.Windows2016Schemaversion))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    DoRefreshData();
                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLADTEMPLATE2016ERROR,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = res.CTRLADTEMPLATE2016ERROR + "\r\n" + ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// BtnTemplateBaseClick method implementation
        /// </summary>
        private void BtnTemplateBaseClick(object sender, EventArgs e)
        {
            try
            {
                if (ManagementService.SetADDSAttributesTemplate(ADDSTemplateKind.AllSchemaVersions))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    DoRefreshData();
                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLADTEMPLATEBASEERROR,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = res.CTRLADTEMPLATEBASEERROR + "\r\n" + ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class SQLConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SQLViewControl _view;
        private CheckBox chkUseSQL;
        private TextBox txtConnectionString;
        private TextBox txtMaxRows;
        private bool _UpdateControlsLayouts;
        private Button btnConnect;
        private CheckBox chkUseAlwaysEncryptSQL;
        private TextBox txtEncryptKeyName;
        private NumericUpDown txtCertificateDuration;
        private CheckBox chkReuseCertificate;
        private TextBox txtCertificateThumbPrint;
        private Button btnCreateDB;
        private Button btnCreateCryptedDB;
        private ErrorProvider errors;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblCertificateThumbPrint;
        private Label lblCertificateDuration;
        private Label lblEncryptKeyName;
        private Label lblMaxRows;
        private Label lblConnectionString;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SQLConfigurationControl(SQLViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    if (!Config.UseActiveDirectory)
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 362;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 362;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                chkUseSQL = new CheckBox();
                chkUseSQL.Text = res.CTRLSQLUSING;
                chkUseSQL.Checked = !Config.UseActiveDirectory;
                chkUseSQL.Left = 10;
                chkUseSQL.Top = 19;
                chkUseSQL.Width = 450;
                chkUseSQL.CheckedChanged += UseSQLCheckedChanged;
                _txtpanel.Controls.Add(chkUseSQL);

                lblConnectionString = new Label();
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblConnectionString.Left = 50;
                lblConnectionString.Top = 51;
                lblConnectionString.Width = 150;
                _txtpanel.Controls.Add(lblConnectionString);

                txtConnectionString = new TextBox();
                txtConnectionString.Text = Config.Hosts.SQLServerHost.ConnectionString;
                txtConnectionString.Left = 210;
                txtConnectionString.Top = 47;
                txtConnectionString.Width = 700;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;
                txtConnectionString.Validating += ConnectionStringValidating;
                txtConnectionString.Validated += ConnectionStringValidated;
                _txtpanel.Controls.Add(txtConnectionString);

                lblMaxRows = new Label();
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblMaxRows.Left = 50;
                lblMaxRows.Top = 82;
                lblMaxRows.Width = 150;
                _txtpanel.Controls.Add(lblMaxRows);

                txtMaxRows = new TextBox();
                txtMaxRows.Text = Config.Hosts.SQLServerHost.MaxRows.ToString();
                txtMaxRows.Left = 210;
                txtMaxRows.Top = 78;
                txtMaxRows.Width = 50;
                txtMaxRows.TextAlign = HorizontalAlignment.Right;
                txtMaxRows.Enabled = !Config.UseActiveDirectory;
                txtMaxRows.Validating += MaxRowsValidating;
                txtMaxRows.Validated += MaxRowsValidated;
                _txtpanel.Controls.Add(txtMaxRows);

                btnConnect = new Button();
                btnConnect.Text = res.CTRLSQLTEST;
                btnConnect.Left = 680;
                btnConnect.Top = 82;
                btnConnect.Width = 230;
                btnConnect.Enabled = !Config.UseActiveDirectory;
                btnConnect.Click += BtnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                btnCreateDB = new Button();
                btnCreateDB.Text = res.CTRLSQLCREATEDB;
                btnCreateDB.Left = 680;
                btnCreateDB.Top = 113;
                btnCreateDB.Width = 230;
                btnCreateDB.Enabled = !Config.UseActiveDirectory;
                btnCreateDB.Click += BtnCreateDBClick;
                _txtpanel.Controls.Add(btnCreateDB);

                chkUseAlwaysEncryptSQL = new CheckBox();
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.Hosts.SQLServerHost.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;
                chkUseAlwaysEncryptSQL.Left = 10;
                chkUseAlwaysEncryptSQL.Top = 144;
                chkUseAlwaysEncryptSQL.Width = 450;
                chkUseAlwaysEncryptSQL.CheckedChanged += UseSQLCryptCheckedChanged;
                _txtpanel.Controls.Add(chkUseAlwaysEncryptSQL);

                lblEncryptKeyName = new Label();
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblEncryptKeyName.Left = 50;
                lblEncryptKeyName.Top = 175;
                lblEncryptKeyName.Width = 150;
                _txtpanel.Controls.Add(lblEncryptKeyName);

                txtEncryptKeyName = new TextBox();
                txtEncryptKeyName.Text = Config.Hosts.SQLServerHost.KeyName;
                txtEncryptKeyName.Left = 210;
                txtEncryptKeyName.Top = 171;
                txtEncryptKeyName.Width = 100;
                txtEncryptKeyName.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                txtEncryptKeyName.Validating += EncryptKeyNameValidating;
                txtEncryptKeyName.Validated += EncryptKeyNameValidated;
                _txtpanel.Controls.Add(txtEncryptKeyName);

                lblCertificateDuration = new Label();
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateDuration.Left = 50;
                lblCertificateDuration.Top = 206;
                lblCertificateDuration.Width = 150;
                _txtpanel.Controls.Add(lblCertificateDuration);

                txtCertificateDuration = new NumericUpDown();
                txtCertificateDuration.Left = 210;
                txtCertificateDuration.Top = 202;
                txtCertificateDuration.Width = 50;
                txtCertificateDuration.TextAlign = HorizontalAlignment.Center;
                txtCertificateDuration.Value = Config.Hosts.SQLServerHost.CertificateValidity;
                txtCertificateDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtCertificateDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCertificateDuration.ValueChanged += CertValidityValueChanged;
                txtCertificateDuration.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                _txtpanel.Controls.Add(txtCertificateDuration);

                chkReuseCertificate = new CheckBox();
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.Hosts.SQLServerHost.CertReuse;
                chkReuseCertificate.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                chkReuseCertificate.Left = 50;
                chkReuseCertificate.Top = 233;
                chkReuseCertificate.Width = 450;
                chkReuseCertificate.CheckedChanged += UseSQLReuseCertCheckedChanged;
                _txtpanel.Controls.Add(chkReuseCertificate);

                lblCertificateThumbPrint = new Label();
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                lblCertificateThumbPrint.Left = 100;
                lblCertificateThumbPrint.Top = 264;
                lblCertificateThumbPrint.Width = 150;
                _txtpanel.Controls.Add(lblCertificateThumbPrint);

                txtCertificateThumbPrint = new TextBox();
                txtCertificateThumbPrint.Text = Config.Hosts.SQLServerHost.ThumbPrint;
                txtCertificateThumbPrint.Left = 260;
                txtCertificateThumbPrint.Top = 260;
                txtCertificateThumbPrint.Width = 300;
                txtCertificateThumbPrint.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted && Config.Hosts.SQLServerHost.CertReuse);
                txtCertificateThumbPrint.Validating += CertificateThumbPrintValidating;
                txtCertificateThumbPrint.Validated += CertificateThumbPrintValidated;
                _txtpanel.Controls.Add(txtCertificateThumbPrint);

                btnCreateCryptedDB = new Button();
                btnCreateCryptedDB.Text = res.CTRLSQLCREATECRYPTEDDB;
                btnCreateCryptedDB.Left = 680;
                btnCreateCryptedDB.Top = 322;
                btnCreateCryptedDB.Width = 230;
                btnCreateCryptedDB.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                btnCreateCryptedDB.Click += BtnCreateCryptedDBClick;
                _txtpanel.Controls.Add(btnCreateCryptedDB);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 372;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 372;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts(!Config.UseActiveDirectory, Config.Hosts.SQLServerHost.IsAlwaysEncrypted, Config.Hosts.SQLServerHost.CertReuse);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                chkUseSQL.Checked = !Config.UseActiveDirectory;

                txtConnectionString.Text = Config.Hosts.SQLServerHost.ConnectionString;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;

                txtMaxRows.Text = Config.Hosts.SQLServerHost.MaxRows.ToString();
                txtMaxRows.Enabled = !Config.UseActiveDirectory;

                btnConnect.Enabled = !Config.UseActiveDirectory;

                btnCreateDB.Enabled = !Config.UseActiveDirectory;

                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.Hosts.SQLServerHost.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;

                txtEncryptKeyName.Text = Config.Hosts.SQLServerHost.KeyName;
                txtEncryptKeyName.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                txtCertificateDuration.Value = Config.Hosts.SQLServerHost.CertificateValidity;
                txtCertificateDuration.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.Hosts.SQLServerHost.CertReuse;
                chkReuseCertificate.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                txtCertificateThumbPrint.Text = Config.Hosts.SQLServerHost.ThumbPrint;
                txtCertificateThumbPrint.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted && Config.Hosts.SQLServerHost.CertReuse);

                btnCreateCryptedDB.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                btnCreateCryptedDB.Text = res.CTRLSQLCREATECRYPTEDDB;

                chkUseSQL.Text = res.CTRLSQLUSING;
                btnConnect.Text = res.CTRLSQLTEST;
                btnCreateDB.Text = res.CTRLSQLCREATEDB;
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (txtConnectionString.Enabled)
            {
                if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                    errors.SetError(txtConnectionString, res.CTRLSQLCONNECTSTRERROR);
                else
                    errors.SetError(txtConnectionString, "");
            }
            else
                errors.SetError(txtConnectionString, "");

            if (txtMaxRows.Enabled)
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                if ((maxrows < 1000) || (maxrows > 1000000))
                    errors.SetError(txtMaxRows, String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                else
                    errors.SetError(txtMaxRows, "");
            }
            else
                errors.SetError(txtMaxRows, "");
            if (chkUseAlwaysEncryptSQL.Checked)
            {
                if (txtEncryptKeyName.Enabled)
                {
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        errors.SetError(txtEncryptKeyName, res.CTRLNULLOREMPTYERROR);
                    else
                        errors.SetError(txtEncryptKeyName, "");
                }
                else
                    errors.SetError(txtEncryptKeyName, "");
                if (chkReuseCertificate.Checked)
                {
                    if (txtCertificateThumbPrint.Enabled)
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            errors.SetError(txtCertificateThumbPrint, string.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                        else
                            errors.SetError(txtCertificateThumbPrint, "");
                    }
                    else
                        errors.SetError(txtCertificateThumbPrint, "");
                }
            }
            else
                errors.SetError(txtCertificateThumbPrint, "");
        }

        /// <summary>
        /// UpdateConnectionString method implmentation
        /// </summary>
        private void UpdateConnectionString(bool crypted)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                string cs = txtConnectionString.Text;
                if (!crypted)
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                    cs += ";Column Encryption Setting=enabled";
                }
                txtConnectionString.Text = cs;
            }
        }


        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled, bool iscrypted, bool reusecert)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtConnectionString.Enabled = isenabled;
                txtMaxRows.Enabled = isenabled;
                btnConnect.Enabled = isenabled;
                btnCreateDB.Enabled = isenabled;
                txtCertificateDuration.Enabled = isenabled && iscrypted;
                txtEncryptKeyName.Enabled = isenabled && iscrypted;
                txtCertificateThumbPrint.Enabled = isenabled && iscrypted && reusecert;
                chkUseAlwaysEncryptSQL.Enabled = isenabled;
                chkReuseCertificate.Enabled = isenabled && iscrypted;
                btnCreateCryptedDB.Enabled = isenabled && iscrypted;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UseSQLCheckedChanged method implementation
        /// </summary>
        private void UseSQLCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.UseActiveDirectory = !chkUseSQL.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ConnectionStringValidating method
        /// </summary>
        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtConnectionString.Modified) && (txtConnectionString.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                        throw new Exception(res.CTRLSQLCONNECTSTRERROR);
                    Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                    errors.SetError(txtConnectionString, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtConnectionString, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ConnectionStringValidated method
        /// </summary>
        private void ConnectionStringValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtConnectionString, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MaxRowsValidating method
        /// </summary>
        private void MaxRowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtMaxRows.Modified) && (txtMaxRows.Enabled))
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new Exception(String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                    Config.Hosts.SQLServerHost.MaxRows = maxrows;
                    errors.SetError(txtMaxRows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MaxRowsValidated method
        /// </summary>
        private void MaxRowsValidated(object sender, EventArgs e)
        {
            try
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                Config.Hosts.SQLServerHost.MaxRows = maxrows;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMaxRows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void BtnConnectClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLSQLCONNECTERROR,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);

                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = res.CTRLSQLCONNECTOK,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Information
                    };
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void BtnCreateDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            try
            {
                bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                Cursor crs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor; 
                    if (result)
                    {
                        this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFADatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                        messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                        messageBoxParameters.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters);
                    }
                }
                finally
                {
                    this.Cursor = crs;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnCreateCryptedDBClick method implmentation
        /// </summary>
        private void BtnCreateCryptedDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            try
            {
                if (Config.Hosts.SQLServerHost.IsAlwaysEncrypted)
                {
                    bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                    Cursor crs = this.Cursor;
                    try
                    {
                        this.Cursor = Cursors.WaitCursor; 
                        if (result)
                        {
                            bool isnew = false;
                            string thumb = string.Empty;
                            if ((Config.Hosts.SQLServerHost.CertReuse) && (Certs.GetCertificate(Config.Hosts.SQLServerHost.ThumbPrint.ToUpper(), StoreLocation.LocalMachine)) != null)
                                thumb = Config.Hosts.SQLServerHost.ThumbPrint.ToUpper();
                            else
                            { 
                                thumb = ManagementService.ADFSManager.RegisterNewSQLCertificate(null, Config.Hosts.SQLServerHost.CertificateValidity, Config.Hosts.SQLServerHost.KeyName);
                                isnew = true;
                            }
                            this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFAEncryptedDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text, Config.Hosts.SQLServerHost.KeyName, thumb);
                            this.Cursor = Cursors.Default; 
                            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                            if (isnew)
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED2, Wizard.txtDBName.Text, thumb);
                            else
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                            messageBoxParameters.Buttons = MessageBoxButtons.OK;
                            messageBoxParameters.Icon = MessageBoxIcon.Information;
                            this._snapin.Console.ShowDialog(messageBoxParameters);
                        }
                    }
                    finally
                    {
                        this.Cursor = crs;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default; 
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLCryptCheckedChanged method implementation
        /// </summary>
        private void UseSQLCryptCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.Hosts.SQLServerHost.IsAlwaysEncrypted = chkUseAlwaysEncryptSQL.Checked;
                    UpdateConnectionString(chkUseAlwaysEncryptSQL.Checked);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLReuseCertCheckedChanged method implementaton
        /// </summary>
        private void UseSQLReuseCertCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    this.Config.Hosts.SQLServerHost.CertReuse = chkReuseCertificate.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, Config.Hosts.SQLServerHost.IsAlwaysEncrypted, Config.Hosts.SQLServerHost.CertReuse);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertValidityValueChanged method implementation
        /// </summary>
        private void CertValidityValueChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                    ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(txtCertificateDuration, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EncryptKeyNameValidating method implmentation
        /// </summary>
        private void EncryptKeyNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtEncryptKeyName.Modified) && (txtEncryptKeyName.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.Hosts.SQLServerHost.KeyName = txtEncryptKeyName.Text;
                    errors.SetError(txtEncryptKeyName, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEncryptKeyName, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EncryptKeyNameValidated method implementation
        /// </summary>
        private void EncryptKeyNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.KeyName = txtEncryptKeyName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEncryptKeyName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidating method implementation
        /// </summary>
        private void CertificateThumbPrintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtCertificateThumbPrint.Modified) && (txtCertificateThumbPrint.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            throw new Exception(string.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                    }
                    Config.Hosts.SQLServerHost.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                    errors.SetError(txtCertificateThumbPrint, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCertificateThumbPrint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidated method implementation
        /// </summary>
        private void CertificateThumbPrintValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCertificateThumbPrint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }   
    }

    public partial class SMTPConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SMTPViewControl _view;
        private TextBox txtCompany;
        private ErrorProvider errors;
        private TextBox txtFrom;
        private TextBox txtHost;
        private TextBox txtPort;
        private CheckBox chkUseSSL;
        private TextBox txtAccount;
        private TextBox txtPassword;
        private CheckBox chkAnonymous;
        private CheckBox chkDeliveryNotifications;
        private Button btnConnect;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblPassword;
        private Label lblAccount;
        private Label lblidentify;
        private Label lblPort;
        private Label lblHost;
        private Label lblServer;
        private Label lblFrom;
        private Label lblCompany;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SMTPConfigurationControl(SMTPViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Email);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;                   
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 351;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 351;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblCompany = new Label();
                lblCompany.Text = res.CTRLSMTPCOMPANY + " : ";
                lblCompany.Left = 10;
                lblCompany.Top = 19;
                lblCompany.Width = 200;
                _txtpanel.Controls.Add(lblCompany);

                txtCompany = new TextBox();
                txtCompany.Text = Config.MailProvider.Company;
                txtCompany.Left = 210;
                txtCompany.Top = 15;
                txtCompany.Width = 250;
                txtCompany.Validating += CompanyValidating;
                txtCompany.Validated += CompanyValidated;
                _txtpanel.Controls.Add(txtCompany);

                lblFrom = new Label();
                lblFrom.Text = res.CTRLSMTPFROM + " : ";
                lblFrom.Left = 10;
                lblFrom.Top = 51;
                lblFrom.Width = 200;
                _txtpanel.Controls.Add(lblFrom);

                txtFrom = new TextBox();
                txtFrom.Text = Config.MailProvider.From;
                txtFrom.Left = 210;
                txtFrom.Top = 47;
                txtFrom.Width = 250;
                txtFrom.Validating += FromValidating;
                txtFrom.Validated += FromValidated;
                _txtpanel.Controls.Add(txtFrom);

                lblServer = new Label();
                lblServer.Text = res.CTRLSMTPSERVER;
                lblServer.Left = 10;
                lblServer.Top = 95;
                lblServer.Width = 180;
                _txtpanel.Controls.Add(lblServer);

                lblHost = new Label();
                lblHost.Text = res.CTRLSMTPSERVERADDRESS + " : ";
                lblHost.Left = 30;
                lblHost.Top = 127;
                lblHost.Width = 180;
                _txtpanel.Controls.Add(lblHost);

                txtHost = new TextBox();
                txtHost.Text = Config.MailProvider.Host;
                txtHost.Left = 210;
                txtHost.Top = 123;
                txtHost.Width = 250;
                txtHost.Validating += HostValidating;
                txtHost.Validated += HostValidated;
                _txtpanel.Controls.Add(txtHost);

                lblPort = new Label();
                lblPort.Text = res.CTRLSMTPPORT + " : ";
                lblPort.Left = 480;
                lblPort.Top = 127;
                lblPort.Width = 40;
                _txtpanel.Controls.Add(lblPort);

                txtPort = new TextBox();
                txtPort.Text = Config.MailProvider.Port.ToString();
                txtPort.Left = 520;
                txtPort.Top = 123;
                txtPort.Width = 40;
                txtPort.TextAlign = HorizontalAlignment.Center;
                txtPort.Validating += PortValidating;
                txtPort.Validated += PortValidated;
                _txtpanel.Controls.Add(txtPort);

                chkUseSSL = new CheckBox();
                chkUseSSL.Text = "SSL";
                chkUseSSL.Checked = Config.MailProvider.UseSSL;
                chkUseSSL.Left = 590;
                chkUseSSL.Top = 123;
                chkUseSSL.Width = 100;
                chkUseSSL.CheckedChanged += SSLChecked;
                _txtpanel.Controls.Add(chkUseSSL);

                lblidentify = new Label();
                lblidentify.Text = res.CTRLSMTPIDENTIFICATION;
                lblidentify.Left = 10;
                lblidentify.Top = 170;
                lblidentify.Width = 180;
                _txtpanel.Controls.Add(lblidentify);

                lblAccount = new Label();
                lblAccount.Text = res.CTRLSMTPACCOUNT + " : ";
                lblAccount.Left = 30;
                lblAccount.Top = 202;
                lblAccount.Width = 180;
                _txtpanel.Controls.Add(lblAccount);

                txtAccount = new TextBox();
                txtAccount.Text = Config.MailProvider.UserName;
                txtAccount.Left = 210;
                txtAccount.Top = 200;
                txtAccount.Width = 250;
                txtAccount.Enabled = !Config.MailProvider.Anonymous;
                txtAccount.Validating += UserNameValidating;
                txtAccount.Validated += UserNameValidated;
                _txtpanel.Controls.Add(txtAccount);

                lblPassword = new Label();
                lblPassword.Text = res.CTRLSMTPPASSWORD + " : ";
                lblPassword.Left = 30;
                lblPassword.Top = 234;
                lblPassword.Width = 180;
                _txtpanel.Controls.Add(lblPassword);

                txtPassword = new TextBox();
                txtPassword.Text = Config.MailProvider.Password;
                txtPassword.Left = 210;
                txtPassword.Top = 232;
                txtPassword.Width = 250;
                txtPassword.Enabled = !Config.MailProvider.Anonymous;
                txtPassword.PasswordChar = '*';
                txtPassword.Validating += PwdValidating;
                txtPassword.Validated += PwdValidated;
                _txtpanel.Controls.Add(txtPassword);

                chkAnonymous = new CheckBox();
                chkAnonymous.Text = res.CTRLSMTPANONYMOUS;
                chkAnonymous.Checked = Config.MailProvider.Anonymous;
                chkAnonymous.Left = 480;
                chkAnonymous.Top = 232;
                chkAnonymous.Width = 150;
                chkAnonymous.CheckedChanged += AnonymousChecked;
                _txtpanel.Controls.Add(chkAnonymous);

                chkDeliveryNotifications = new CheckBox();
                chkDeliveryNotifications.Text = res.CTRLSMTPDELIVERYNOTIFICATIONS;
                chkDeliveryNotifications.Checked = Config.MailProvider.DeliveryNotifications;
                chkDeliveryNotifications.Left = 10;
                chkDeliveryNotifications.Top = 264;
                chkDeliveryNotifications.Width = 250;
                chkDeliveryNotifications.CheckedChanged += DeliveryNotificationsChecked;
                _txtpanel.Controls.Add(chkDeliveryNotifications);

                btnConnect = new Button();
                btnConnect.Text = res.CTRLSMTPTEST;
                btnConnect.Left = 480;
                btnConnect.Top = 285;
                btnConnect.Width = 150;
                btnConnect.Click += BtnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 361;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 361;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                txtCompany.Text = Config.MailProvider.Company;

                txtFrom.Text = Config.MailProvider.From;

                txtHost.Text = Config.MailProvider.Host;

                txtPort.Text = Config.MailProvider.Port.ToString();

                chkUseSSL.CheckedChanged += SSLChecked;

                txtAccount.Text = Config.MailProvider.UserName;

                txtAccount.Enabled = !Config.MailProvider.Anonymous;

                txtPassword.Text = Config.MailProvider.Password;

                txtPassword.Enabled = !Config.MailProvider.Anonymous;

                chkAnonymous.Checked = Config.MailProvider.Anonymous;

                chkDeliveryNotifications.Checked = Config.MailProvider.DeliveryNotifications;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                btnConnect.Text = res.CTRLSMTPTEST;
                chkAnonymous.Text = res.CTRLSMTPANONYMOUS;
                chkDeliveryNotifications.Text = res.CTRLSMTPDELIVERYNOTIFICATIONS;
                lblPassword.Text = res.CTRLSMTPPASSWORD + " : ";
                lblAccount.Text = res.CTRLSMTPACCOUNT + " : ";
                lblidentify.Text = res.CTRLSMTPIDENTIFICATION;
                lblPort.Text = res.CTRLSMTPPORT + " : ";
                lblHost.Text = res.CTRLSMTPSERVERADDRESS + " : ";
                lblServer.Text = res.CTRLSMTPSERVER;
                lblFrom.Text = res.CTRLSMTPFROM + " : ";
                lblCompany.Text = res.CTRLSMTPCOMPANY + " : ";
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtCompany.Text))
                errors.SetError(txtCompany, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCompany, "");

            if (string.IsNullOrEmpty(txtFrom.Text))
                errors.SetError(txtFrom, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtFrom, "");

            if (string.IsNullOrEmpty(txtHost.Text))
                errors.SetError(txtHost, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtHost, "");

            int port = Convert.ToInt32(txtPort.Text);
            if ((port <= UInt16.MinValue) || (port >= UInt16.MaxValue))
                errors.SetError(txtPort, res.CTRLSMTPPORTERROR);
            else
                errors.SetError(txtPort, "");

            if (string.IsNullOrEmpty(txtAccount.Text))
                errors.SetError(txtAccount, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtAccount, "");

            if (string.IsNullOrEmpty(txtPassword.Text))
                errors.SetError(txtPassword, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtPassword, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        #region Company
        /// <summary>
        /// CompanyValidating method
        /// </summary>
        private void CompanyValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtCompany.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCompany.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Company = txtCompany.Text;
                    errors.SetError(txtCompany, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCompany, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CompanyValidated method
        /// </summary>
        private void CompanyValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Company = txtCompany.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCompany, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region From
        /// <summary>
        /// FromValidating method implementation
        /// </summary>
        private void FromValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtFrom.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!MMCService.IsValidEmail(txtFrom.Text))
                        throw new Exception(res.CTRLSMTPMAILERROR);
                    Config.MailProvider.From = txtFrom.Text;
                    errors.SetError(txtFrom, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtFrom, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// FromValidated method implmentation
        /// </summary>
        private void FromValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.From = txtFrom.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtFrom, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Host
        /// <summary>
        /// HostValidating method implementation
        /// </summary>
        private void HostValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtHost.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtHost.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Host = txtHost.Text;
                    errors.SetError(txtHost, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtHost, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// HostValidated method implementation
        /// </summary>
        private void HostValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Host = txtHost.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtHost, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Port
        /// <summary>
        /// PortValidating method implementation
        /// </summary>
        private void PortValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPort.Modified)
                {
                    int v = Convert.ToInt32(txtPort.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((v <= UInt16.MinValue) || (v >= UInt16.MaxValue))
                        throw new Exception(res.CTRLSMTPPORTERROR);
                    Config.MailProvider.Port = v;
                    errors.SetError(txtPort, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPort, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PortValidated method implementation
        /// </summary>
        private void PortValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Port = Convert.ToInt32(txtPort.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPort, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region SSL
        /// <summary>
        /// SSLChecked method implementation
        /// </summary>
        private void SSLChecked(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.MailProvider.UseSSL = chkUseSSL.Checked;
                    errors.SetError(chkUseSSL, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUseSSL, ex.Message);
            }
        }
        #endregion

        #region UserName
        /// <summary>
        /// UserNameValidating method implmentation
        /// </summary>
        private void UserNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtAccount.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtAccount.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.UserName = txtAccount.Text;
                    errors.SetError(txtAccount, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtAccount, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// UserNameValidated method implmentation
        /// </summary>
        private void UserNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.UserName = txtAccount.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtAccount, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Password
        /// <summary>
        /// PwdValidating method implementation
        /// </summary>
        private void PwdValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPassword.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtPassword.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Password = txtPassword.Text;
                    errors.SetError(txtPassword, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPassword, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PwdValidated method implementation
        /// </summary>
        private void PwdValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Password = txtPassword.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPassword, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Anonymous
        /// <summary>
        /// AnonymousChecked method implementation
        /// </summary>
        private void AnonymousChecked(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.MailProvider.Anonymous = chkAnonymous.Checked;
                    txtAccount.Enabled = !chkAnonymous.Checked;
                    txtPassword.Enabled = !chkAnonymous.Checked;
                    errors.SetError(chkAnonymous, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAnonymous, ex.Message);
            }
        }
        #endregion

        #region DeliveryNotificationsChecked
        /// <summary>
        /// DeliveryNotificationsChecked method implementation
        /// </summary>
        private void DeliveryNotificationsChecked(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.MailProvider.DeliveryNotifications = chkDeliveryNotifications.Checked;
                    errors.SetError(chkDeliveryNotifications, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkDeliveryNotifications, ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// btnConnectClick method
        /// </summary>
        private void BtnConnectClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                MailProvider mail = Config.MailProvider;
                MailMessage Message = new MailMessage(mail.From, mail.From)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    Subject = "MFA SMTP¨Test",
                    IsBodyHtml = false,
                    Body = string.Format("Send mail test"),
                    DeliveryNotificationOptions = DeliveryNotificationOptions.Never
                };

                SmtpClient client = new SmtpClient
                {
                    Host = mail.Host,
                    Port = mail.Port,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = mail.UseSSL
                };
                if (!mail.Anonymous)
                    client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
                client.Send(Message);

                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = res.CTRLSMTPMESSAGEOK + " " + mail.From,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Information
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class SMSConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SMSViewControl _view;
        private ErrorProvider errors;
        private TextBox txtCompany;
        private CheckBox chkIsTwoWay;
        private TextBox txtTimeout;
        private TextBox txtSHA1;
        private TextBox txtDLL;
        private TextBox txtParams;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblParams;
        private Label lblDLL;
        private Label lblSha1;
        private Label lblTimeout;
        private Label lblCompany;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SMSConfigurationControl(SMSViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.External);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 261;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 261;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblCompany = new Label();
                lblCompany.Text = res.CTRLSMSCOMPANY + " : ";
                lblCompany.Left = 10;
                lblCompany.Top = 19;
                lblCompany.Width = 170;
                _txtpanel.Controls.Add(lblCompany);

                txtCompany = new TextBox();
                txtCompany.Text = Config.ExternalProvider.Company;
                txtCompany.Left = 190;
                txtCompany.Top = 15;
                txtCompany.Width = 250;
                txtCompany.Validating += CompanyValidating;
                txtCompany.Validated += CompanyValidated;
                _txtpanel.Controls.Add(txtCompany);

                chkIsTwoWay = new CheckBox();
                chkIsTwoWay.Text = res.CTRLSMSASYNCCALL;
                chkIsTwoWay.Left = 10;
                chkIsTwoWay.Top = 47;
                chkIsTwoWay.Width = 160;
                chkIsTwoWay.Checked = Config.ExternalProvider.IsTwoWay;
                chkIsTwoWay.CheckedChanged += ChkIsTwoWayChanged;
                _txtpanel.Controls.Add(chkIsTwoWay);

                lblTimeout = new Label();
                lblTimeout.Text = res.CTRLSMSTIMEOUT + " : ";
                lblTimeout.Left = 190;
                lblTimeout.Top = 51;
                lblTimeout.Width = 60;
                _txtpanel.Controls.Add(lblTimeout);

                txtTimeout = new TextBox();
                txtTimeout.Text = Config.ExternalProvider.Timeout.ToString();
                txtTimeout.Left = 260;
                txtTimeout.Top = 47;
                txtTimeout.Width = 50;
                txtTimeout.TextAlign = HorizontalAlignment.Center;
                txtTimeout.Validating += TimeOutValidating;
                txtTimeout.Validated += TimeOutValidated;
                _txtpanel.Controls.Add(txtTimeout);

                lblSha1 = new Label();
                lblSha1.Text = res.CTRLSMSSHA1 + " : ";
                lblSha1.Left = 370;
                lblSha1.Top = 51;
                lblSha1.Width = 110;
                _txtpanel.Controls.Add(lblSha1);

                txtSHA1 = new TextBox();
                txtSHA1.Text = Config.ExternalProvider.Sha1Salt;
                txtSHA1.Left = 490;
                txtSHA1.Top = 47;
                txtSHA1.Width = 120;
                txtSHA1.Validating += SHA1Validating;
                txtSHA1.Validated += SHA1Validated;
                _txtpanel.Controls.Add(txtSHA1);

                lblDLL = new Label();
                lblDLL.Text = res.CTRLSMSASSEMBLY + " : ";
                lblDLL.Left = 10;
                lblDLL.Top = 82;
                lblDLL.Width = 170;
                _txtpanel.Controls.Add(lblDLL);

                txtDLL = new TextBox();
                txtDLL.Text = Config.ExternalProvider.FullQualifiedImplementation;
                txtDLL.Left = 190;
                txtDLL.Top = 78;
                txtDLL.Width = 820;
                txtDLL.Validating += DLLValidating;
                txtDLL.Validated += DLLValidated;
                _txtpanel.Controls.Add(txtDLL);

                lblParams = new Label();
                lblParams.Text = res.CTRLSMSPARAMS + " : ";
                lblParams.Left = 10;
                lblParams.Top = 114;
                lblParams.Width = 170;
                _txtpanel.Controls.Add(lblParams);

                txtParams = new TextBox();
                txtParams.Text = Config.ExternalProvider.Parameters.Data;
                txtParams.Left = 190;
                txtParams.Top = 114;
                txtParams.Width = 820;
                txtParams.Height = 100;
                txtParams.Multiline = true;
                txtParams.Validating += ParamsValidating;
                txtParams.Validated += ParamsValidated;
                _txtpanel.Controls.Add(txtParams);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 271;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 271;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                txtCompany.Text = Config.ExternalProvider.Company;

                chkIsTwoWay.Checked = Config.ExternalProvider.IsTwoWay;

                txtTimeout.Text = Config.ExternalProvider.Timeout.ToString();

                txtSHA1.Text = Config.ExternalProvider.Sha1Salt;

                txtDLL.Text = Config.ExternalProvider.FullQualifiedImplementation;

                txtParams.Text = Config.ExternalProvider.Parameters.Data;

                lblCompany.Text = res.CTRLSMSCOMPANY + " : ";
                chkIsTwoWay.Text = res.CTRLSMSASYNCCALL;
                lblTimeout.Text = res.CTRLSMSTIMEOUT + " : ";
                lblSha1.Text = res.CTRLSMSSHA1 + " : ";
                lblDLL.Text = res.CTRLSMSASSEMBLY + " : ";
                lblParams.Text = res.CTRLSMSPARAMS + " : ";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtCompany.Text))
                errors.SetError(txtCompany, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCompany, "");

            int timeout = Convert.ToInt32(txtTimeout.Text);
            if ((timeout <= 0) || (timeout > 1000))
                errors.SetError(txtTimeout, string.Format(res.CTRLINVALIDVALUE, "1", "1000"));
            else
                errors.SetError(txtTimeout, "");

            if (string.IsNullOrEmpty(txtSHA1.Text))
                errors.SetError(txtSHA1, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtSHA1, "");

            if (!string.IsNullOrEmpty(txtDLL.Text))
            {
                if (!AssemblyParser.CheckSMSAssembly(txtDLL.Text))
                    errors.SetError(txtDLL, res.CTRLSMSIVALIDEXTERROR);
                else
                    errors.SetError(txtDLL, "");
            }
            else
                errors.SetError(txtDLL, "");
            errors.SetError(txtParams, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// chkIsTwoWayChanged method implementation
        /// </summary>
        private void ChkIsTwoWayChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.ExternalProvider.IsTwoWay = chkIsTwoWay.Checked;
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkIsTwoWay, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #region Company
        /// <summary>
        /// Company method
        /// </summary>
        private void CompanyValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtCompany.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCompany.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.ExternalProvider.Company = txtCompany.Text;
                    errors.SetError(txtCompany, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCompany, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CompanyValidated method
        /// </summary>
        private void CompanyValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Company = txtCompany.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCompany, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region TimeOut
        /// <summary>
        /// TimeOutValidating event
        /// </summary>
        private void TimeOutValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtTimeout.Modified)
                {
                    int v =  Convert.ToInt32(txtTimeout.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((v <= 0) || (v > 1000))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "1000"));
                    Config.ExternalProvider.Timeout = v;
                    errors.SetError(txtTimeout, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTimeout, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TimeOutValidated method
        /// </summary>
        private void TimeOutValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Timeout = Convert.ToInt32(txtTimeout.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTimeout, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region SHA1Salt
        /// <summary>
        /// SHA1Validating event
        /// </summary>
        private void SHA1Validating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtSHA1.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtSHA1.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                    errors.SetError(txtSHA1, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtSHA1, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SHA1Validated event
        /// </summary>
        private void SHA1Validated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtSHA1, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region DLL
        /// <summary>
        /// DLLValidating event
        /// </summary>
        private void DLLValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtDLL.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!string.IsNullOrEmpty(txtDLL.Text))
                    {
                        if (!AssemblyParser.CheckSMSAssembly(txtDLL.Text))
                            throw new Exception(res.CTRLSMSIVALIDEXTERROR);
                    }
                    Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                    errors.SetError(txtDLL, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLL, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DLLValidated event
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDLL, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Params
        /// <summary>
        /// ParamsValidating event
        /// </summary>
        private void ParamsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtParams.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.ExternalProvider.Parameters.Data = txtParams.Text;
                    errors.SetError(txtParams, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ParamsValidated event
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Parameters.Data = txtParams.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtParams, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { 
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class AzureConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private AzureViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;
        private TextBox txtTenantId;
        private TextBox txtThumbprint;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblThumbPrint;
        private Label lblTenantID;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public AzureConfigurationControl(AzureViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Azure);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 120;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 85;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 85;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblTenantID = new Label();
                lblTenantID.Text = res.CTRLAZURETENANTID + " : ";
                lblTenantID.Left = 10;
                lblTenantID.Top = 19;
                lblTenantID.Width = 160;
                _txtpanel.Controls.Add(lblTenantID);

                txtTenantId = new TextBox();
                txtTenantId.Text = Config.AzureProvider.TenantId;
                txtTenantId.Left = 180;
                txtTenantId.Top = 15;
                txtTenantId.Width = 300;
                txtTenantId.Validating += TenantidValidating;
                txtTenantId.Validated += TenantidValidated;
                _txtpanel.Controls.Add(txtTenantId);

                lblThumbPrint = new Label();
                lblThumbPrint.Text = res.CTRLAZURETHUMPRINT + " : ";
                lblThumbPrint.Left = 10;
                lblThumbPrint.Top = 51;
                lblThumbPrint.Width = 160;
                _txtpanel.Controls.Add(lblThumbPrint);

                txtThumbprint = new TextBox();
                txtThumbprint.Text = Config.AzureProvider.ThumbPrint;
                txtThumbprint.Left = 180;
                txtThumbprint.Top = 47;
                txtThumbprint.Width = 300;
                txtThumbprint.Validating += ThumbprintValidating;
                txtThumbprint.Validated += ThumbprintValidated;
                _txtpanel.Controls.Add(txtThumbprint);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 95;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 95;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtTenantId.Text = Config.AzureProvider.TenantId;
                txtThumbprint.Text = Config.AzureProvider.ThumbPrint;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                lblThumbPrint.Text = res.CTRLAZURETHUMPRINT + " : ";
                lblTenantID.Text = res.CTRLAZURETENANTID + " : ";

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtTenantId.Text))
                errors.SetError(txtTenantId, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtTenantId, "");

            if ((string.IsNullOrEmpty(txtThumbprint.Text)))
                errors.SetError(txtThumbprint, res.CTRLNULLOREMPTYERROR);
            else if (Certs.GetCertificate(txtThumbprint.Text, StoreLocation.LocalMachine) == null)
                errors.SetError(txtThumbprint, string.Format(res.CTRLSQLINVALIDCERTERROR, txtThumbprint.Text));
            else
                errors.SetError(txtThumbprint, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtTenantId.Enabled = isenabled;
                txtThumbprint.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        #region Tenantid
        /// <summary>
        /// TenantidValidating method
        /// </summary>
        private void TenantidValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtTenantId.Modified)
                {
                   
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtTenantId.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.AzureProvider.TenantId = txtTenantId.Text;
                    errors.SetError(txtTenantId, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTenantId, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TenantidValidated method
        /// </summary>
        private void TenantidValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AzureProvider.TenantId = txtTenantId.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTenantId, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Thumbprint
        /// <summary>
        /// ThumbprintValidating event
        /// </summary>
        private void ThumbprintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtThumbprint.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((string.IsNullOrEmpty(txtThumbprint.Text)))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                    errors.SetError(txtThumbprint, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtThumbprint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ThumbprintValidated method
        /// </summary>
        private void ThumbprintValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtThumbprint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// TOTPConfigurationControl class imlementation
    /// </summary>
    public partial class TOTPConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceTOTPViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private TextBox txtTOTPShadows;
        private TextBox txtHashAlgo;
        private ComboBox cbFormat;
        private ComboBox cbKeySize;
        private Panel _panelWiz;
        private CheckBox chkAllowMicrosoft;
        private CheckBox chkAllowGoogle;
        private CheckBox chkAllowAuthy;
        private CheckBox chkAllowSearch;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;

        private Label lblTOTPWizard;
        private Label lblMaxKeyLen;
        private Label lblSecMode;
        private Label lblHashAlgo;
        private Label lblTOTPShadows;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public TOTPConfigurationControl(ServiceTOTPViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get 
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config; 
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 250;
                this.Width = 1050;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 220;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 220;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblTOTPShadows = new Label();
                lblTOTPShadows.Text = res.CTRLGLMAXCODES + " : ";
                lblTOTPShadows.Left = 10;
                lblTOTPShadows.Top = 19;
                lblTOTPShadows.Width = 170;
                _txtpanel.Controls.Add(lblTOTPShadows);

                txtTOTPShadows = new TextBox();
                txtTOTPShadows.Text = Config.OTPProvider.TOTPShadows.ToString();
                txtTOTPShadows.Left = 180;
                txtTOTPShadows.Top = 15;
                txtTOTPShadows.Width = 20;
                txtTOTPShadows.TextAlign = HorizontalAlignment.Center;
                txtTOTPShadows.MaxLength = 2;
                txtTOTPShadows.Validating += TOTPShadowsValidating;
                txtTOTPShadows.Validated += TOTPShadowsValidated;
                _txtpanel.Controls.Add(txtTOTPShadows);

                lblHashAlgo = new Label();
                lblHashAlgo.Text = res.CTRLGLHASH + " : ";
                lblHashAlgo.Left = 10;
                lblHashAlgo.Top = 51;
                lblHashAlgo.Width = 170;
                _txtpanel.Controls.Add(lblHashAlgo);

                txtHashAlgo = new TextBox();
                txtHashAlgo.Text = Config.OTPProvider.Algorithm.ToString();
                txtHashAlgo.Left = 180;
                txtHashAlgo.Top = 47;
                txtHashAlgo.Width = 60;
                txtHashAlgo.TextAlign = HorizontalAlignment.Center;
                txtHashAlgo.MaxLength = 6;
                txtHashAlgo.CharacterCasing = CharacterCasing.Upper;
                txtHashAlgo.Validating += HashAlgoValidating;
                txtHashAlgo.Validated += HashAlgoValidated;
                _txtpanel.Controls.Add(txtHashAlgo);

                lblSecMode = new Label();
                lblSecMode.Text = res.CTRLSECKEYMODE+" : ";
                lblSecMode.Left = 10;
                lblSecMode.Top = 100;
                lblSecMode.Width = 150;
                _txtpanel.Controls.Add(lblSecMode);

                MMCSecurityFormatList lst = new MMCSecurityFormatList();
                cbFormat = new ComboBox();
                cbFormat.DropDownStyle = ComboBoxStyle.DropDownList;
                cbFormat.Left = 180;
                cbFormat.Top = 96;
                cbFormat.Width = 200;
                _txtpanel.Controls.Add(cbFormat);

                cbFormat.DataSource = lst;
                cbFormat.ValueMember = "ID";
                cbFormat.DisplayMember = "Label";
                cbFormat.SelectedValue = Config.KeysConfig.KeyFormat;
                cbFormat.SelectedIndexChanged += SelectedFormatChanged;

                lblMaxKeyLen = new Label();
                lblMaxKeyLen.Text = res.CTRLSECKEYLENGTH+" : ";
                lblMaxKeyLen.Left = 10;
                lblMaxKeyLen.Top = 132;
                lblMaxKeyLen.Width = 150;
                _txtpanel.Controls.Add(lblMaxKeyLen);

                MMCSecurityKeySizeist lkeys = new MMCSecurityKeySizeist();
                cbKeySize = new ComboBox();
                cbKeySize.DropDownStyle = ComboBoxStyle.DropDownList;
                cbKeySize.Left = 180;
                cbKeySize.Top = 128;
                cbKeySize.Width = 200;
                _txtpanel.Controls.Add(cbKeySize);

                cbKeySize.DataSource = lkeys;
                cbKeySize.ValueMember = "ID";
                cbKeySize.DisplayMember = "Label";
                cbKeySize.SelectedValue = Config.KeysConfig.KeySize;
                cbKeySize.SelectedIndexChanged += SelectedKeySizeChanged;

                _panelWiz = new Panel();
                _panelWiz.Left = 500;
                _panelWiz.Top = 10;
                _panelWiz.Height = 200;
                _panelWiz.Width = 400;
                _txtpanel.Controls.Add(_panelWiz);

                lblTOTPWizard = new Label();
                lblTOTPWizard.Text = res.CTRLSECWIZARD + " : ";
                lblTOTPWizard.Left = 10;
                lblTOTPWizard.Top = 41;
                lblTOTPWizard.Width = 250;
                _panelWiz.Controls.Add(lblTOTPWizard);

                chkAllowMicrosoft = new CheckBox();
                chkAllowMicrosoft.Text = res.CTRLGLSHOWMICROSOFT;
                chkAllowMicrosoft.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoMicrosoftAuthenticator);
                chkAllowMicrosoft.Left = 20;
                chkAllowMicrosoft.Top = 65;
                chkAllowMicrosoft.Width = 300;
                chkAllowMicrosoft.CheckedChanged += AllowMicrosoftCheckedChanged;
                _panelWiz.Controls.Add(chkAllowMicrosoft);

                chkAllowGoogle = new CheckBox();
                chkAllowGoogle.Text = res.CTRLGLSHOWGOOGLE;
                chkAllowGoogle.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGoogleAuthenticator);
                chkAllowGoogle.Left = 20;
                chkAllowGoogle.Top = 96;
                chkAllowGoogle.Width = 300;
                chkAllowGoogle.CheckedChanged += AllowGoogleCheckedChanged;
                _panelWiz.Controls.Add(chkAllowGoogle);

                chkAllowAuthy = new CheckBox();
                chkAllowAuthy.Text = res.CTRLGLSHOWAUTHY;
                chkAllowAuthy.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoAuthyAuthenticator);
                chkAllowAuthy.Left = 20;
                chkAllowAuthy.Top = 127;
                chkAllowAuthy.Width = 300;
                chkAllowAuthy.CheckedChanged += AllowAuthyCheckedChanged;
                _panelWiz.Controls.Add(chkAllowAuthy);

                chkAllowSearch = new CheckBox();
                chkAllowSearch.Text = res.CTRLGLALLOWGOOGLESEARCH;
                chkAllowSearch.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGooglSearch);
                chkAllowSearch.Left = 20;
                chkAllowSearch.Top = 158;
                chkAllowSearch.Width = 300;
                chkAllowSearch.CheckedChanged += AllowSearchGoogleCheckedChanged;
                _panelWiz.Controls.Add(chkAllowSearch);
          
                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 230;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 230;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                txtTOTPShadows.Text = Config.OTPProvider.TOTPShadows.ToString();
                txtHashAlgo.Text = Config.OTPProvider.Algorithm.ToString();

                chkAllowMicrosoft.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoMicrosoftAuthenticator);

                chkAllowGoogle.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGoogleAuthenticator);

                chkAllowAuthy.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoAuthyAuthenticator);

                chkAllowSearch.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGooglSearch);

                cbFormat.SelectedValue = Config.KeysConfig.KeyFormat;

                cbKeySize.SelectedValue = Config.KeysConfig.KeySize;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

                chkAllowSearch.Text = res.CTRLGLALLOWGOOGLESEARCH;
                chkAllowAuthy.Text = res.CTRLGLSHOWAUTHY;
                chkAllowGoogle.Text = res.CTRLGLSHOWGOOGLE;
                chkAllowMicrosoft.Text = res.CTRLGLSHOWMICROSOFT;
                lblTOTPWizard.Text = res.CTRLSECWIZARD + " : ";
                lblMaxKeyLen.Text = res.CTRLSECKEYLENGTH + " : ";
                lblSecMode.Text = res.CTRLSECKEYMODE + " : ";
                lblHashAlgo.Text = res.CTRLGLHASH + " : ";
                lblTOTPShadows.Text = res.CTRLGLMAXCODES + " : ";

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            try
            {
                errors.SetError(txtHashAlgo, "");
                HashMode hash = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
            }
            catch (Exception ex)
            {
                errors.SetError(txtHashAlgo, ex.Message);
            }
            try
            { 
                errors.SetError(txtTOTPShadows, "");
                int refr = Convert.ToInt32(txtTOTPShadows.Text);
                if (string.IsNullOrEmpty(txtTOTPShadows.Text))
                    throw new Exception(res.CTRLNULLOREMPTYERROR);
                else if ((refr < 1) || (refr > 10))
                    throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "10"));
            }
            catch (Exception ex)
            {
                errors.SetError(txtTOTPShadows, ex.Message);
            }

        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        #region HashAlgo
        /// <summary>
        /// HashAlgoValidating event
        /// </summary>
        private void HashAlgoValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtHashAlgo.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtHashAlgo.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    HashMode hash = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                    Config.OTPProvider.Algorithm = hash;
                    errors.SetError(txtHashAlgo, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtHashAlgo, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// HashAlgoValidated method implmentation
        /// </summary>
        private void HashAlgoValidated(object sender, EventArgs e)
        {
            try
            {
                Config.OTPProvider.Algorithm = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region TOTPShadows
        /// <summary>
        /// TOTPShadowsValidating event
        /// </summary>
        private void TOTPShadowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {

                if (txtTOTPShadows.Modified)
                {
                    int refr = Convert.ToInt32(txtTOTPShadows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtTOTPShadows.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 1) || (refr > 10))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "10"));
                    Config.OTPProvider.TOTPShadows = refr;
                    errors.SetError(txtTOTPShadows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTOTPShadows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TOTPShadowsValidated event
        /// </summary>
        private void TOTPShadowsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.OTPProvider.TOTPShadows = Convert.ToInt32(txtTOTPShadows.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTOTPShadows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SelectedFormatChanged method
        /// </summary>
        private void SelectedFormatChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeyFormat = (SecretKeyFormat)cbFormat.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedKeySizeChanged method
        /// </summary>
        private void SelectedKeySizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeySize = (KeySizeMode)cbKeySize.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #region Wizard Options
        /// <summary>
        /// AllowSearchGoogleCheckedChanged method implementation
        /// </summary>
        private void AllowSearchGoogleCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowSearch.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGooglSearch;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoGooglSearch;
                    errors.SetError(chkAllowSearch, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowSearch, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowAuthyCheckedChanged method implementation
        /// </summary>
        private void AllowAuthyCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowAuthy.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoAuthyAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoAuthyAuthenticator;
                    errors.SetError(chkAllowAuthy, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowAuthy, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowGoogleCheckedChanged method implementation
        /// </summary>
        private void AllowGoogleCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowGoogle.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGoogleAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoGoogleAuthenticator;
                    errors.SetError(chkAllowGoogle, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowGoogle, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowMicrosoftCheckedChanged method implementation
        /// </summary>
        private void AllowMicrosoftCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowMicrosoft.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoMicrosoftAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoMicrosoftAuthenticator;
                    errors.SetError(chkAllowMicrosoft, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowMicrosoft, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// BiometricsConfigurationControl class imlementation
    /// </summary>
    public partial class BiometricsConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceBiometricsViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblTimeOut;
        private TextBox txtTimeOut;
        private Label lblDriftTolerance;
        private TextBox txtDriftTolerance;
        private Label lblServerDomain;
        private TextBox txtServerDomain;
        private Label lblServerName;
        private TextBox txtServerName;
        private Label lblServerUri;
        private TextBox txtServerUri;
        private Label lblChallengeSize;
        private ComboBox cbChallengeSize;
        private CheckBox chkAutologin;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public BiometricsConfigurationControl(ServiceBiometricsViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 250;
                this.Width = 800;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 220;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 220;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblTimeOut = new Label
                {
                    Text = res.CTRLWEBAUTHNTIMEOUT + " : ",
                    Left = 10,
                    Top = 20,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblTimeOut);

                txtTimeOut = new TextBox
                {
                    Text = Config.WebAuthNProvider.Configuration.Timeout.ToString(),
                    Left = 190,
                    Top = 16,
                    Width = 60,
                    MaxLength = 6,
                    TextAlign = HorizontalAlignment.Center
                };
                txtTimeOut.Validating += TimeOutValidating;
                txtTimeOut.Validated += TimeOutValidated;
                _txtpanel.Controls.Add(txtTimeOut);


                chkAutologin = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNAUTOLOGIN,
                    Checked = Config.WebAuthNProvider.DirectLogin,
                    Left = 350,
                    Top = 20,
                    Width = 150
                };
                chkAutologin.CheckedChanged += ChkchkAutologinChanged;
                _txtpanel.Controls.Add(chkAutologin);

                lblDriftTolerance = new Label
                {
                    Text = res.CTRLWEBAUTHNDRIFTTOLERANCE + " : ",
                    Left = 10,
                    Top = 54,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblDriftTolerance);

                txtDriftTolerance = new TextBox
                {
                    Text = Config.WebAuthNProvider.Configuration.TimestampDriftTolerance.ToString(),
                    Left = 190,
                    Top = 50,
                    Width = 60,
                    MaxLength = 6,
                    TextAlign = HorizontalAlignment.Center
                };
                txtDriftTolerance.Validating += DriftToleranceValidating;
                txtDriftTolerance.Validated += DriftToleranceValidated;
                _txtpanel.Controls.Add(txtDriftTolerance);

                lblServerDomain = new Label
                {
                    Text = res.CTRLWEBAUTHNSERVERDOMAIN + " : ",
                    Left = 10,
                    Top = 88,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblServerDomain);

                txtServerDomain = new TextBox
                {
                    Text = Config.WebAuthNProvider.Configuration.ServerDomain,
                    Left = 190,
                    Top = 84,
                    Width = 300
                };
                txtServerDomain.Validating += ServerDomainValidating;
                txtServerDomain.Validated += ServerDomainValidated;
                _txtpanel.Controls.Add(txtServerDomain);

                lblServerName = new Label
                {
                    Text = res.CTRLWEBAUTHNSERVERNAME + " : ",
                    Left = 10,
                    Top = 122,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblServerName);

                txtServerName = new TextBox
                {
                    Text = Config.WebAuthNProvider.Configuration.ServerName,
                    Left = 190,
                    Top = 118,
                    Width = 300
                };
                txtServerName.Validating += ServerNameValidating;
                txtServerName.Validated += ServerNameValidated;
                _txtpanel.Controls.Add(txtServerName);

                lblServerUri = new Label
                {
                    Text = res.CTRLWEBAUTHNSERVERURL + " : ",
                    Left = 10,
                    Top = 156,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblServerUri);

                txtServerUri = new TextBox
                {
                    Text = Config.WebAuthNProvider.Configuration.Origin,
                    Left = 190,
                    Top = 152,
                    Width = 300
                };
                txtServerUri.Validating += ServerUriValidating;
                txtServerUri.Validated += ServerUriValidated;
                _txtpanel.Controls.Add(txtServerUri);


                lblChallengeSize = new Label
                {
                    Text = res.CTRLWEBAUTHNCHALLENGE + " : ",
                    Left = 10,
                    Top = 190,
                    Width = 180
                };
                _txtpanel.Controls.Add(lblChallengeSize);

                MMCChallengeSizeList lst = new MMCChallengeSizeList();
                cbChallengeSize = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 190,
                    Top = 186,
                    Width = 130,
                    DataSource = lst,
                    ValueMember = "ID",
                    DisplayMember = "Label",
                    SelectedValue = Config.WebAuthNProvider.Configuration.ChallengeSize,
                };
                cbChallengeSize.SelectedIndexChanged += SelectedChallengeSizeChanged;
                _txtpanel.Controls.Add(cbChallengeSize);


                tblSaveConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE,
                    Left = 20,
                    Top = 230,
                    Width = 80,
                    TabStop = true
                };
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel
                {
                    Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL,
                    Left = 110,
                    Top = 230,
                    Width = 80,
                    TabStop = true
                };
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();
                lblTimeOut.Text = res.CTRLWEBAUTHNTIMEOUT + " : ";
                lblDriftTolerance.Text = res.CTRLWEBAUTHNDRIFTTOLERANCE + " : ";
                lblServerDomain.Text = res.CTRLWEBAUTHNSERVERDOMAIN + " : ";
                lblServerName.Text = res.CTRLWEBAUTHNSERVERNAME + " : ";
                lblServerUri.Text = res.CTRLWEBAUTHNSERVERURL + " : ";
                lblChallengeSize.Text = res.CTRLWEBAUTHNCHALLENGE + " : ";

                txtTimeOut.Text = Config.WebAuthNProvider.Configuration.Timeout.ToString();
                chkAutologin.Text = res.CTRLWEBAUTHNAUTOLOGIN;
                txtDriftTolerance.Text = Config.WebAuthNProvider.Configuration.TimestampDriftTolerance.ToString();
                txtServerDomain.Text = Config.WebAuthNProvider.Configuration.ServerDomain;
                txtServerName.Text = Config.WebAuthNProvider.Configuration.ServerName;
                txtServerUri.Text = Config.WebAuthNProvider.Configuration.Origin;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            uint refr = Convert.ToUInt32(txtTimeOut.Text);
            if (string.IsNullOrEmpty(txtTimeOut.Text))
                errors.SetError(txtTimeOut, res.CTRLNULLOREMPTYERROR);
            else if ((refr < 60000) || (refr > 600000))
                errors.SetError(txtTimeOut, string.Format(res.CTRLINVALIDVALUE, "60000", "600000"));
            else
                errors.SetError(txtTimeOut, "");

            int drift = Convert.ToInt32(txtDriftTolerance.Text);
            if (string.IsNullOrEmpty(txtDriftTolerance.Text))
                errors.SetError(txtDriftTolerance, res.CTRLNULLOREMPTYERROR);
            else if ((drift < 0) || (drift > 300000))
                errors.SetError(txtDriftTolerance, string.Format(res.CTRLINVALIDVALUE, "0", "300000"));
            else
                errors.SetError(txtDriftTolerance, "");

            if (string.IsNullOrEmpty(txtServerDomain.Text))
                errors.SetError(txtServerDomain, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtServerDomain, "");

            if (string.IsNullOrEmpty(txtServerName.Text))
                errors.SetError(txtServerName, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtServerName, "");

            if (string.IsNullOrEmpty(txtServerUri.Text))
                errors.SetError(txtServerUri, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtServerUri, "");

        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        #region Timeout
        /// <summary>
        /// TimeOutValidating method implementation
        /// </summary>
        private void TimeOutValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtTimeOut.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    uint refr = Convert.ToUInt32(txtTimeOut.Text);
                    if (string.IsNullOrEmpty(txtTimeOut.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 60000) || (refr > 600000))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "60000", "600000"));
                    Config.WebAuthNProvider.Configuration.Timeout = refr;
                    errors.SetError(txtTimeOut, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTimeOut, ex.Message);
            }
        }

        /// <summary>
        /// TimeOutValidated method implementation
        /// </summary>
        private void TimeOutValidated(object sender, EventArgs e)
        {
            try
            {
                Config.WebAuthNProvider.Configuration.Timeout = Convert.ToUInt32(txtTimeOut.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTimeOut, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region DriftOfTolerance
        /// <summary>
        /// DriftToleranceValidating method implementation
        /// </summary>
        private void DriftToleranceValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtDriftTolerance.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    int refr = Convert.ToInt32(txtDriftTolerance.Text);
                    if (string.IsNullOrEmpty(txtDriftTolerance.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 0) || (refr > 300000))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "0", "300000"));
                    Config.WebAuthNProvider.Configuration.TimestampDriftTolerance = refr;
                    errors.SetError(txtDriftTolerance, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDriftTolerance, ex.Message);
            }
        }

        /// <summary>
        /// DriftToleranceValidated method implementation
        /// </summary>
        private void DriftToleranceValidated(object sender, EventArgs e)
        {
            try
            {
                Config.WebAuthNProvider.Configuration.TimestampDriftTolerance = Convert.ToInt32(txtDriftTolerance.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDriftTolerance, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region ServerDomain
        /// <summary>
        /// ServerDomainValidating method implementation
        /// </summary>
        private void ServerDomainValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtServerDomain.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtServerDomain.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.WebAuthNProvider.Configuration.ServerDomain = txtServerDomain.Text;
                    errors.SetError(txtServerDomain, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtServerDomain, ex.Message);
            }
        }

        /// <summary>
        /// ServerDomainValidated method implementation
        /// </summary>
        private void ServerDomainValidated(object sender, EventArgs e)
        {
            try
            {
                Config.WebAuthNProvider.Configuration.ServerDomain = txtServerDomain.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtServerDomain, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region ServerName
        /// <summary>
        /// ServerNameValidating method implementation
        /// </summary>
        private void ServerNameValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtServerName.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtServerName.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.WebAuthNProvider.Configuration.ServerName = txtServerName.Text;
                    errors.SetError(txtServerName, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtServerName, ex.Message);
            }
        }

        /// <summary>
        /// ServerNameValidated method implementation
        /// </summary>
        private void ServerNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.WebAuthNProvider.Configuration.ServerName = txtServerName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtServerName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region ServerUri
        /// <summary>
        /// ServerUriValidating method implementation
        /// </summary>
        private void ServerUriValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtServerUri.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtServerUri.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.WebAuthNProvider.Configuration.Origin = txtServerUri.Text;
                    errors.SetError(txtServerUri, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtServerUri, ex.Message);
            }
        }

        /// <summary>
        /// ServerNameValidated method implementation
        /// </summary>
        private void ServerUriValidated(object sender, EventArgs e)
        {
            try
            {
                Config.WebAuthNProvider.Configuration.Origin = txtServerUri.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtServerUri, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SelectedChallengeSizeChanged method implementation
        /// </summary>
        private void SelectedChallengeSizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.WebAuthNProvider.Configuration.ChallengeSize = (int)cbChallengeSize.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ChkchkAutologinChanged method implementation
        /// </summary>
        private void ChkchkAutologinChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.WebAuthNProvider.DirectLogin = chkAutologin.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

         /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// WebAuthNConfigurationControl class imlementation
    /// </summary>
    public partial class WebAuthNConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityWebAuthNViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblAttachement;
        private ComboBox cbAttachement;
        private Label lblConveyance;
        private ComboBox cbConveyance;
        private Label lblUserVerification;
        private ComboBox cbUserVerification;
        private CheckBox chkExtensions;
        private CheckBox chkUserVerificationMethod;
        private CheckBox chkUserVerificationIndex;
        private CheckBox chkLocation;
        private CheckBox chkRequireResidentKey;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public WebAuthNConfigurationControl(ServiceSecurityWebAuthNViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 320;
                this.Width = 800;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 290;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 290;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblAttachement = new Label
                {
                    Text = res.CTRLWEBAUTHNATTACHEMENT + " : ",
                    Left = 10,
                    Top = 20,
                    Width = 280
                };
                _txtpanel.Controls.Add(lblAttachement);

                MMCAttachementList lst1 = new MMCAttachementList();
                cbAttachement = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 300,
                    Top = 16,
                    Width = 150,
                    DataSource = lst1,
                    ValueMember = "ID",
                    DisplayMember = "Label",
                    SelectedValue = Config.WebAuthNProvider.Options.AuthenticatorAttachment
                };
                cbAttachement.SelectedIndexChanged += SelectedAttachementChanged;
                _txtpanel.Controls.Add(cbAttachement);

                lblConveyance = new Label
                {
                    Text = res.CTRLWEBAUTHNCONVEYANCE + " : ",
                    Left = 10,
                    Top = 54,
                    Width = 280
                };
                _txtpanel.Controls.Add(lblConveyance);

                MMCConveyancePreferenceList lst2 = new MMCConveyancePreferenceList();
                cbConveyance = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 300,
                    Top = 50,
                    Width = 150,
                    DataSource = lst2,
                    ValueMember = "ID",
                    DisplayMember = "Label",
                    SelectedValue = Config.WebAuthNProvider.Options.AttestationConveyancePreference
                };
                cbConveyance.SelectedIndexChanged += SelectedConveyanceChanged;
                _txtpanel.Controls.Add(cbConveyance);


                lblUserVerification = new Label
                {
                    Text = res.CTRLWEBAUTHNUSERVERFICATION + " : ",
                    Left = 10,
                    Top = 88,
                    Width = 280
                };
                _txtpanel.Controls.Add(lblUserVerification);

                MMCUserVerificationRequirementList lst3 = new MMCUserVerificationRequirementList();
                cbUserVerification = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 300,
                    Top = 84,
                    Width = 150,
                    DataSource = lst3,
                    ValueMember = "ID",
                    DisplayMember = "Label",
                    SelectedValue = Config.WebAuthNProvider.Options.UserVerificationRequirement
                };
                cbUserVerification.SelectedIndexChanged += SelectedUserVerificationChanged;
                _txtpanel.Controls.Add(cbUserVerification);

                chkExtensions = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNEXTENSIONS,
                    Checked = Config.WebAuthNProvider.Options.Extensions,
                    Left = 15,
                    Top = 118,
                    Width = 300
                };
                chkExtensions.CheckedChanged += ChkExtensionsChanged;
                _txtpanel.Controls.Add(chkExtensions);

                chkUserVerificationMethod = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNUSERMETHOD,
                    Checked = Config.WebAuthNProvider.Options.UserVerificationMethod,
                    Left = 15,
                    Top = 154,
                    Width = 300
                };
                chkUserVerificationMethod.CheckedChanged += chkUserVerificationMethodChanged;
                _txtpanel.Controls.Add(chkUserVerificationMethod);

                chkUserVerificationIndex = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNUSERINDEX,
                    Checked = Config.WebAuthNProvider.Options.UserVerificationIndex,
                    Left = 15,
                    Top = 188,
                    Width = 300
                };
                chkUserVerificationIndex.CheckedChanged += chkUserVerificationIndexChanged;
                _txtpanel.Controls.Add(chkUserVerificationIndex);

                chkLocation = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNUSERLOCATION,
                    Checked = Config.WebAuthNProvider.Options.Location,
                    Left = 15,
                    Top = 222,
                    Width = 300
                };
                chkLocation.CheckedChanged += chkLocationChanged;
                _txtpanel.Controls.Add(chkLocation);

                chkRequireResidentKey = new CheckBox
                {
                    Text = res.CTRLWEBAUTHNRESIDENTKEY,
                    Checked = Config.WebAuthNProvider.Options.RequireResidentKey,
                    Left = 15,
                    Top = 256,
                    Width = 300
                };
                chkRequireResidentKey.CheckedChanged += chkRequireResidentKeyChanged;
                _txtpanel.Controls.Add(chkRequireResidentKey);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 305;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 305;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();
                lblAttachement.Text = res.CTRLWEBAUTHNATTACHEMENT    + " : ";
                lblConveyance.Text = res.CTRLWEBAUTHNCONVEYANCE + " : ";
                lblUserVerification.Text = res.CTRLWEBAUTHNUSERVERFICATION + " : ";

                chkExtensions.Text = res.CTRLWEBAUTHNEXTENSIONS; 
                chkUserVerificationMethod.Text = res.CTRLWEBAUTHNUSERMETHOD;
                chkUserVerificationIndex.Text = res.CTRLWEBAUTHNUSERINDEX;
                chkLocation.Text = res.CTRLWEBAUTHNUSERLOCATION;
                chkRequireResidentKey.Text = res.CTRLWEBAUTHNRESIDENTKEY;

                cbAttachement.SelectedValue = Config.WebAuthNProvider.Options.AuthenticatorAttachment;
                cbConveyance.SelectedValue = Config.WebAuthNProvider.Options.AttestationConveyancePreference;
                cbUserVerification.SelectedValue = Config.WebAuthNProvider.Options.UserVerificationRequirement;
                chkUserVerificationMethod.Checked = Config.WebAuthNProvider.Options.UserVerificationMethod;
                chkUserVerificationIndex.Checked = Config.WebAuthNProvider.Options.UserVerificationIndex;
                chkLocation.Checked = Config.WebAuthNProvider.Options.Location;
                chkRequireResidentKey.Checked = Config.WebAuthNProvider.Options.RequireResidentKey;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// SelectedChallengeSizeChanged method implementation
        /// </summary>
        private void SelectedAttachementChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.WebAuthNProvider.Options.AuthenticatorAttachment = cbAttachement.SelectedValue.ToString();
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedConveyanceChanged method implementation
        /// </summary>
        private void SelectedConveyanceChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.WebAuthNProvider.Options.AttestationConveyancePreference = cbConveyance.SelectedValue.ToString();
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedUserVerificationChanged method implementation
        /// </summary>
        private void SelectedUserVerificationChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.WebAuthNProvider.Options.UserVerificationRequirement = cbUserVerification.SelectedValue.ToString();
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ChkExtensionsChanged method implementation
        /// </summary>
        private void ChkExtensionsChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.WebAuthNProvider.Options.Extensions = chkExtensions.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkExtensions, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkExtensions, ex.Message);
            }
        }

        /// <summary>
        /// chkUserVerificationMethodChanged method implementation
        /// </summary>
        private void chkUserVerificationMethodChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.WebAuthNProvider.Options.UserVerificationMethod = chkUserVerificationMethod.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkUserVerificationMethod, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUserVerificationMethod, ex.Message);
            }
        }

        /// <summary>
        /// chkUserVerificationIndexChanged method implementation
        /// </summary>
        private void chkUserVerificationIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.WebAuthNProvider.Options.UserVerificationIndex = chkUserVerificationIndex.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkUserVerificationIndex, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUserVerificationIndex, ex.Message);
            }
        }

        /// <summary>
        /// chkLocationChanged method implementation
        /// </summary>
        private void chkLocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.WebAuthNProvider.Options.Location = chkLocation.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkLocation, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkLocation, ex.Message);
            }
        }

        /// <summary>
        /// chkRequireResidentKeyChanged method implementation
        /// </summary>
        private void chkRequireResidentKeyChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.WebAuthNProvider.Options.RequireResidentKey = chkRequireResidentKey.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkRequireResidentKey, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkRequireResidentKey, ex.Message);
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }
    /// <summary>
    /// SecurityConfigurationRNGControl class imlementation
    /// </summary>
    public partial class SecurityConfigurationRootControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityRootViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;

        private TextBox txtDeliveryWindow;
        private TextBox txtMaxRetries;
        private Label lblDeliveryWindow;
        private Label lblMaxRetries;
        private Label lblReplayLevel;
        private ComboBox cbReplayLevel;
        private ComboBox cbLibVersion;
        private Label lblLibVersion;
        private TextBox txtXORValue;
        private Label lblXORValue;
        private Label lblWarning;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SecurityConfigurationRootControl(ServiceSecurityRootViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 230;
                this.Width = 600;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 200;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 200;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblDeliveryWindow = new Label();
                lblDeliveryWindow.Text = res.CTRLGLDELVERY + " : ";
                lblDeliveryWindow.Left = 10;
                lblDeliveryWindow.Top = 20;
                lblDeliveryWindow.Width = 200;
                _txtpanel.Controls.Add(lblDeliveryWindow);

                txtDeliveryWindow = new TextBox();
                txtDeliveryWindow.Text = Config.DeliveryWindow.ToString();
                txtDeliveryWindow.Left = 260;
                txtDeliveryWindow.Top = 16;
                txtDeliveryWindow.Width = 60;
                txtDeliveryWindow.MaxLength = 4;
                txtDeliveryWindow.TextAlign = HorizontalAlignment.Center;
                txtDeliveryWindow.Validating += DeliveryWindowValidating;
                txtDeliveryWindow.Validated += DeliveryWindowValidated;
                _txtpanel.Controls.Add(txtDeliveryWindow);

                lblMaxRetries = new Label();
                lblMaxRetries.Text = res.CTRLDLGMAXRETRIES + " : ";
                lblMaxRetries.Left = 440;
                lblMaxRetries.Top = 20;
                lblMaxRetries.Width = 150;
                _txtpanel.Controls.Add(lblMaxRetries);

                txtMaxRetries = new TextBox();
                txtMaxRetries.Text = Config.MaxRetries.ToString();
                txtMaxRetries.Left = 600;
                txtMaxRetries.Top = 16;
                txtMaxRetries.Width = 40;
                txtMaxRetries.MaxLength = 2;
                txtMaxRetries.Validating += MaxRetriesValidating;
                txtMaxRetries.Validated += MaxRetriesValidated;
                txtMaxRetries.TextAlign = HorizontalAlignment.Center;
                _txtpanel.Controls.Add(txtMaxRetries);

                lblReplayLevel = new Label();
                lblReplayLevel.Text = res.CTRLGLREPLAY + " : ";
                lblReplayLevel.Left = 10;
                lblReplayLevel.Top = 57;
                lblReplayLevel.Width = 180;
                _txtpanel.Controls.Add(lblReplayLevel);

                MMCReplayModeList lst = new MMCReplayModeList();
                cbReplayLevel = new ComboBox();
                cbReplayLevel.DropDownStyle = ComboBoxStyle.DropDownList;
                cbReplayLevel.Left = 260;
                cbReplayLevel.Top = 53;
                cbReplayLevel.Width = 130;
                _txtpanel.Controls.Add(cbReplayLevel);

                cbReplayLevel.DataSource = lst;
                cbReplayLevel.ValueMember = "ID";
                cbReplayLevel.DisplayMember = "Label";
                cbReplayLevel.SelectedValue = Config.ReplayLevel;
                cbReplayLevel.SelectedIndexChanged += SelectedReplayLevelChanged;

                lblLibVersion = new Label();
                lblLibVersion.Text = res.CTRLSECLIBLABEL +"(*)"+ " : ";
                lblLibVersion.Left = 10;
                lblLibVersion.Top = 104;
                lblLibVersion.Width = 250;
                _txtpanel.Controls.Add(lblLibVersion);

                MMCLibVersionList lst2 = new MMCLibVersionList();
                cbLibVersion = new ComboBox();
                cbLibVersion.DropDownStyle = ComboBoxStyle.DropDownList;
                cbLibVersion.Left = 260;
                cbLibVersion.Top = 100;
                cbLibVersion.Width = 50;
                _txtpanel.Controls.Add(cbLibVersion);

                cbLibVersion.DataSource = lst2;
                cbLibVersion.ValueMember = "ID";
                cbLibVersion.DisplayMember = "Label";
                cbLibVersion.SelectedValue = Config.KeysConfig.KeyVersion;
                cbLibVersion.SelectedIndexChanged += SelectedLibVersionChanged;

                lblXORValue = new Label();
                lblXORValue.Text = res.CTRLSECXORLABEL + "(*)" + " : ";
                lblXORValue.Left = 10;
                lblXORValue.Top = 141;
                lblXORValue.Width = 250;
                _txtpanel.Controls.Add(lblXORValue);

                txtXORValue = new TextBox();
                txtXORValue.Text = Config.KeysConfig.XORSecret;
                txtXORValue.Left = 260;
                txtXORValue.Top = 137;
                txtXORValue.Width = 300;
                txtXORValue.PasswordChar = '*';
                txtXORValue.Validating += XORValueValidating;
                txtXORValue.Validated += XORValueValidated;
                _txtpanel.Controls.Add(txtXORValue);


                lblWarning = new Label();
                lblWarning.Text = "(*) "+ res.CTRLSECWARNING;
                lblWarning.Left = 10;
                lblWarning.Top = 178;
                lblWarning.Width = 500;
                _txtpanel.Controls.Add(lblWarning);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 210;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 210;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                lblDeliveryWindow.Text = res.CTRLGLDELVERY + " : ";
                lblMaxRetries.Text = res.CTRLDLGMAXRETRIES + " : ";
                lblReplayLevel.Text = res.CTRLGLREPLAY + " : ";
                lblLibVersion.Text = res.CTRLSECLIBLABEL + "(*)" + " : ";
                lblXORValue.Text = res.CTRLSECXORLABEL + "(*)" + " : ";
                lblWarning.Text = "(*) " + res.CTRLSECWARNING;

                txtDeliveryWindow.Text = Config.DeliveryWindow.ToString();
                txtMaxRetries.Text = Config.MaxRetries.ToString();
                cbReplayLevel.SelectedValue = Config.ReplayLevel;
                cbLibVersion.SelectedValue = Config.KeysConfig.KeyVersion;
                txtXORValue.Text = Config.KeysConfig.XORSecret;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            int refr = Convert.ToInt32(txtDeliveryWindow.Text);
            if (string.IsNullOrEmpty(txtDeliveryWindow.Text))
                errors.SetError(txtDeliveryWindow, res.CTRLNULLOREMPTYERROR);
            else if ((refr < 60) || (refr > 600))
                errors.SetError(txtDeliveryWindow, string.Format(res.CTRLINVALIDVALUE, "60", "600"));
            else
                errors.SetError(txtDeliveryWindow, "");

            if (string.IsNullOrEmpty(txtXORValue.Text))
                errors.SetError(txtXORValue, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtXORValue, "");

            int ref2 = Convert.ToInt32(txtMaxRetries.Text);
            if (string.IsNullOrEmpty(txtMaxRetries.Text))
                errors.SetError(txtMaxRetries, res.CTRLNULLOREMPTYERROR);
            else if ((ref2 < 1) || (ref2 > 12))
                errors.SetError(txtMaxRetries, string.Format(res.CTRLINVALIDVALUE, "1", "12"));
            else
                errors.SetError(txtMaxRetries, "");
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                if (Config.KeysConfig.KeyVersion != SecretKeyVersion.V2)
                    txtXORValue.Enabled = false;
                else
                    txtXORValue.Enabled = true;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        #region DeliveryWindow
        /// <summary>
        /// DeliveryWindowValidating method implementation
        /// </summary>
        private void DeliveryWindowValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtDeliveryWindow.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    int refr = Convert.ToInt32(txtDeliveryWindow.Text);
                    if (string.IsNullOrEmpty(txtDeliveryWindow.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 60) || (refr > 600))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "60", "600"));
                    Config.DeliveryWindow = refr;
                    errors.SetError(txtDeliveryWindow, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDeliveryWindow, ex.Message);
            }
        }

        /// <summary>
        /// DeliveryWindowValidated method implementation
        /// </summary>
        private void DeliveryWindowValidated(object sender, EventArgs e)
        {
            try
            {
                Config.DeliveryWindow = Convert.ToInt32(txtDeliveryWindow.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDeliveryWindow, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MaxRetries
        /// <summary>
        /// MaxRetriesValidating method implementation
        /// </summary>
        private void MaxRetriesValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtMaxRetries.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    int refr = Convert.ToInt32(txtMaxRetries.Text);
                    if (string.IsNullOrEmpty(txtMaxRetries.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 1) || (refr > 12))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "12"));
                    Config.MaxRetries = refr;
                    errors.SetError(txtMaxRetries, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRetries, ex.Message);
            }
        }

        /// <summary>
        /// MaxRetriesValidated method implementation
        /// </summary>
        private void MaxRetriesValidated(object sender, EventArgs e)
        {
            try
            {
                ManagementService.ADFSManager.SetDirty(true);
                Config.MaxRetries = Convert.ToInt32(txtMaxRetries.Text);
                errors.SetError(txtMaxRetries, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SelectedReplayLevelChanged method implementation
        /// </summary>
        private void SelectedReplayLevelChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.ReplayLevel = (ReplayLevel)cbReplayLevel.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedLibVersionChanged method implementation
        /// </summary>
        private void SelectedLibVersionChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeyVersion = (SecretKeyVersion)cbLibVersion.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// XORValueValidated method implementation
        /// </summary>
        private void XORValueValidated(object sender, EventArgs e)
        {
            try
            {
                ManagementService.ADFSManager.SetDirty(true);
                Config.KeysConfig.XORSecret = txtXORValue.Text;
                errors.SetError(txtXORValue, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// XORValueValidating method implementation
        /// </summary>
        private void XORValueValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtXORValue.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtXORValue.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.KeysConfig.XORSecret = txtXORValue.Text;
                    errors.SetError(txtXORValue, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtXORValue, ex.Message);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// SecurityConfigurationRNGControl class imlementation
    /// </summary>
    public partial class SecurityConfigurationRNGControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityRNGViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Panel _panelRNG;
        private Label lblRNGKey;
        private ComboBox cbKeyMode;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SecurityConfigurationRNGControl(ServiceSecurityRNGViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 100;
                this.Width = 400;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 70;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 70;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                _panelRNG = new Panel();
                _panelRNG.Left = 0;
                _panelRNG.Top = 10;
                _panelRNG.Height = 60;
                _panelRNG.Width = 400;
                _txtpanel.Controls.Add(_panelRNG);

                Label lblRNG = new Label();
                lblRNG.Text = "RNG (Random Number Generator)";
                lblRNG.Left = 10;
                lblRNG.Top = 0;
                lblRNG.Width = 250;
                _panelRNG.Controls.Add(lblRNG);

                lblRNGKey = new Label();
                lblRNGKey.Text = res.CTRLSECKEYGEN + " : ";
                lblRNGKey.Left = 30;
                lblRNGKey.Top = 27;
                lblRNGKey.Width = 140;
                _panelRNG.Controls.Add(lblRNGKey);

                MMCSecurityKeyGeneratorList lgens = new MMCSecurityKeyGeneratorList();
                cbKeyMode = new ComboBox();
                cbKeyMode.DropDownStyle = ComboBoxStyle.DropDownList;
                cbKeyMode.Left = 180;
                cbKeyMode.Top = 25;
                cbKeyMode.Width = 80;
                _panelRNG.Controls.Add(cbKeyMode);

                cbKeyMode.DataSource = lgens;
                cbKeyMode.ValueMember = "ID";
                cbKeyMode.DisplayMember = "Label";
                cbKeyMode.SelectedValue = Config.KeysConfig.KeyGenerator;
                cbKeyMode.SelectedIndexChanged += SelectedKeyGenChanged;

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 80;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 80;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                cbKeyMode.SelectedValue = Config.KeysConfig.KeyGenerator;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                lblRNGKey.Text = res.CTRLSECKEYLENGTH + " : ";

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {

        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// SelectedKeyGenChanged method
        /// </summary>
        private void SelectedKeyGenChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeyGenerator = (KeyGeneratorMode)cbKeyMode.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// SecurityConfigurationRSAControl class imlementation
    /// </summary>
    public partial class SecurityConfigurationRSAControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityRSAViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private NumericUpDown txtCERTDuration;
        private Panel _panelRSA;
        private TextBox txtRSAThumb;
        private Button btnRSACert;

        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private CheckBox chkUseOneCertPerUser;
        private Label lblRSAKey;
        private Label lblCERTDuration;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SecurityConfigurationRSAControl(ServiceSecurityRSAViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
               // _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 175;
                this.Width = 1050;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 135;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 135;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                _panelRSA = new Panel();
                _panelRSA.Left = 0;
                _panelRSA.Top = 10;
                _panelRSA.Height = 135;
                _panelRSA.Width = 1050;
                _txtpanel.Controls.Add(_panelRSA);

                Label lblRSA = new Label();
                lblRSA.Text = "RSA (Rivest Shamir Adleman)";
                lblRSA.Left = 10;
                lblRSA.Top = 0;
                lblRSA.Width = 250;
                _panelRSA.Controls.Add(lblRSA);

                lblCERTDuration = new Label();
                lblCERTDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCERTDuration.Left = 30;
                lblCERTDuration.Top = 27;
                lblCERTDuration.Width = 140;
                _panelRSA.Controls.Add(lblCERTDuration);

                txtCERTDuration = new NumericUpDown();
                txtCERTDuration.Left = 180;
                txtCERTDuration.Top = 24;
                txtCERTDuration.Width = 50;
                txtCERTDuration.TextAlign = HorizontalAlignment.Center;
                txtCERTDuration.Value = Config.KeysConfig.CertificateValidity;
                txtCERTDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtCERTDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCERTDuration.ValueChanged += CertValidityChanged;
                _panelRSA.Controls.Add(txtCERTDuration);

                chkUseOneCertPerUser = new CheckBox();
                chkUseOneCertPerUser.Text = res.CTRLSECCERTPERUSER;
                chkUseOneCertPerUser.Checked = Config.KeysConfig.CertificatePerUser;
                chkUseOneCertPerUser.Left = 30;
                chkUseOneCertPerUser.Top = 56;
                chkUseOneCertPerUser.Width = 450;
                chkUseOneCertPerUser.CheckedChanged += UseOneCertPerUserCheckedChanged;
                _panelRSA.Controls.Add(chkUseOneCertPerUser);


                lblRSAKey = new Label();
                lblRSAKey.Text = res.CTRLSECTHUMPRINT + " : ";
                lblRSAKey.Left = 30;
                lblRSAKey.Top = 93;
                lblRSAKey.Width = 140;
                _panelRSA.Controls.Add(lblRSAKey);

                txtRSAThumb = new TextBox();
                if (!string.IsNullOrEmpty(Config.KeysConfig.CertificateThumbprint))
                    txtRSAThumb.Text = Config.KeysConfig.CertificateThumbprint.ToUpper();
                txtRSAThumb.Left = 180;
                txtRSAThumb.Top = 90;
                txtRSAThumb.Width = 300;
                txtRSAThumb.Enabled = !Config.KeysConfig.CertificatePerUser;
                txtRSAThumb.Validating += RSAThumbValidating;
                txtRSAThumb.Validated += RSAThumbValidated;
                _panelRSA.Controls.Add(txtRSAThumb);

                btnRSACert = new Button();
                btnRSACert.Text = res.CTRLSECNEWCERT;
                btnRSACert.Left = 680;
                btnRSACert.Top = 88;
                btnRSACert.Width = 250;
                btnRSACert.Enabled = !Config.KeysConfig.CertificatePerUser;
                btnRSACert.Click += BtnRSACertClick;
                _panelRSA.Controls.Add(btnRSACert);


                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 145;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 145;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtCERTDuration.Value = Config.KeysConfig.CertificateValidity;
                chkUseOneCertPerUser.Checked = Config.KeysConfig.CertificatePerUser;

                if (!string.IsNullOrEmpty(Config.KeysConfig.CertificateThumbprint))
                    txtRSAThumb.Text = Config.KeysConfig.CertificateThumbprint.ToUpper();
                else
                    txtRSAThumb.Text = string.Empty;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

                chkUseOneCertPerUser.Text = res.CTRLSECCERTPERUSER;
                btnRSACert.Text = res.CTRLSECNEWCERT;
                lblRSAKey.Text = res.CTRLSECTHUMPRINT + " : ";
                lblCERTDuration.Text = res.CTRLSECCERTIFDURATION + " : ";

                txtRSAThumb.Enabled = !Config.KeysConfig.CertificatePerUser;
                btnRSACert.Enabled = !Config.KeysConfig.CertificatePerUser;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            if (txtRSAThumb.Enabled)
            {
                if (string.IsNullOrEmpty(txtRSAThumb.Text))
                    errors.SetError(txtRSAThumb, res.CTRLNULLOREMPTYERROR);
                else if (!ManagementService.ADFSManager.CheckCertificate(txtRSAThumb.Text))
                    errors.SetError(txtRSAThumb, res.CTRLSECINVALIDCERT);
                else
                    errors.SetError(txtRSAThumb, "");
            }
            else
                errors.SetError(txtRSAThumb, "");
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtRSAThumb.Enabled = !Config.KeysConfig.CertificatePerUser;
                btnRSACert.Enabled = !Config.KeysConfig.CertificatePerUser;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }        

        /// <summary>
        /// CertValidityChanged method 
        /// </summary>
        private void CertValidityChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.CertificateValidity = Convert.ToInt32(txtCERTDuration.Value);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseOneCertPerUserCheckedChanged method 
        /// </summary>
        private void UseOneCertPerUserCheckedChanged(object sender, EventArgs e)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                try
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.CertificatePerUser = chkUseOneCertPerUser.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkUseOneCertPerUser, "");
                }
                catch (Exception ex)
                {
                    errors.SetError(chkUseOneCertPerUser, ex.Message);
                }
            }
        }

        /// <summary>
        /// RSAThumbValidating method implmentation
        /// </summary>
        private void RSAThumbValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtRSAThumb.Modified) && (txtRSAThumb.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtRSAThumb.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!ManagementService.ADFSManager.CheckCertificate(txtRSAThumb.Text))
                        throw new Exception(res.CTRLSECINVALIDCERT);
                    Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text.ToUpper();
                    errors.SetError(txtRSAThumb, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtRSAThumb, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// RSAThumbValidated method implmentation
        /// </summary>
        private void RSAThumbValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text.ToUpper(); ;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtRSAThumb, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnRSACertClick method implmentation
        /// </summary>
        private void BtnRSACertClick(object sender, EventArgs e)
        {
            try
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = res.CTRLSECRSAGENERATE;
                messageBoxParameters.Buttons = MessageBoxButtons.OKCancel;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                bool result = (this._snapin.Console.ShowDialog(messageBoxParameters) == DialogResult.OK);
                Cursor curs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (result)
                    {
                        this.txtRSAThumb.Text = ManagementService.ADFSManager.RegisterNewRSACertificate(null, Config.KeysConfig.CertificateValidity);

                        MessageBoxParameters messageBoxParameters2 = new MessageBoxParameters();
                        messageBoxParameters2.Text = string.Format(res.CTRLSECNEWCERTCREATED, this.txtRSAThumb.Text);
                        messageBoxParameters2.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters2.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters2);

                    }
                }
                finally
                {
                    this.Cursor = curs;
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    /// <summary>
    /// SecurityConfigurationRSAControl class imlementation
    /// </summary>
    public partial class SecurityConfigurationRSAXControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityRSAXViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;


        private Panel _panelCUSTOM;
        private TextBox txtDLLCUST;
        private TextBox txtParams;
        private Button btnCUSTOMDB;
        private CheckBox chkUseAlwaysEncryptSQL;
        private TextBox txtEncryptKeyName;
        private NumericUpDown txtCertificateDuration;
        private CheckBox chkReuseCertificate;
        private TextBox txtCertificateThumbPrint;
        private Button btnCreateCryptedDB;
        private TextBox txtConnectionString;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblCertificateThumbPrint;
        private Label lblCertificateDuration;
        private Label lblEncryptKeyName;
        private Label lblParams;
        private Label lblConnectionString;
        private Label lblDLL;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SecurityConfigurationRSAXControl(ServiceSecurityRSAXViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    IExternalProvider _provider = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (!_provider.Enabled)
                        _panel.BackColor = Color.DarkGray;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
               // _view.RefreshProviderInformation();

                this.Dock = DockStyle.Top;
                this.Height = 410;
                this.Width = 1050;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 330;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 330;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                _panelCUSTOM = new Panel();
                _panelCUSTOM.Left = 0;
                _panelCUSTOM.Top = 10;
                _panelCUSTOM.Height = 320;
                _panelCUSTOM.Width = 1050;
                _txtpanel.Controls.Add(_panelCUSTOM);

                Label lblRSACUST = new Label();
                lblRSACUST.Text = "RSA CUSTOM (One certificate per user)";
                lblRSACUST.Left = 10;
                lblRSACUST.Top = 0;
                lblRSACUST.Width = 250;
                _panelCUSTOM.Controls.Add(lblRSACUST);

                lblDLL = new Label();
                lblDLL.Text = res.CTRLSECASSEMBLY + " : ";
                lblDLL.Left = 30;
                lblDLL.Top = 27;
                lblDLL.Width = 150;
                _panelCUSTOM.Controls.Add(lblDLL);

                txtDLLCUST = new TextBox();
                txtDLLCUST.Text = Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation;
                txtDLLCUST.Left = 180;
                txtDLLCUST.Top = 23;
                txtDLLCUST.Width = 820;
                txtDLLCUST.Validating += DLLValidating;
                txtDLLCUST.Validated += DLLValidated;
                _panelCUSTOM.Controls.Add(txtDLLCUST);

                lblConnectionString = new Label();
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblConnectionString.Left = 30;
                lblConnectionString.Top = 58;
                lblConnectionString.Width = 150;
                _panelCUSTOM.Controls.Add(lblConnectionString);

                txtConnectionString = new TextBox();
                txtConnectionString.Text = Config.KeysConfig.ExternalKeyManager.ConnectionString;
                txtConnectionString.Left = 180;
                txtConnectionString.Top = 54;
                txtConnectionString.Width = 820;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;
                txtConnectionString.Validating += ConnectionStringValidating;
                txtConnectionString.Validated += ConnectionStringValidated;
                _panelCUSTOM.Controls.Add(txtConnectionString);

                lblParams = new Label();
                lblParams.Text = res.CTRLSECPARAMS + " : ";
                lblParams.Left = 30;
                lblParams.Top = 85;
                lblParams.Width = 150;
                _panelCUSTOM.Controls.Add(lblParams);

                txtParams = new TextBox();
                txtParams.Text = Config.KeysConfig.ExternalKeyManager.Parameters.Data;
                txtParams.Left = 180;
                txtParams.Top = 85;
                txtParams.Width = 820;
                txtParams.Height = 60;
                txtParams.Multiline = true;
                txtParams.Validating += ParamsValidating;
                txtParams.Validated += ParamsValidated;
                _panelCUSTOM.Controls.Add(txtParams);

                btnCUSTOMDB = new Button();
                btnCUSTOMDB.Text = res.CTRLSECNEWDATABASE;
                btnCUSTOMDB.Left = 680;
                btnCUSTOMDB.Top = 160;
                btnCUSTOMDB.Width = 250;
                btnCUSTOMDB.Click += BtnCUSTOMDBClick;
                _panelCUSTOM.Controls.Add(btnCUSTOMDB);

                chkUseAlwaysEncryptSQL = new CheckBox();
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;
                chkUseAlwaysEncryptSQL.Left = 10;
                chkUseAlwaysEncryptSQL.Top = 161;
                chkUseAlwaysEncryptSQL.Width = 450;
                chkUseAlwaysEncryptSQL.CheckedChanged += UseSQLCryptCheckedChanged;
                _panelCUSTOM.Controls.Add(chkUseAlwaysEncryptSQL);

                lblEncryptKeyName = new Label();
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblEncryptKeyName.Left = 50;
                lblEncryptKeyName.Top = 192;
                lblEncryptKeyName.Width = 150;
                _panelCUSTOM.Controls.Add(lblEncryptKeyName);

                txtEncryptKeyName = new TextBox();
                txtEncryptKeyName.Text = Config.KeysConfig.ExternalKeyManager.KeyName;
                txtEncryptKeyName.Left = 210;
                txtEncryptKeyName.Top = 188;
                txtEncryptKeyName.Width = 100;
                txtEncryptKeyName.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                txtEncryptKeyName.Validating += EncryptKeyNameValidating;
                txtEncryptKeyName.Validated += EncryptKeyNameValidated;
                _panelCUSTOM.Controls.Add(txtEncryptKeyName);

                lblCertificateDuration = new Label();
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateDuration.Left = 50;
                lblCertificateDuration.Top = 223;
                lblCertificateDuration.Width = 150;
                _panelCUSTOM.Controls.Add(lblCertificateDuration);

                txtCertificateDuration = new NumericUpDown();
                txtCertificateDuration.Left = 210;
                txtCertificateDuration.Top = 219;
                txtCertificateDuration.Width = 50;
                txtCertificateDuration.TextAlign = HorizontalAlignment.Center;
                txtCertificateDuration.Value = Config.KeysConfig.ExternalKeyManager.CertificateValidity;
                txtCertificateDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtCertificateDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCertificateDuration.ValueChanged += CertCryptValidityChanged;
                txtCertificateDuration.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                _panelCUSTOM.Controls.Add(txtCertificateDuration);

                chkReuseCertificate = new CheckBox();
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.KeysConfig.ExternalKeyManager.CertReuse;
                chkReuseCertificate.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                chkReuseCertificate.Left = 50;
                chkReuseCertificate.Top = 254;
                chkReuseCertificate.Width = 450;
                chkReuseCertificate.CheckedChanged += UseSQLReuseCertCheckedChanged;
                _panelCUSTOM.Controls.Add(chkReuseCertificate);

                lblCertificateThumbPrint = new Label();
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                lblCertificateThumbPrint.Left = 100;
                lblCertificateThumbPrint.Top = 285;
                lblCertificateThumbPrint.Width = 150;
                _panelCUSTOM.Controls.Add(lblCertificateThumbPrint);

                txtCertificateThumbPrint = new TextBox();
                if (!string.IsNullOrEmpty(Config.KeysConfig.ExternalKeyManager.ThumbPrint))
                    txtCertificateThumbPrint.Text = Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper();
                txtCertificateThumbPrint.Left = 260;
                txtCertificateThumbPrint.Top = 281;
                txtCertificateThumbPrint.Width = 300;
                txtCertificateThumbPrint.Enabled = (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted && Config.KeysConfig.ExternalKeyManager.CertReuse);
                txtCertificateThumbPrint.Validating += CertificateThumbPrintValidating;
                txtCertificateThumbPrint.Validated += CertificateThumbPrintValidated;
                _panelCUSTOM.Controls.Add(txtCertificateThumbPrint);

                btnCreateCryptedDB = new Button();
                btnCreateCryptedDB.Text = res.CTRLSECNEWCRYPTEDDATABASE;
                btnCreateCryptedDB.Left = 680;
                btnCreateCryptedDB.Top = 281;
                btnCreateCryptedDB.Width = 250;
                btnCreateCryptedDB.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                btnCreateCryptedDB.Click += BtnCUSTOMCRYPTEDDBClick;
                _panelCUSTOM.Controls.Add(btnCreateCryptedDB);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 340;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 340;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtDLLCUST.Text = Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation;
                txtConnectionString.Text = Config.KeysConfig.ExternalKeyManager.ConnectionString;
                txtParams.Text = Config.KeysConfig.ExternalKeyManager.Parameters.Data;

                chkUseAlwaysEncryptSQL.Checked = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                txtEncryptKeyName.Text = Config.KeysConfig.ExternalKeyManager.KeyName;
                txtEncryptKeyName.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                txtCertificateDuration.Value = Config.KeysConfig.ExternalKeyManager.CertificateValidity;
                txtCertificateDuration.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                chkReuseCertificate.Checked = Config.KeysConfig.ExternalKeyManager.CertReuse;
                chkReuseCertificate.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                if (!string.IsNullOrEmpty(Config.KeysConfig.ExternalKeyManager.ThumbPrint))
                    txtCertificateThumbPrint.Text = Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper();
                else
                    txtCertificateThumbPrint.Text = string.Empty;

                txtCertificateThumbPrint.Enabled = (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted && Config.KeysConfig.ExternalKeyManager.CertReuse);

                btnCreateCryptedDB.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

                btnCreateCryptedDB.Text = res.CTRLSECNEWCRYPTEDDATABASE;
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                btnCUSTOMDB.Text = res.CTRLSECNEWDATABASE;
                lblParams.Text = res.CTRLSECPARAMS + " : ";
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblDLL.Text = res.CTRLSECASSEMBLY + " : ";
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtDLLCUST.Text))
                errors.SetError(txtDLLCUST, res.CTRLNULLOREMPTYERROR);
            else if (!AssemblyParser.CheckKeysAssembly(txtDLLCUST.Text))
                errors.SetError(txtDLLCUST, res.CTRLSECINVALIDEXTERROR);
            else
                errors.SetError(txtDLLCUST, "");

            if (txtConnectionString.Enabled)
            {
                if (!ManagementService.CheckKeysConnection(txtConnectionString.Text))
                    errors.SetError(txtConnectionString, res.CTRLSQLCONNECTSTRERROR);
                else
                    errors.SetError(txtConnectionString, "");
            }
            else
                errors.SetError(txtConnectionString, "");

            if (txtEncryptKeyName.Enabled)
            {
                if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                    errors.SetError(txtEncryptKeyName, res.CTRLNULLOREMPTYERROR);
                else
                    errors.SetError(txtEncryptKeyName, "");
            }
            else
                errors.SetError(txtEncryptKeyName, "");

            if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
            {
                if (txtCertificateThumbPrint.Enabled)
                {
                    if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                        errors.SetError(txtCertificateThumbPrint, String.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                    else
                        errors.SetError(txtCertificateThumbPrint, "");
                }
                else
                    errors.SetError(txtCertificateThumbPrint, "");
            }
            else
                errors.SetError(txtCertificateThumbPrint, "");
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                if (chkUseAlwaysEncryptSQL.Checked)
                {
                    this.txtEncryptKeyName.Enabled = true;
                    this.txtCertificateDuration.Enabled = true;
                    this.chkReuseCertificate.Enabled = true;
                    if (this.chkReuseCertificate.Checked)
                        this.txtCertificateThumbPrint.Enabled = true;
                    else
                        this.txtCertificateThumbPrint.Enabled = false;
                    this.btnCreateCryptedDB.Enabled = true;
                }
                else
                {
                    this.txtEncryptKeyName.Enabled = false;
                    this.txtCertificateDuration.Enabled = false;
                    this.chkReuseCertificate.Enabled = false;
                    this.txtCertificateThumbPrint.Enabled = false;
                    this.btnCreateCryptedDB.Enabled = false;
                }
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// DLLValidating method implementation
        /// </summary>
        private void DLLValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtDLLCUST.Modified) && (txtDLLCUST.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtDLLCUST.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!AssemblyParser.CheckKeysAssembly(txtDLLCUST.Text))
                        throw new Exception(res.CTRLSECINVALIDEXTERROR);
                    Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                    errors.SetError(txtDLLCUST, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLLCUST, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DLLValidated method implmentation
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDLLCUST, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ConnectionStringValidating method implmentation
        /// </summary>
        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtConnectionString.Modified) && (txtConnectionString.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckKeysConnection(txtConnectionString.Text))
                        throw new Exception(res.CTRLSQLCONNECTSTRERROR);
                    Config.KeysConfig.ExternalKeyManager.ConnectionString = txtConnectionString.Text;
                    errors.SetError(txtConnectionString, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtConnectionString, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ConnectionStringValidated method implmentation
        /// </summary>
        private void ConnectionStringValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.ConnectionString = txtConnectionString.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtConnectionString, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ParamsValidating method implmentation
        /// </summary>
        private void ParamsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtParams.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                    errors.SetError(txtParams, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ParamsValidated method implmentation
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtParams, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnCUSTOMDBClick 
        /// </summary>
        private void BtnCUSTOMDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            Wizard.Text = res.CTRLSECWIZTITLE;
            try
            {
                bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                Cursor curs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (result)
                    {
                        this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFASecretKeysDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                        messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                        messageBoxParameters.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters);
                    }
                }
                finally
                {
                    this.Cursor = curs;
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLCryptCheckedChanged method implementation
        /// </summary>
        private void UseSQLCryptCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted = chkUseAlwaysEncryptSQL.Checked;
                    UpdateConnectionString(chkUseAlwaysEncryptSQL.Checked);
                    UpdateControlsLayouts();
                    errors.SetError(chkUseAlwaysEncryptSQL, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUseAlwaysEncryptSQL, ex.Message);
            }
        }

        /// <summary>
        /// UpdateConnectionString method implmentation
        /// </summary>
        private void UpdateConnectionString(bool crypted)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                string cs = txtConnectionString.Text;
                if (!crypted)
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                    cs += ";Column Encryption Setting=enabled";
                }
                txtConnectionString.Text = cs;
            }
        }

        /// <summary>
        /// UseSQLReuseCertCheckedChanged method implementaton
        /// </summary>
        private void UseSQLReuseCertCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.CertReuse = chkReuseCertificate.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkReuseCertificate, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkReuseCertificate, ex.Message);
            }
        }

        /// <summary>
        /// CertValidityChanged method implementation
        /// </summary>
        private void CertCryptValidityChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.CertificateValidity = Convert.ToInt32(txtCertificateDuration.Value);
                    errors.SetError(txtCertificateDuration, "");
                }

            }
            catch (Exception ex)
            {
                errors.SetError(txtCertificateDuration, ex.Message);
            }
        }

        /// <summary>
        /// EncryptKeyNameValidated method implementation
        /// </summary>
        private void EncryptKeyNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.KeyName = txtEncryptKeyName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEncryptKeyName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// EncryptKeyNameValidating method implmentation
        /// </summary>
        private void EncryptKeyNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtEncryptKeyName.Modified) && (txtEncryptKeyName.Enabled))
                {
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        throw new ArgumentException(res.CTRLNULLOREMPTYERROR, "KeyName");
                    Config.KeysConfig.ExternalKeyManager.KeyName = txtEncryptKeyName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                    errors.SetError(txtEncryptKeyName, "");
                }
                UpdateControlsLayouts();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEncryptKeyName, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidated method implementation
        /// </summary>
        private void CertificateThumbPrintValidated(object sender, EventArgs e)
        {
            try
            {
                ManagementService.ADFSManager.SetDirty(true);
                Config.KeysConfig.ExternalKeyManager.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                errors.SetError(txtCertificateThumbPrint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidating method implementation
        /// </summary>
        private void CertificateThumbPrintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtCertificateThumbPrint.Modified) && (txtCertificateThumbPrint.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            throw new ArgumentException(String.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text), "Certificate ThumbPrint");
                    }
                    Config.KeysConfig.ExternalKeyManager.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                    errors.SetError(txtCertificateThumbPrint, "");
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCertificateThumbPrint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// btnCUSTOMCRYPTEDDBClick event
        /// </summary>
        private void BtnCUSTOMCRYPTEDDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            Wizard.Text = res.CTRLSECWIZTITLE;
            try
            {
                if (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted)
                {
                    bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                    Cursor curs = this.Cursor;
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        if (result)
                        {
                            bool isnew = false;
                            string thumb = string.Empty;
                            if ((Config.KeysConfig.ExternalKeyManager.CertReuse) && (Certs.GetCertificate(Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper(), StoreLocation.LocalMachine)) != null)
                                thumb = Config.Hosts.SQLServerHost.ThumbPrint.ToUpper();
                            else
                            {
                                thumb = ManagementService.ADFSManager.RegisterNewSQLCertificate(null, Config.KeysConfig.ExternalKeyManager.CertificateValidity, Config.KeysConfig.ExternalKeyManager.KeyName);
                                isnew = true;
                            }
                            this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFAEncryptedSecretKeysDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text, Config.KeysConfig.ExternalKeyManager.KeyName, thumb);

                            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                            if (isnew)
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED2, Wizard.txtDBName.Text, thumb);
                            else
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                            messageBoxParameters.Buttons = MessageBoxButtons.OK;
                            messageBoxParameters.Icon = MessageBoxIcon.Information;
                            this._snapin.Console.ShowDialog(messageBoxParameters);
                        }
                    }
                    finally
                    {
                        this.Cursor = curs;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }
    internal static class AssemblyParser
    {
        /// <summary>
        /// CheckSMSAssembly method implmentation
        /// </summary>
        internal static bool CheckSMSAssembly(string fqiassembly)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(fqiassembly));
                Type _typetoload = assembly.GetType(ParseType(fqiassembly));
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) != null); // Allow Calling internal Constructors
                else if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) != null); // Allow Calling internal Constructors
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// CheckKeysAssembly method implmentation
        /// </summary>
        internal static bool CheckKeysAssembly(string fqiassembly)
        {
            try
            {
                if (Utilities.LoadExternalKeyManagerCreator(fqiassembly) == null)
                    if (Utilities.LoadExternalKeyManagerWrapper(fqiassembly) == null)
                        return false;
                    else
                        return true;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }
    }
}
