using System;
using System.Windows.Forms;
using JointCode.AddIns.UiLib;

namespace JointCode.AddIns.Shell
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(WorkBench.MainForm);
        }
    }
}
