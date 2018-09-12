using System;
using System.Windows.Forms;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripComboBoxExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        public string Name { get; set; }

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            return new ToolStripComboBox{Text = Name};
        }
    }
}
