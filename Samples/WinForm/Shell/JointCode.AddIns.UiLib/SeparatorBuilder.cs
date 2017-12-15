using System;
using Eggec.Addins;
using System.Windows.Forms;

namespace Eggec.TextEditorLib
{
    public class SeparatorTemplate : ISimpleExtensionTemplate<ToolStripItem>
    {
        #region ISimpleExtensionTemplate<ToolStripItem> Members

        public ToolStripItem BuildExtension(AddinContext adnContext)
        {
            ToolStripSeparator separator = new ToolStripSeparator();
            return separator;
        }

        #endregion
    }
}
