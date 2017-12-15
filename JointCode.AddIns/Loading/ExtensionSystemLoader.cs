//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections.Generic;
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Loading.Loaders;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Loading
{
    class ExtensionSystemLoader
    {
        readonly LoaderFactory _loaderFactory;
        // (Path=>ExtensionLoader) || (Id=>ExtensionPointLoader)
        readonly Dictionary<string, Loader> _path2Loaders;

        internal ExtensionSystemLoader(RuntimeAssemblyResolver asmResolver)
        {
            _path2Loaders = new Dictionary<string, Loader>();
            _loaderFactory = new LoaderFactory(asmResolver);
        }

        #region ExtensionPoint/ExtensionBuilder

        internal void RegisterExtensionPoint(ExtensionPointRecord epRecord, Type extensionRootType)
        {
        	_loaderFactory.RegisterExtensionPoint(epRecord, extensionRootType);
            if (epRecord.Children != null)
            {
                foreach (var child in epRecord.Children)
                    RegisterExtensionBuilderRecursively(child);
            }
        }
        
        internal void UnregisterExtensionPoint(ExtensionPointRecord epRecord)
        {
        	_loaderFactory.UnregisterExtensionPoint(epRecord);
        }

        internal void RegisterExtensionBuilderGroup(ExtensionBuilderRecordGroup ebRecordGroup)
        {
            foreach (var child in ebRecordGroup.Children)
                RegisterExtensionBuilderRecursively(child);
        }

        internal void UnregisterExtensionBuilderGroup(ExtensionBuilderRecordGroup ebRecordGroup)
        {
            //_loaderFactory.UnregisterExtensionBuilder(ebRecord);
        }

        void RegisterExtensionBuilderRecursively(ExtensionBuilderRecord ebRecord)
        {
            if (ebRecord.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
                _loaderFactory.RegisterExtensionBuilder(ebRecord);

            if (ebRecord.Children != null)
            {
                foreach (var child in ebRecord.Children)
                    RegisterExtensionBuilderRecursively(child);
            }
        }
        
        #endregion

        #region Extension

        /// <summary>
        /// If the extension point has not loaded yet, call this method to register new extensions to be loaded with it. 
        /// </summary>
        internal void RegisterExtensionGroup(ExtensionPointRecord epRecord, ExtensionRecordGroup exRecordGroup)
        {
            ICompositeExtensionLoader parentLoader;
            if (exRecordGroup.RootIsExtensionPoint)
            {
                if (!TryGetLoader(exRecordGroup.ParentPath, false, out parentLoader))
                {
                    var epLoader = _loaderFactory.CreateExtensionPointLoader(epRecord);
                    _path2Loaders.Add(epRecord.Id, epLoader);
                    parentLoader = epLoader;
                }
            }
            else
            {
                TryGetLoader(exRecordGroup.ParentPath, true, out parentLoader);
            }

            foreach (var child in exRecordGroup.Children)
                RegisterExtensionsRecursively(parentLoader, child);
        }

        void RegisterExtensionsRecursively(ICompositeExtensionLoader parentLoader, ExtensionRecord exRecord)
        {
            var exLoader = _loaderFactory.CreateExtensionLoader(exRecord);

            _path2Loaders.Add(exLoader.Path, exLoader);
            parentLoader.AddChild(exLoader);

            if (exRecord.Children != null)
            {
                foreach (var child in exRecord.Children)
                    RegisterExtensionsRecursively(exLoader as ICompositeExtensionLoader, child);
            }
        }

        internal void UnregisterExtensions(ExtensionRecordGroup exRecordGroup)
        {
            //ICompositeExtensionLoader parentLoader;
            //if (!TryGetLoader<ICompositeExtensionLoader>(exRecordGroup.ParentPath, false, out parentLoader))
            //    return;
            //var children = new List<ExtensionRecord>(exRecordGroup.Children);

            //for (int i = 0; i < children.Count; i++)
            //{
            //    var child = children[i];
            //    var childLoader = _loaderFactory.CreateExtensionLoader(child);
            //    _path2Loaders.Add(childLoader.Path, childLoader);
            //    parentLoader.AddChild(childLoader);

            //    if (child.Children != null)
            //        children.AddRange(child.Children);
            //}

            ////for (int i = children.Count - 1; i >= 0; i--)
            ////{
            ////    Loader loader;
            ////    if (TryGetLoader(children[i].Head.Path, true, out loader))
            ////        loader.Unload(c);
            ////}
        }

        ///// <summary>
        ///// If the extension point has loaded already, call this method to add and load new extensions. 
        ///// </summary>
        ///// <param name="exRecordGroup"></param>
        //internal void RegisterAndLoadExtensions(ExtensionRecordGroup exRecordGroup)
        //{
        //}

        //internal void UnloadAndUnregisterExtensions(ExtensionRecordGroup exRecordGroup)
        //{
        //} 

        #endregion

        #region Load

        internal bool TryLoadExtensionPoint(IAddinContext adnContext, string extensionPointId, object root)
        {
            ExtensionPointLoader epLoader;
            if (!TryGetLoader(extensionPointId, false, out epLoader))
                return false;
            if (!epLoader.Loaded)
            {
                if (!epLoader.TrySetRoot(root))
                    return false;
                epLoader.Load(adnContext);
            }
            return true;
        }

        internal void LoadExtensionPoint(IAddinContext adnContext, string extensionPointId, object root)
        {
            ExtensionPointLoader epLoader;
            TryGetLoader<ExtensionPointLoader>(extensionPointId, true, out epLoader);
            if (!epLoader.Loaded)
            {
            	epLoader.TrySetRoot(root);
                epLoader.Load(adnContext);
            }
        }

        internal void UnloadExtensionPoint(IAddinContext adnContext, string extensionPointId)
		{
            ExtensionPointLoader epLoader;
            if (!TryGetLoader<ExtensionPointLoader>(extensionPointId, false, out epLoader))
                return;
            if (epLoader.Loaded)
                epLoader.Unload(adnContext);
		}

        #endregion
        
        bool TryGetLoader<T>(string path, bool fastfail, out T result) where T : class, ILoader
        {
        	Loader loader;
            if (!_path2Loaders.TryGetValue(path, out loader))
            {
                if (fastfail)
                    throw new InvalidOperationException();
                result = null;
                return false;
            }
        	
        	result = loader as T;
            if (result == null)
            {
                if (fastfail)
                    throw new InvalidOperationException();
                return false;
            }

        	return true;
        }

        //bool TryGetLoader(string path, bool fastfail, out Loader result)
        //{
        //    if (_path2Loaders.TryGetValue(path, out result))
        //        return true;
        //    if (fastfail)
        //        throw new InvalidOperationException();
        //    return false;
        //}
    }
}
