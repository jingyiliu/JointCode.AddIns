using System.Windows.Forms;
using JointCode.AddIns.ParentAddin;

namespace JointCode.AddIns.ChildAddin
{
    class ChildAddinCommand : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("ChildAddin loaded!");
        }

        #endregion
    }
}
