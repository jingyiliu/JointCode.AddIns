//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns
{
    // One extension root type (e.g, System.Windows.Forms.MenuStrip) can only map to an IExtensionPoint<TExtension, TRoot> 
    // implementation (e.g, class MenuStripExtensionPoint : IExtensionPoint<ToolStripItem, MenuStrip>), and one 
    // IExtensionPoint<TExtension, TRoot> implementation maps to an identification name. 
    // So, if we want to map a same extension root type to 2 different identification name, there is no direct way. However,
    // we can bypass this restriction by design a subclass to inherit from the extension root type (e.g, class MyMenuStrip : MenuStrip),
    // then we provide a new IExtensionPoint<ToolStripItem, MyMenuStrip> implementation (say class MenuStripPoint), and use
    // that implementation to get a new identification name.

    class DefaultNameConvention : INameConvention
    {
        public string GetExtensionPointName(Type extensionRootType)
        {
            return extensionRootType.Name;
        }

        public string GetExtensionBuilderName(Type extensionBuilderType)
        {
            return extensionBuilderType.Name.EndsWith("ExtensionBuilder")
                ? extensionBuilderType.Name.Remove(extensionBuilderType.Name.Length - "ExtensionBuilder".Length)
                : extensionBuilderType.Name;
        }
    }

	/// <summary>
    /// Used to map a type name to an identification name, which is unique within an application.
	/// </summary>
    public interface INameConvention
    {
        /// <summary>
        /// Gets the name of the extension point.
        /// </summary>
        /// <param name="extensionRootType">The type of extension root (the second generic parameter of an 
        /// <see cref="IExtensionPoint{TExtension, TExtensionRoot}"/> implementation).</param>
        /// <returns></returns>
    	string GetExtensionPointName(Type extensionRootType);
        /// <summary>
        /// Gets the name of the extension builder.
        /// </summary>
        /// <param name="extensionBuilderType">The type of an <see cref="IExtensionBuilder{TExtension}"/> implementation.</param>
        /// <returns></returns>
        string GetExtensionBuilderName(Type extensionBuilderType);
    }
}
