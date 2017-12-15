using System.Windows.Forms;

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
