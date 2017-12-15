using System;
using System.Windows.Forms;
using JointCode.AddIns.UiLib;

namespace JointCode.AddIns.Shell
{
    class Program
    {
        static void PrintAssemblies(int i)
        {
            Console.WriteLine(i.ToString() + "_______________________________________________");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GlobalAssemblyCache)
                    Console.WriteLine(assembly.GetName().Name);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Application starting up...");

            PrintAssemblies(1);

            AddinManager.Initialize(true);

            try
            {
                Console.WriteLine("All addin assemblies should not loaded at this time...");
                PrintAssemblies(2);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(WorkBench.MainForm);
            }
            catch (System.Exception ex)
            {
                //var exceptionReporter = new ExceptionReporter();
                //exceptionReporter.ReadConfig();
                //exceptionReporter.Show(ex);
            }
        }
    }
}
