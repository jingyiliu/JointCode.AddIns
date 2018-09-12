using System.Windows.Forms;
using JointCode.AddIns.AppLib;
using JointCode.AddIns.ParentAddin;
using JointCode.AddIns.Shell.AddinsSharedLib;

namespace JointCode.AddIns.ChildAddin
{
    class ChildAddinCommand : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            var cc = new CommonClass();
            MessageBox.Show(cc.GetLocation(), "ChildAddin Loaded");
            var np = new NameProvider();
            MessageBox.Show(np.GetName("ChildAddin"));
        }

        #endregion
    }
}
