using System.Windows.Forms;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripExtensionPoint : IExtensionPoint<ToolStripItem, ToolStrip>
    {
        #region IExtensionPoint<ToolStripItem, ToolStrip> Members

        ToolStrip _root;

        public ToolStrip Root
        {
            set { _root = value; }
        }

        public void AddChildExtension(ToolStripItem child)
        {
            _root.Items.Add(child);
        }

        public void InsertChildExtension(int index, ToolStripItem child)
        {
            _root.Items.Insert(index, child);
        }

        public void RemoveChildExtension(ToolStripItem child)
        {
            _root.Items.Remove(child);
        }

        #endregion
    }
}
