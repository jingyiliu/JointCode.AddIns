using System;
using System.Diagnostics;
using System.Windows.Forms;
using JointCode.AddIns.Core;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.RootAddin
{
    public class ToolStripMenuItemExtensionBuilder : ICompositeExtensionBuilder<ToolStripItem>
    {
        ToolStripMenuItem _menu;
        IAddinContext _adnContext;

        [ExtensionProperty(Required = true)]
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public AddinTypeHandle CommandType { get; set; }

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

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            if (_menu != null)
                return _menu;

            _adnContext = adnContext;

            var menu = new ToolStripMenuItem();
            menu.Text = Name;
            menu.ToolTipText = Tooltip;

            if (CommandType != null)
                menu.Click += OnMenuClick; // lazy loading type into the runtime
            
            _menu = menu;
            return menu;
        }

        void OnMenuClick(object sender, EventArgs e)
        {
            var type = _adnContext.Addin.Runtime.GetType(CommandType);
            var command = (IRootCommand)Activator.CreateInstance(type);
            command.Run();
        }
    }
}