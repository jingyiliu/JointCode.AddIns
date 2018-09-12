using System;
using System.Windows.Forms;
using JointCode.AddIns.Core;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripMenuItemExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        IAddinContext _adnContext;

        public string Name { get; set; }
        public string Tooltip { get; set; }
        public AddinTypeHandle CommandType { get; set; }

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            _adnContext = adnContext;

            var menu = new ToolStripMenuItem();
            menu.Text = Name;
            menu.ToolTipText = Tooltip;

            if (CommandType != null)
                menu.Click += OnMenuClick; // lazy loading type into the runtime

            return menu;
        }

        void OnMenuClick(object sender, EventArgs e)
        {
            var type = _adnContext.Addin.Runtime.GetType(CommandType);
            var command = (IParentCommand)Activator.CreateInstance(type);
            command.Run();
        }
    }
}