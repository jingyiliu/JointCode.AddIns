using System;
using System.Windows.Forms;
using JointCode.AddIns.Core;

namespace JointCode.AddIns.ParentAddin
{
    public class ToolStripButtonExtensionBuilder : IExtensionBuilder<ToolStripItem>
    {
        IAddinContext _adnContext;
        public TypeId CommandType { get; set; }
        public string Name { get; set; }

        public ToolStripItem BuildExtension(IAddinContext adnContext)
        {
            _adnContext = adnContext;

            var item = new ToolStripButton();
            item.Text = Name;

            if (CommandType != null)
                item.Click += OnMenuClick; // lazy loading type into the runtime
            return item;
        }

        void OnMenuClick(object sender, EventArgs e)
        {
            var type = _adnContext.RuntimeSystem.GetType(CommandType);
            var command = (IParentCommand)Activator.CreateInstance(type);
            command.Run();
        }
    }
}
