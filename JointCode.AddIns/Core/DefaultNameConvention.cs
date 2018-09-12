using System;

namespace JointCode.AddIns.Core
{
    // One extension root type (e.g, System.Windows.Forms.MenuStrip) can only map to an IExtensionPoint<TExtension, TRoot> 
    // implementation (e.g, class MenuStripExtensionPoint : IExtensionPoint<ToolStripItem, MenuStrip>), and one 
    // IExtensionPoint<TExtension, TRoot> implementation maps to an identification name. 
    // So, if we want to map a same extension root type to 2 different identification name, there is no direct way. However,
    // we can bypass this restriction by design a subclass to inherit from the extension root type (e.g, class MyMenuStrip : MenuStrip),
    // then we provide a new IExtensionPoint<ToolStripItem, MyMenuStrip> implementation (say class MenuStripPoint), and use
    // that implementation to get a new identification name.
    //[Serializable]
    class DefaultNameConvention : INameConvention
    {
        const string ExtensionBuilder = "ExtensionBuilder";

        // @extensionPointTypeName: JointCode.AddIns.RootAddin.MenuStripExtensionPoint
        // @result: MenuStrip
        public string GetExtensionPointName(Type extensionRootType)
        {
            return extensionRootType.Name;
        }

        // @extensionBuilderTypeName: JointCode.AddIns.RootAddin.ToolStripMenuItemExtensionBuilder
        // @result: ToolStripMenuItem
        public string GetExtensionBuilderName(string extensionBuilderTypeName)
        {
            string result;
            var lastIndex = extensionBuilderTypeName.LastIndexOf('.');

            if (lastIndex < 0)
            {
                result = extensionBuilderTypeName.EndsWith(ExtensionBuilder, StringComparison.InvariantCultureIgnoreCase)
                    ? extensionBuilderTypeName.Substring(0, extensionBuilderTypeName.Length - ExtensionBuilder.Length)
                    : extensionBuilderTypeName;
            }
            else
            {
                result = extensionBuilderTypeName.EndsWith(ExtensionBuilder, StringComparison.InvariantCultureIgnoreCase)
                    ? extensionBuilderTypeName.Substring(lastIndex + 1, extensionBuilderTypeName.Length - lastIndex - 1 - ExtensionBuilder.Length)
                    : extensionBuilderTypeName.Substring(lastIndex + 1, extensionBuilderTypeName.Length - lastIndex - 1);
            }

            return result;
        }
    }
}