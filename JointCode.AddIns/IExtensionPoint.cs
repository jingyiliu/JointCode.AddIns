//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.AddIns
{
    public interface IExtensionPoint { }

    /// <summary>
    /// IExtensionPoint{TExtension}
    /// </summary>
    /// <typeparam name="TExtension"></typeparam>
    /// <remarks>
    /// Normally, one extension type (the TExtension of the IExtensionPoint{TExtension, TRoot} interface) maps to one ExtensionPointLinker{TExtension, TRoot} or 
    /// HierarchicalContainerNode{TExtension, TRoot} instance.
    /// However, if two containerObject builders share a same extension type, then at lease one of them must provide an Id (identity) in the 
    /// IExtensionPointPositioner to distinguish itself from the required.
    /// </remarks>
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
    /// IExtensionPoint{TExtension, TRoot}
    /// </summary>
    /// <typeparam name="TExtension"></typeparam>
    /// <typeparam name="TRoot">The type of the containerObject.</typeparam>
    /// <remarks>
    /// Normally, one extension type (the TExtension of the IExtensionPoint{TExtension, TRoot} interface) maps to one ExtensionPointLinker{TExtension, TRoot} or 
    /// HierarchicalContainerNode{TExtension, TRoot} instance.
    /// However, if two containerObject builders share a same extension type, then at lease one of them must provide an Id (identity) in the 
    /// IExtensionPointPositioner to distinguish itself from the required.
    /// </remarks>
    public interface IExtensionPoint<TExtension, TRoot> : IExtensionPoint<TExtension>
    {
    	TRoot Root { set; }
    }
}
