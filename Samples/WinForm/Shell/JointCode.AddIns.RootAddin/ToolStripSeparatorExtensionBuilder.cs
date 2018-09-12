using System.Windows.Forms;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.RootAddin
{
    public class ToolStripSeparatorExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            return new ToolStripSeparator();
        }
    }
}
