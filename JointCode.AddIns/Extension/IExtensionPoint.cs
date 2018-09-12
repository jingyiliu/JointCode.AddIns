//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.AddIns.Extension
{
    /// <summary>
    /// Represent an extension point.
    /// </summary>
    public interface IExtensionPoint { }

    /// <summary>
    /// Represent an extension point.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface IExtensionPoint<TExtension> : IExtensionPoint
    {
        /// <summary>
        /// Loads a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void AddChildExtension(TExtension child);
        /// <summary>
        /// Inserts a child extension
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="child">The child.</param>
        void InsertChildExtension(int index, TExtension child);
        /// <summary>
        /// Unloads a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void RemoveChildExtension(TExtension child);
    }

    /// <summary>
    /// Represent an extension point.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    /// <typeparam name="TExtensionRoot">The type of the extension root.</typeparam>
    public interface IExtensionPoint<TExtension, TExtensionRoot> : IExtensionPoint<TExtension>
    {
        TExtensionRoot Root { set; }
    }
}
