//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.AddIns.Extension;

namespace JointCode.AddIns
{
    /// <summary>
    /// Used to map a type name to an identification name, which is unique within an application.
	/// </summary>
    public interface INameConvention
    {
        /// <summary>
        /// Gets the name of the extension point.
        /// The extension point name provided in the addin manifest can comply with the name convention, or may not comply with the name convention.
        /// </summary>
        /// <param name="extensionRootType">The extension root type of extension point (an  <see cref="IExtensionPoint{TExtension, TExtensionRoot}"/> implementation).</param>
        /// <returns></returns>
    	string GetExtensionPointName(Type extensionRootType);
        /// <summary>
        /// Gets the name of the extension builder.
        /// The extension builder name provided in the addin manifest must comply with the name convention.
        /// </summary>
        /// <param name="extensionBuilderTypeName">The full type name of extension builder (an <see cref="IExtensionBuilder{TExtension}"/> implementation).</param>
        /// <returns></returns>
        string GetExtensionBuilderName(string extensionBuilderTypeName);
    }
}
