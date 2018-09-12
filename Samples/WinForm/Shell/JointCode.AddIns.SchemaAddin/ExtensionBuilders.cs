using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.SchemaAddin
{

    public class ToolStripSeparatorExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            return new ToolStripSeparator();
        }
    }

    //class ExtensionSchemaOnlyCommand : IRootCommand
    //{
    //    #region ICommand Members

    //    public void Run()
    //    {
    //        MessageBox.Show("That's RootMenu2Command");
    //    }

    //    #endregion
    //}
}
