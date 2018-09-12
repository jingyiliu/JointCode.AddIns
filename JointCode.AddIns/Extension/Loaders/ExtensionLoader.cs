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
    abstract class ExtensionLoader : Loader
    {
        readonly ExtensionRecord _exRecord;

        protected ExtensionLoader(ExtensionRecord exRecord)
        {
            _exRecord = exRecord;
        }

        internal ExtensionRecord ExtensionRecord
        {
            get { return _exRecord; }
        }

        internal string Path
        {
            get { return _exRecord.Head.Path; }
        }

        //internal string ParentPath
        //{
        //    get { return _exRecord.Head.ParentPath; }
        //}

        //internal bool SatisfyCondition()
        //{
        //    //Condition condition;
        //    //condition = _context.GetCondition(Path);
        //    //if (condition != null)
        //    //{
        //    //    condition.Loader = this;
        //    //    if (condition.Evaluate())
        //    //        return true;
        //    //    else
        //    //        return false;
        //    //}
        //    return true;
        //}

        internal abstract void SetParent(object parent);

        //public abstract void NotifyExtensionChange(ExtensionChange changeType);
    }

    class ExtensionLoader<TExtension> : ExtensionLoader, IExtensionLoader<TExtension>
    {
        ICompositeExtensionLoader<TExtension> _parent;
        IExtensionBuilder<TExtension> _builder;
        TExtension _instance;

        internal ExtensionLoader(ExtensionRecord exRecord, IExtensionBuilder<TExtension> builder)
            : base(exRecord)
        {
            _builder = builder;
        }

        #region ILoadable Members
        /// <summary>
        /// Loads this instance and all of it _children.
        /// </summary>
        /// <remarks>
        /// This metAutohod is supposed to be called by <see cref="Condition"/>.
        /// </remarks>
        public override void Load(IAddinContext context)
        {
            if (_parent != null)// && !Loaded && SatisfyCondition())
                _parent.LoadChild(context, this);
        }

        /// <summary>
        /// Unloads this instance and all of it _children.
        /// </summary>
        /// <remarks>
        /// This metAutohod is supposed to be called by <see cref="Condition"/>.
        /// </remarks>
        public override void Unload(IAddinContext context)
        {
            if (_parent != null)// && Loaded)
            {
                _parent.UnloadChild(context, this);
                _parent.RemoveChild(this);
                //Dispose();
            }
        }
        #endregion

        #region IExtensionLoader<TExtension> Members

        internal override void SetParent(object parent)
        {
            _parent = (ICompositeExtensionLoader<TExtension>) parent;
        }

        public TExtension GetOrCreateExtension(IAddinContext context)
        {
            if (_instance != null)
                return _instance;
            _instance = _builder.BuildExtension(context);
            return _instance;
        }

        //public override void NotifyExtensionChange(ExtensionChange changeType)
        //{
        //    //ExtensionEventHandler handler;
        //    //handler = _context.GetEventHandler(Path);
        //    //if (handler != null)
        //    //{
        //    //    ExtensionEventArgs args = new ExtensionEventArgs(Path, changeType);
        //    //    handler(Extension, args);
        //    //}
        //}
        #endregion

        //public override void Dispose()
        //{
        //    //if (disposing)
        //    //{
        //    //    _context.UnregisterCondition(Path);
        //    //    _context.UnregisterEventHandler(Path);
        //    //}
        //    //DisposeUnmanagedResources();
        //}
    }

    abstract class CompositeExtensionLoader : ExtensionLoader, ICompositeExtensionLoader
    {
        protected CompositeExtensionLoader(ExtensionRecord exRecord)
            : base(exRecord) { }

        #region ICompositeExtensionLoader Members
        public abstract void AddChild(ExtensionLoader extLoader);
        public abstract void InsertChild(int index, ExtensionLoader extLoader);
        public abstract void RemoveChild(ExtensionLoader extLoader);
        #endregion
    }

    class CompositeExtensionLoader<TExtension> 
        : CompositeExtensionLoader, IExtensionLoader<TExtension>, ICompositeExtensionLoader<TExtension>
    {
        ICompositeExtensionLoader<TExtension> _parent;
        readonly ICompositeExtensionBuilder<TExtension> _builder;
        readonly ExtensionLoaderCollection _children;
        TExtension _instance;

        internal CompositeExtensionLoader(ExtensionRecord exRecord, ICompositeExtensionBuilder<TExtension> builder)
            : base(exRecord)
        {
            _builder = builder;
            _children = new ExtensionLoaderCollection();
        }

        #region ILoadable Members

        /// <summary>
        /// Loads this instance and all of it _children.
        /// </summary>
        /// <remarks>
        /// This metAutohod is supposed to be called by <see cref="Condition"/>.
        /// </remarks>
        public override void Load(IAddinContext context)
        {
            if (_parent != null)// && !Loaded && SatisfyCondition())
            {
                _parent.LoadChild(context, this);
                //LoadChildren(context);
            }
        }
        ///// <summary>
        ///// Loads the children.
        ///// </summary>
        //void LoadChildren(IAddinContext context)
        //{
        //    if (_children != null)
        //    {
        //        foreach (var child in _children)
        //        {
        //            //child.SetParent(this);
        //            child.Load(context);
        //        }
        //    }
        //}

        /// <summary>
        /// Unloads this instance and all of it children.
        /// </summary>
        /// <remarks>
        /// This metAutohod is supposed to be called by <see cref="Condition"/>.
        /// </remarks>
        public override void Unload(IAddinContext context)
        {
            if (_parent != null)// && Loaded)
            {
                //UnloadChildren(context);
                _parent.UnloadChild(context, this);
                _parent.RemoveChild(this);
                //Dispose();
            }
        }
        ///// <summary>
        ///// Unloads the children.
        ///// </summary>
        //void UnloadChildren(IAddinContext context)
        //{
        //    if (_children != null)
        //    {
        //        foreach (var child in _children)
        //        {
        //            child.Unload(context);
        //        }
        //    }
        //}

        #endregion

        #region ICompositeExtensionLoader Members

        public override void AddChild(ExtensionLoader extLoader)
        {
            extLoader.SetParent(this);
            _children.Add(extLoader);
        }

        public override void InsertChild(int index, ExtensionLoader extLoader)
        {
            extLoader.SetParent(this);
            _children.Insert(index, extLoader);
        }

        public override void RemoveChild(ExtensionLoader extLoader)
        {
            _children.Remove(extLoader);
        }

        #endregion

        #region ICompositeExtensionLoader<TExtension> Members

        public void LoadChild(IAddinContext context, IExtensionLoader<TExtension> loader)
        {
            //if (loader.Loaded)
            //    return;
            var ext = loader.GetOrCreateExtension(context);
            _builder.AddChildExtension(ext);
            //loader.Loaded = true;
            ////loader.NotifyExtensionChange(ExtensionChange.Load);
        }

        public void LoadChild(IAddinContext context, int index, IExtensionLoader<TExtension> loader)
        {
            //if (loader.Loaded)
            //    return;
            var ext = loader.GetOrCreateExtension(context);
            _builder.InsertChildExtension(index, ext);
            //loader.Loaded = true;
            ////loader.NotifyExtensionChange(ExtensionChange.Load);
        }

        public void UnloadChild(IAddinContext context, IExtensionLoader<TExtension> loader)
        {
            //if (!loader.Loaded)
            //    return;
            var ext = loader.GetOrCreateExtension(context);
            //loader.NotifyExtensionChange(ExtensionChange.Unload);
            _builder.RemoveChildExtension(ext);
            //loader.Loaded = false;
        }

        #endregion

        #region IExtensionLoader<TExtension> Members

        internal override void SetParent(object parent)
        {
            _parent = (ICompositeExtensionLoader<TExtension>)parent;
        }

        public TExtension GetOrCreateExtension(IAddinContext context)
        {
            if (_instance != null)
                return _instance;
            _instance = _builder.BuildExtension(context);
            return _instance;
        }

        //public override void NotifyExtensionChange(ExtensionChange changeType)
        //{
        //    //ExtensionEventHandler handler;
        //    //handler = _context.GetEventHandler(Path);
        //    //if (handler != null)
        //    //{
        //    //    ExtensionEventArgs args = new ExtensionEventArgs(Path, changeType);
        //    //    handler(Extension, args);
        //    //}
        //}

        #endregion

        //public override void Dispose()
        //{
        //    //if (disposing)
        //    //{
        //    //    _context.UnregisterCondition(Path);
        //    //    _context.UnregisterEventHandler(Path);
        //    //}
        //    //DisposeUnmanagedResources();
        //}
    }
}
