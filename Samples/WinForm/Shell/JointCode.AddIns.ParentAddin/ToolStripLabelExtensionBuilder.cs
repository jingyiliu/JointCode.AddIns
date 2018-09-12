using System;
using System.Windows.Forms;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripLabelExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        public string Name { get; set; }

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            return new ToolStripLabel{Text = Name};
        }
    }
}
