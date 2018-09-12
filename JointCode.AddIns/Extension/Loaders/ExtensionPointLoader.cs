//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Extension.Loaders
{
    abstract class ExtensionPointLoader : Loader, ICompositeExtensionLoader
    {
        readonly ExtensionLoaderCollection _children;
        readonly ExtensionPointRecord _epRecord;

        protected ExtensionPointLoader(ExtensionPointRecord epRecord)
        {
            _epRecord = epRecord;
            _children = new ExtensionLoaderCollection();
        }

        internal ExtensionLoaderCollection Children { get { return _children; } }

        internal ExtensionPointRecord ExtensionPointRecord { get { return _epRecord; } }

        internal abstract bool TrySetRoot(object root);

        #region ICompositeExtensionLoader Members

        public void AddChild(ExtensionLoader extLoader)
        {
            extLoader.SetParent(this);
            _children.Add(extLoader);
        }

        public void InsertChild(int index, ExtensionLoader extLoader)
        {
            extLoader.SetParent(this);
            _children.Insert(index, extLoader);
        }

        public void RemoveChild(ExtensionLoader extLoader)
        {
            _children.Remove(extLoader);
        }

        #endregion

        /// <summary>
        /// Loads this instance and all of it _children.
        /// </summary>
        public override void Load(IAddinContext context)
        {
            //if (Loaded || Children == null)
            //    return;
            ////foreach (var child in Children)
            ////{
            ////    //child.SetParent(this);
            ////    child.Load(context);
            ////}
            //Loaded = true;
        }
        /// <summary>
        /// Unloads this instance and all of it _children.
        /// </summary>
        public override void Unload(IAddinContext context)
        {
            //if (!Loaded || Children == null)
            //    return;
            ////foreach (var child in Children)
            ////    child.Unload(context);
            //Loaded = false;
        }
    }

    class ExtensionPointLoader<TExtension, TRoot> : ExtensionPointLoader, ICompositeExtensionLoader<TExtension> where TRoot : class
    {
        readonly IExtensionPoint<TExtension, TRoot> _extensionPoint;

        internal ExtensionPointLoader(ExtensionPointRecord epRecord, IExtensionPoint<TExtension, TRoot> extensionPoint)
            : base(epRecord)
        {
            _extensionPoint = extensionPoint;
        }

        internal override bool TrySetRoot(object root)
        {
            //var tRoot = (TRoot)root;
            var tRoot = root as TRoot;
            if (tRoot == null)
            	return false;
            _extensionPoint.Root = tRoot;
            return true;
        }

        #region ILoadable Members

        ///// <summary>
        ///// Loads this instance and all of it _children.
        ///// </summary>
        //public override void Load(IAddinContext context)
        //{
        //    if (Loaded || Children == null)
        //        return;
        //    //foreach (var child in Children)
        //    //{
        //    //    //child.SetParent(this);
        //    //    child.Load(context);
        //    //}
        //    Loaded = true;
        //}
        ///// <summary>
        ///// Unloads this instance and all of it _children.
        ///// </summary>
        //public override void Unload(IAddinContext context)
        //{
        //    if (!Loaded || Children == null)
        //        return;
        //    //foreach (var child in Children)
        //    //    child.Unload(context);
        //    Loaded = false;
        //}

        #endregion

        #region ICompositeExtensionLoader<TExtension> Members

        public void LoadChild(IAddinContext context, IExtensionLoader<TExtension> extLoader)
        {
            //if (extLoader.Loaded)
            //    return;
            var ext = extLoader.GetOrCreateExtension(context);
            _extensionPoint.AddChildExtension(ext);
            //extLoader.Loaded = true;
            ////extLoader.NotifyExtensionChange(ExtensionChange.Load);
        }

        public void LoadChild(IAddinContext context, int index, IExtensionLoader<TExtension> extLoader)
        {
            //if (extLoader.Loaded)
            //    return;
            var ext = extLoader.GetOrCreateExtension(context);
            _extensionPoint.InsertChildExtension(index, ext);
            //extLoader.Loaded = true;
            ////extLoader.NotifyExtensionChange(ExtensionChange.Load);
        }

        public void UnloadChild(IAddinContext context, IExtensionLoader<TExtension> extLoader)
        {
            //if (!extLoader.Loaded)
            //    return;
            var ext = extLoader.GetOrCreateExtension(context);
            //extLoader.NotifyExtensionChange(ExtensionChange.Unload);
            _extensionPoint.RemoveChildExtension(ext);
            //extLoader.Loaded = false;
        }

        #endregion

//        public override void Dispose()
//        {
////            if (disposing)
////            {
////            }
//            //DisposeUnmanagedResources();
//        }
    }
}
