using System;
using System.Windows.Forms;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripDropDownButtonExtensionBuilder : ICompositeExtensionBuilder<ToolStripItem>
    {
        ToolStripDropDownButton _menu;

        public string Name { get; set; }

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            if (_menu != null)
                return _menu;
            _menu = new ToolStripDropDownButton{ Text = Name };
            return _menu;
        }

        public void AddChildExtension(ToolStripItem child)
        {
            _menu.DropDownItems.Add(child);
        }

        public void InsertChildExtension(int idx, ToolStripItem child)
        {
            _menu.DropDownItems.Insert(idx, child);
        }

        public void RemoveChildExtension(ToolStripItem child)
        {
            _menu.DropDownItems.Remove(child);
        }
    }
}
