using System.Windows.Forms;
using JointCode.AddIns.RootAddin;

namespace JointCode.AddIns.ParentAddin
{
    class ParentMenu1Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's ParentMenu1Command");
        }

        #endregion
    }

    class ParentMenu2Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's ParentMenu2Command");
        }

        #endregion
    }

    class ParentMenu3Command : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's ParentMenu3Command");
        }

        #endregion
    }

    class ParentMenu4Command : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's ParentMenu4Command");
        }

        #endregion
    }
}
