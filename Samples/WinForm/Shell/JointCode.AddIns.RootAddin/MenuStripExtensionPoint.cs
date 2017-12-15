using System.Windows.Forms;

namespace JointCode.AddIns.RootAddin
{
    public interface IHasRoot
    {
        MenuStrip Root { set; }
    }

    public class BaseMenuStripExtensionPoint : IHasRoot
    {
        protected MenuStrip _root;
        public MenuStrip Root
        {
            set { _root = value; }
        }
    }

    public class MenuStripExtensionPoint : BaseMenuStripExtensionPoint, IExtensionPoint<ToolStripItem, MenuStrip>
    {
        #region IExtensionPoint<ToolStripItem,MenuStrip> Members

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
