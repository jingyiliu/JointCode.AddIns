using System.Windows.Forms;

namespace JointCode.AddIns.RootAddin
{
    class RootMenu1Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's RootMenu1Command");
        }

        #endregion
    }

    class RootMenu2Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            MessageBox.Show("That's RootMenu2Command");
        }

        #endregion
    }
}
