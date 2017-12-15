using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Eggec.Addins;
using System.IO;

namespace Eggec.TextEditorLib
{
    public interface ICommand
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        void Run();
    }

    public class MenuItemTemplate : ISimpleExtensionTemplate<ToolStripItem>
    {
        string _name;
        string _tooltip;
        string _commandType;
        AddinContext _adnContext;
        ToolStripMenuItem _menu;

        [ExtensionData(Required = true)]
        public string Name
        {
            set { _name = value; }
        }

        [ExtensionData(Required = false)]
        public string Tooltip
        {
            set { _tooltip = value; }
        }

        [ExtensionData(Required = true)]
        public string CommandType
        {
            set { _commandType = value; }
        }

        public AddinContext AddinContext
        {
            get { return _adnContext; }
        }

        #region IComplexExtensionTemplate<ToolStripItem> Members

        public ToolStripItem BuildExtension(AddinContext adnContext)
        {
            _adnContext = adnContext;
            ToolStripMenuItem menu = new ToolStripMenuItem(_name);
            if (_tooltip != null)
                menu.ToolTipText = _tooltip;

            menu.Click += OnClick;
            _menu = menu;
            return menu;
        }

        void OnClick(object sender, EventArgs e)
        {
            PrintAssemblies(4);

            ICommand command = AddinContext.CreateInstance(_commandType) as ICommand;
            if (command != null)
                command.Run();

            PrintAssemblies(5);
        }

        void PrintAssemblies(int i)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(i.ToString() + "_______________________________________________");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GlobalAssemblyCache)
                    Console.WriteLine(assembly.GetName().Name);
            }
        }

        #endregion
    }
}
