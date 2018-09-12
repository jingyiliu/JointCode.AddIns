using System;
using System.Windows.Forms;
using JointCode.AddIns.Core;

namespace JointCode.AddIns.UiLib
{
    public partial class MainForm : Form
    {
        private AddinEngine _addinEngine;

        public MainForm()
        {
            InitializeComponent();

            txtInformation.Text = GetLoadedAssemblies();

			var adnOptions = AddinOptions.Create()
                .WithMessageDialog(new MessageDialog())
                .WithFileSettings(new AddinFileSettings());
             _addinEngine = new AddinEngine(adnOptions);
            _addinEngine.Start(true);

            RefreshInformationArea();
        }

        void RefreshInformationArea()
        {
            txtInformation.Text += "***************************************************************************************************************";
            txtInformation.Text += Environment.NewLine + Environment.NewLine + GetLoadedAssemblies() + Environment.NewLine + GetAddinsAndExtensionPoints();
        }

        string GetLoadedAssemblies()
        {
            var str = "_________________ Current assemblies loaded in AppDomain _________________" + Environment.NewLine;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GlobalAssemblyCache)
                    str += assembly.GetName().Name + Environment.NewLine;
            }
            return str;
        }

        string GetAddinsAndExtensionPoints()
        {
            var addins = _addinEngine.GetAllAddins();

            var info = string.Empty;
            foreach (var addin in addins)
            {
                info += Environment.NewLine + "## Addin: " + addin.Header.AddinId.Guid
                        + "   [" + (addin.Header.Name == null
                            ? string.Empty
                            : addin.Header.Name) + "}"
                        + Environment.NewLine;
                info += "Status: " + addin.Enabled + "\tStarted: " + addin.Started + Environment.NewLine;

                var epDescs = addin.Extension.GetExtensionPointDescriptions();
                if (epDescs != null)
                {
                    info += "=======================================" + Environment.NewLine;
                    foreach (var epDesc in epDescs)
                        info += "@@ ExtensionPoint: " + epDesc.Name + "\tLoaded: " + epDesc.Loaded + "\tTypeName: " + epDesc.TypeName + Environment.NewLine;
                }
            }

            return info;
        }

        #region Event Handlers

        private void menuItemExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        #endregion

        private void btnStart_Click(object sender, EventArgs e)
        {
            var strGuids = txtAddin.Text.Split(',');
            foreach (var strGuid in strGuids)
            {
                var guid = new Guid(strGuid.Trim());
                var addin = _addinEngine.GetAddin(guid);
                addin.Start();
            }
            RefreshInformationArea();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            var strGuids = txtAddin.Text.Split(',');
            foreach (var strGuid in strGuids)
            {
                var guid = new Guid(strGuid.Trim());
                var addin = _addinEngine.GetAddin(guid);
                addin.Stop();
            }
            RefreshInformationArea();
        }

        private void btnEnable_Click(object sender, EventArgs e)
        {
            var strGuids = txtAddin.Text.Split(',');
            foreach (var strGuid in strGuids)
            {
                var guid = new Guid(strGuid.Trim());
                var addin = _addinEngine.GetAddin(guid);
                addin.Enable();
            }
            RefreshInformationArea();
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            var strGuids = txtAddin.Text.Split(',');
            foreach (var strGuid in strGuids)
            {
                var guid = new Guid(strGuid.Trim());
                var addin = _addinEngine.GetAddin(guid);
                addin.Disable();
            }
            RefreshInformationArea();
        }

        private void btnLoadEP_Click(object sender, EventArgs e)
        {
            var epPaths = txtExtensionPoint.Text.Split(',');
            foreach (var epPath in epPaths)
            {
                var path = epPath.Trim();
                object extensionRoot = path.EndsWith("MenuStrip") ? mainMenu : toolBar;
                _addinEngine.LoadExtensionPoint(path, extensionRoot);
            }
            RefreshInformationArea();
        }

        private void btnUnloadEP_Click(object sender, EventArgs e)
        {
            var epPaths = txtExtensionPoint.Text.Split(',');
            foreach (var epPath in epPaths)
                _addinEngine.UnloadExtensionPoint(epPath.Trim());
            RefreshInformationArea();
        }

        private void btnLoadAssemblies_Click(object sender, EventArgs e)
        {
            var guid = new Guid(txtAddin.Text);
            var addin = _addinEngine.GetAddin(guid);
            addin.Runtime.LoadAssemblies();

            RefreshInformationArea();
        }

        private void btnStopEngine_Click(object sender, EventArgs e)
        {
            _addinEngine.Stop();
        }

        private void btnStartEngine_Click(object sender, EventArgs e)
        {
            _addinEngine.Start(true);
        }
    }
}