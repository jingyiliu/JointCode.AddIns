using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Eggec.Addins;

namespace Eggec.TextEditorLib
{
    public class MenuTemplate : IComplexExtensionTemplate<ToolStripItem>
    {
        string _name;
        ToolStripMenuItem _menu;

        [ExtensionData]
        public string Name
        {
            set { _name = value; }
        }

        #region IComplexExtensionTemplate<ToolStripItem> Members

        public ToolStripItem BuildExtension(AddinContext adnContext)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(_name);
            _menu = menu;
            return menu;
        }

        public void AddChildExtension(ToolStripItem child)
        {
            _menu.DropDownItems.Add(child);
        }

        public void InsertChildExtension(int index, ToolStripItem child)
        {
            _menu.DropDownItems.Insert(index, child);
        }

        public void RemoveChildExtension(ToolStripItem child)
        {
            _menu.DropDownItems.Remove(child);
        }

        #endregion
    }
}
