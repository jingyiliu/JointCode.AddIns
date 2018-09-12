//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Extension.Loaders;
using JointCode.AddIns.Metadata.Assets;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Extension
{
    class RuntimeExtensionLoader
    {
        readonly LoaderFactory _loaderFactory;
        // (Path=>ExtensionLoader) || (Id=>ExtensionPointLoader)
        Dictionary<string, Loader> _path2Loaders;
        List<ExtensionPointRecord> _loadedExtensionPointRecords;

        internal RuntimeExtensionLoader(RuntimeAssemblyResolver asmResolver, IExtensionPointFactory extensionPointFactory, IExtensionBuilderFactory extensionBuilderFactory)
        {
            _path2Loaders = new Dictionary<string, Loader>();
            _loadedExtensionPointRecords = new List<ExtensionPointRecord>();
            _loaderFactory = new LoaderFactory(asmResolver, extensionPointFactory, extensionBuilderFactory);
        }

        internal void Reset()
        {
            _loaderFactory.Reset();
            _path2Loaders = new Dictionary<string, Loader>();
            _loadedExtensionPointRecords = new List<ExtensionPointRecord>();
        }

        #region ExtensionPoint/ExtensionBuilder

        //internal List<ExtensionPointRecord> LoadedExtensionPoints { get { return _loadedExtensionPointRecords; } }
        internal int LoadedExtensionPointCount { get { return _loadedExtensionPointRecords.Count; } }

        internal ExtensionPointRecord GetLoadedExtensionPoint(int index)
        {
            return _loadedExtensionPointRecords[index];
        }

        //internal bool ExtensionPointLoaded(ExtensionPointRecord epRecord)
        //{
        //    return _loaderFactory.ExtensionPointRegistered(epRecord);
        //}

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
            if (epRecord.Children != null)
            {
                foreach (var child in epRecord.Children)
                    UnregisterExtensionBuilderRecursively(child);
            }
            _loaderFactory.UnregisterExtensionPoint(epRecord);
        }

        internal void RegisterExtensionBuilders(ExtensionBuilderRecordGroup ebRecordGroup)
        {
            foreach (var child in ebRecordGroup.Children)
                RegisterExtensionBuilderRecursively(child);
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

        internal void UnregisterExtensionBuilders(ExtensionBuilderRecordGroup ebRecordGroup)
        {
            foreach (var child in ebRecordGroup.Children)
                UnregisterExtensionBuilderRecursively(child);
        }

        void UnregisterExtensionBuilderRecursively(ExtensionBuilderRecord ebRecord)
        {
            if (ebRecord.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
                _loaderFactory.UnregisterExtensionBuilder(ebRecord);

            if (ebRecord.Children != null)
            {
                foreach (var child in ebRecord.Children)
                    UnregisterExtensionBuilderRecursively(child);
            }
        }

        #endregion

        #region Extension

        internal void LoadExtensions(IAddinContext adnContext, ExtensionPointRecord epRecord, ExtensionRecordGroup exRecordGroup)
        {
            if (exRecordGroup.Children == null)
                return;
            var parentLoader = GetParentLoader(epRecord, exRecordGroup);
            foreach (var child in exRecordGroup.Children)
                LoadExtensionsRecursively(adnContext, parentLoader, child);
        }

        ICompositeExtensionLoader GetParentLoader(ExtensionPointRecord epRecord, ExtensionRecordGroup exRecordGroup)
        {
            ICompositeExtensionLoader parentLoader;
            if (exRecordGroup.RootIsExtensionPoint)
            {
                if (!TryGetLoader(exRecordGroup.ParentPath, false, out parentLoader))
                {
                    var epLoader = _loaderFactory.CreateExtensionPointLoader(epRecord);
                    _path2Loaders.Add(epRecord.Path, epLoader);
                    parentLoader = epLoader;
                }
            }
            else
            {
                TryGetLoader(exRecordGroup.ParentPath, true, out parentLoader);
            }

            return parentLoader;
        }

        void LoadExtensionsRecursively(IAddinContext adnContext, ICompositeExtensionLoader parentLoader, ExtensionRecord exRecord)
        {
            var exLoader = _loaderFactory.CreateExtensionLoader(exRecord);
            parentLoader.AddChild(exLoader);
            _path2Loaders.Add(exLoader.Path, exLoader);
            exLoader.Load(adnContext); // do load the extension object here.

            if (exRecord.Children != null)
            {
                foreach (var child in exRecord.Children)
                    LoadExtensionsRecursively(adnContext, exLoader as ICompositeExtensionLoader, child);
            }
        }

        internal void UnloadExtensions(IAddinContext adnContext, ExtensionRecordGroup exRecordGroup)
        {
            if (exRecordGroup.Children == null)
                return;

            var extensionRecords = new List<ExtensionRecord>(exRecordGroup.Children);
            GetAllExtensionRecords(extensionRecords);

            // 从最末端开始卸载扩展
            for (int i = extensionRecords.Count - 1; i >= 0; i--)
            {
                var extensionRecord = extensionRecords[i];
                Loader loader;
                TryGetAndRemoveLoader(extensionRecord.Head.Path, true, out loader);
                loader.Unload(adnContext);
            }
        }

        // 广度优先算法
        void GetAllExtensionRecords(List<ExtensionRecord> result)
        {
            for (int i = 0; i < result.Count; i++)
            {
                var ex = result[i];
                if (ex.Children != null)
                {
                    foreach (var e in ex.Children)
                        result.Add(e);
                }
            }
        }

        #endregion

        #region Load

        internal bool TryLoadExtensionPoint(IAddinContext adnContext, ExtensionPointRecord epRecord, object root)
        {
            ExtensionPointLoader epLoader;
            if (!TryGetLoader(epRecord.Path, false, out epLoader))
            {
                if (!_loaderFactory.TryCreateExtensionPointLoader(epRecord, out epLoader))
                    return false;
                _path2Loaders.Add(epRecord.Path, epLoader);
                _loadedExtensionPointRecords.Add(epRecord);
            }
            //if (!epLoader.Loaded)
            //{
                if (!epLoader.TrySetRoot(root))
                    return false;
                epLoader.Load(adnContext);
            //}
            return true;
        }

        internal void LoadExtensionPoint(IAddinContext adnContext, ExtensionPointRecord epRecord, object root)
        {
            ExtensionPointLoader epLoader;
            if (!TryGetLoader(epRecord.Path, false, out epLoader))
            {
                epLoader = _loaderFactory.CreateExtensionPointLoader(epRecord);
                _path2Loaders.Add(epRecord.Path, epLoader);
                _loadedExtensionPointRecords.Add(epRecord);
            }
            //if (!epLoader.Loaded)
            //{
            	epLoader.TrySetRoot(root);
                epLoader.Load(adnContext);
            //}
        }

        internal void UnloadExtensionPoint(IAddinContext adnContext, ExtensionPointRecord epRecord)
		{
		    _loadedExtensionPointRecords.Remove(epRecord);
            Loader epLoader;
            if (!TryGetAndRemoveLoader(epRecord.Path, true, out epLoader))
                return;
            //if (epLoader.Loaded)
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

        bool TryGetAndRemoveLoader(string path, bool fastfail, out Loader result)
        {
            if (!_path2Loaders.TryGetValue(path, out result))
            {
                if (fastfail)
                    throw new InvalidOperationException();
                return false;
            }
            _path2Loaders.Remove(path);
            return true;
        }
    }
}
