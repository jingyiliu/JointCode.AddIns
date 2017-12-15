using System.Windows.Forms;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripSeparatorExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            return new ToolStripSeparator();
        }
    }
}
