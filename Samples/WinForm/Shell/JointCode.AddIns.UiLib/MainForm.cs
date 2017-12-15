using System;
using System.Windows.Forms;

namespace JointCode.AddIns.UiLib
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            PrintAssemblies(10001);

            //************************************************************
            // load extension point
            AddinManager.TryLoadExtensionPoint("MenuStrip", mainMenu);
            AddinManager.TryLoadExtensionPoint("ToolStrip", toolBar);
            //************************************************************

            PrintAssemblies(10002);
        }

        void PrintAssemblies(int i)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(i.ToString() + "_______________________________________________");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GlobalAssemblyCache)
                    Console.WriteLine(assembly.GetName().Name);
            }
        }

        #region Event Handlers

        private void menuItemExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        #endregion
    }
}