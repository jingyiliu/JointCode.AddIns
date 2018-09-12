using System.Windows.Forms;
using JointCode.AddIns.AppLib;
using JointCode.AddIns.RootAddin;
using JointCode.AddIns.Shell.AddinsSharedLib;
using JointCode.AddIns.UiLib;

namespace JointCode.AddIns.ParentAddin
{
    class ParentMenu1Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            var cc = new CommonClass();
            MessageBox.Show(cc.GetLocation(), "ParentMenu1Command");
        }

        #endregion
    }

    class ParentMenu2Command : IRootCommand
    {
        #region ICommand Members

        public void Run()
        {
            var cc = new CommonClass();
            MessageBox.Show(cc.GetLocation(), "ParentMenu2Command");
        }

        #endregion
    }

    class ParentMenu3Command : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            var np = new NameProvider();
            MessageBox.Show(np.GetName("ParentMenu3Command"));
        }

        #endregion
    }

    class ParentMenu4Command : IParentCommand
    {
        #region ICommand Members

        public void Run()
        {
            var np = new CommonClass();
            var messageForm = new MessageForm(np.GetLoadedAssemblies(), "ParentMenu4Command");
            messageForm.Show();
        }

        #endregion
    }
}
