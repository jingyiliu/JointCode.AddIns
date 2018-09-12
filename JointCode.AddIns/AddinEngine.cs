//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Extension;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using JointCode.Common.Extensions;

namespace JointCode.AddIns
{
    /// <summary>
    /// Provides a base class for implementing the disposable pattern.
    /// Note that this class derives from <see cref="CriticalFinalizerObject"/>.
    /// </summary>
    public abstract class CriticalDisposable : CriticalFinalizerObject, IDisposable
    {
        bool _disposed;

        /// <summary>
        /// Finalizes the instance. Should never be called.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        ~CriticalDisposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            //    DisposeManagedResources();
            //DisposeUnmanagedResources();
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
                throw new InvalidOperationException(string.Format("The current instance ({0}) has been disposed!", GetType().FullName));
        }
    }

    // the addin engine associates an extension point with an extension root (which might be an ui element), 
    // because an application has only one set of ui, thus the addin engine should have only one instance as well.
    public partial class AddinEngine : CriticalDisposable
    {
        readonly RuntimeAssemblyResolver _assemblyResolver;
        readonly RuntimeExtensionLoader _runtimeExtensionLoader;
        readonly AddinRelationManager _addinRelationManager;
        readonly AddinFramework _addinFramework;
        AddinStorage _addinStorage;

        //public AddinEngine() : this(AddinOptions.Create()) { }

        public AddinEngine(AddinOptions addinOptions)
        {
            _addinRelationManager = new AddinRelationManager();
            _addinFramework = new AddinFramework(addinOptions);
            _assemblyResolver = new RuntimeAssemblyResolver(_addinFramework);
            _runtimeExtensionLoader = new RuntimeExtensionLoader(_assemblyResolver, _addinFramework.ExtensionPointFactory, _addinFramework.ExtensionBuilderFactory);
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            _assemblyResolver.Dispose();
            base.Dispose(disposing);
        }

        internal RuntimeAssemblyResolver RuntimeAssemblyResolver { get { return _assemblyResolver; } }

        public AddinFramework Framework { get { return _addinFramework; } }

        /// <summary>
        /// Initializes the <see cref="AddinEngine"/>, and starts all addins that is enabled, based on the dependencies between them.
        /// </summary>
        /// <param name="shouldRefresh">
        /// if this value is set to <c>true</c>, the <seealso cref="AddinEngine"/> will scan the addin probing directories for 
        /// new and updated addins every time this method is called.
        /// otherwise, it will only scan the addin probing directories once.
        /// </param>
        public bool Start(bool shouldRefresh)
        {
            if (!Initialize(shouldRefresh))
                return false;
            var enabledAddinRecords = _addinStorage.GetEnabledAddins();
            for (int i = 0; i < enabledAddinRecords.Count; i++)
            {
                var addin = GetOrCreateAddin(enabledAddinRecords[i]);
                addin.Start();
            }
            return true;
        }

        /// <summary>
        /// Starts all addins that is enabled, based on the dependencies between them.
        /// Notes that this method can only be called after the <see cref="AddinEngine"/> has been initialized,
        /// otherwise an <see cref="InvalidOperationException"/> will be thrown.
        /// </summary>
        public void Start()
        {
            VerifyInitialized();
            var enabledAddinRecords = _addinStorage.GetEnabledAddins();
            for (int i = 0; i < enabledAddinRecords.Count; i++)
            {
                var addin = GetOrCreateAddin(enabledAddinRecords[i]);
                addin.Start();
            }
        }

        /// <summary>
        /// Stops the <see cref="AddinEngine"/>, and all addins that is started, based on the dependencies between them.
        /// </summary>
        public void Stop()
        {
            var startedAddins = _addinFramework.Repository.GetStartedAddins();
            for (int i = startedAddins.Length - 1; i >= 0; i--)
                startedAddins[i].Stop();

            _runtimeExtensionLoader.Reset();
            _addinFramework.Repository.Reset();
            _addinRelationManager.Reset();
            _addinStorage.Reset();
            _assemblyResolver.Reset();

            _initialized = false;
        }

        internal void UpdateStorageFile()
        {
            _addinStorage.Write();
        }

        #region Addin

        public int AddinCount { get { return _addinFramework.Repository.AddinCount; } }

        public IEnumerable<Addin> GetAllAddins()
        {
            VerifyInitialized();
            return _addinFramework.Repository.AddinCount == 0 ? null : new List<Addin>(_addinFramework.Repository.Addins);
        }

        public Addin[] GetStartedAddins()
        {
            VerifyInitialized();
            return _addinFramework.Repository.AddinCount == 0 ? null : _addinFramework.Repository.GetStartedAddins();
        }

        public Addin GetAddin(Guid guid)
        {
            VerifyInitialized();
            Addin addin;
            return _addinFramework.Repository.TryGetAddin(ref guid, out addin) ? addin : null;
        }

        public bool TryGetAddin(Guid guid, out Addin addin)
        {
            VerifyInitialized();
            return _addinFramework.Repository.TryGetAddin(ref guid, out addin);
        }

        public Addin GetAddin(string name)
        {
            VerifyInitialized();
            Addin addin;
            return _addinFramework.Repository.TryGet(name, out addin) ? addin : null;
        }

        public bool TryGetAddin(string name, out Addin addin)
        {
            VerifyInitialized();
            return _addinFramework.Repository.TryGet(name, out addin);
        }

        Addin GetOrCreateAddin(AddinRecord addinRecord)
        {
            Addin result;
            if (_addinFramework.Repository.TryGetAddin(ref addinRecord.AddinId._guid, out result))
                return result;

            result = new Addin(this, _addinFramework, addinRecord);
            _addinFramework.Repository.AddAddin(result);

            // todo: 如果一个插件的任何一个父插件被停止、禁用，则该插件在启用时，都需要重新解析获取其父插件
            List<Addin> parentAddins = null, childAddins = null;

            var parentAddinRecords = _addinRelationManager.TryGetDependedAddins(addinRecord);
            if (parentAddinRecords != null)
            {
                parentAddins = new List<Addin>();
                foreach (var parentAddinRecord in parentAddinRecords)
                    parentAddins.Add(GetOrCreateAddin(parentAddinRecord));
            }

            var childAddinRecords = _addinRelationManager.TryGetAffectingAddins(addinRecord);
            if (childAddinRecords != null)
            {
                childAddins = new List<Addin>();
                foreach (var childAddinRecord in childAddinRecords)
                    childAddins.Add(GetOrCreateAddin(childAddinRecord));
            }

            result.SetParentAddins(parentAddins);
            result.SetChildAddins(childAddins);

            return result;
        }

        #endregion

        #region ExtensionPoint
        ArgumentException GetExtensionPointNotFoundException(string extensionPointPath)
        {
            return new ArgumentException(string.Format("Can not find the extension point with the specified path [{0}]!",
                extensionPointPath));
        }

        ArgumentException GetExtensionPointNotFoundException(Type extensionRootType, string extensionPointPath)
        {
            return new ArgumentException(string.Format("The provided extension root type is [{0}], which is translated into an extension point path [{1}] by the name convention of addin settings, but we can not find the extension point with that path!",
                extensionRootType.ToFullTypeName(), extensionPointPath));
        }

        public bool TryLoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        {
            VerifyInitialized();
            var extensionPointPath = _addinFramework.NameConvention.GetExtensionPointName(typeof(TExtensionRoot));
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                return false;
            //if (extensionPointRecord.Loaded)
            //    return true;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            return addin.Extension.TryLoadExtensionPoint(extensionPointRecord, extensionRoot); 
        }

        public bool TryLoadExtensionPoint(string extensionPointPath, object extensionRoot)
        {
            VerifyInitialized();
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                return false;
            //if (extensionPointRecord.Loaded)
            //    return true;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            return addin.Extension.TryLoadExtensionPoint(extensionPointRecord, extensionRoot);
        }

        public void LoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        {
            VerifyInitialized();
            var extensionPointPath = _addinFramework.NameConvention.GetExtensionPointName(typeof(TExtensionRoot));
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                throw GetExtensionPointNotFoundException(typeof(TExtensionRoot), extensionPointPath);
            //if (extensionPointRecord.Loaded)
            //    return;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            addin.Extension.LoadExtensionPoint(extensionPointRecord, extensionRoot);
        }

        public void LoadExtensionPoint(string extensionPointPath, object extensionRoot)
        {
            VerifyInitialized();
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                throw GetExtensionPointNotFoundException(extensionPointPath);
            //if (extensionPointRecord.Loaded)
            //    return;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            addin.Extension.LoadExtensionPoint(extensionPointRecord, extensionRoot);
        }

        public void UnloadExtensionPoint(Type extensionRootType)
        {
            VerifyInitialized();
            var extensionPointPath = _addinFramework.NameConvention.GetExtensionPointName(extensionRootType);
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                throw GetExtensionPointNotFoundException(extensionRootType, extensionPointPath);
            //if (extensionPointRecord.Loaded)
            //    return;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            addin.Extension.UnloadExtensionPoint(extensionPointRecord);
        }

        public void UnloadExtensionPoint(string extensionPointPath)
        {
            VerifyInitialized();
            var extensionPointRecord = _addinRelationManager.GetExtensionPointByPath(extensionPointPath);
            if (extensionPointRecord == null)
                throw GetExtensionPointNotFoundException(extensionPointPath);
            //if (extensionPointRecord.Loaded)
            //    return;
            var addin = GetOrCreateAddin(extensionPointRecord.AddinRecord);
            addin.Extension.UnloadExtensionPoint(extensionPointRecord);
        }

        internal bool TryLoadExtensionPoint(IAddinContext declaringAddin, ExtensionPointRecord extensionPointRecord, object extensionRoot)
        {
            VerifyInitialized();

            if (extensionPointRecord.Loaded)
                return true;
            
            _runtimeExtensionLoader.RegisterExtensionPoint(extensionPointRecord, extensionRoot.GetType());
            if (!_runtimeExtensionLoader.TryLoadExtensionPoint(declaringAddin, extensionPointRecord, extensionRoot))
                return false;
            extensionPointRecord.Loaded = true;

            DoLoadIntoExtensionPoint(declaringAddin, extensionPointRecord);
            return true;
        }

        internal void LoadExtensionPoint(IAddinContext declaringAddin, ExtensionPointRecord extensionPointRecord, object extensionRoot)
        {
            VerifyInitialized();

            if (extensionPointRecord.Loaded)
                return;

            // register the extension point itself
            _runtimeExtensionLoader.RegisterExtensionPoint(extensionPointRecord, extensionRoot.GetType());
            _runtimeExtensionLoader.LoadExtensionPoint(declaringAddin, extensionPointRecord, extensionRoot);
            extensionPointRecord.Loaded = true;

            DoLoadIntoExtensionPoint(declaringAddin, extensionPointRecord);
        }

        void DoLoadIntoExtensionPoint(IAddinContext declaringAddin, ExtensionPointRecord extensionPointRecord)
        {
            // register extensions of the same addin that extends the extension point
            LoadIntoExtensionPoint(declaringAddin, extensionPointRecord);

            // register extensions of other addins (extending addins) that extending the extension point.
            var extendingAddinRecords = _addinRelationManager.TryGetSortedExtendingAddins(extensionPointRecord);
            if (extendingAddinRecords != null)
            {
                for (int i = 0; i < extendingAddinRecords.Count; i++)
                {
                    var extendingAddin = GetOrCreateAddin(extendingAddinRecords[i]);
                    extendingAddin.Extension.LoadInto(extensionPointRecord);
                }
            }
        }

        internal void UnloadExtensionPoint(IAddinContext declaringAddin, ExtensionPointRecord extensionPointRecord)
        {
            VerifyInitialized();

            //if (!ExtensionPointIsLoaded(extensionPointRecord))
            //    throw new InvalidOperationException(string.Format("The specified extension point [{0}] is not loaded, which do not need to be unloaded!", extensionPointRecord.Id));

            if (!extensionPointRecord.Loaded)
                return;

            DoUnloadFromExtensionPoint(declaringAddin, extensionPointRecord);

            _runtimeExtensionLoader.UnloadExtensionPoint(declaringAddin, extensionPointRecord);
            _runtimeExtensionLoader.UnregisterExtensionPoint(extensionPointRecord);
            extensionPointRecord.Loaded = false;
        }

        void DoUnloadFromExtensionPoint(IAddinContext declaringAddin, ExtensionPointRecord extensionPointRecord)
        {
            // register extensions of other addins (extending addins) that extending the extension point.
            var extendingAddinRecords = _addinRelationManager.TryGetSortedExtendingAddins(extensionPointRecord);
            if (extendingAddinRecords != null)
            {
                for (int i = extendingAddinRecords.Count - 1; i >= 0; i--)
                {
                    var extendingAddin = GetOrCreateAddin(extendingAddinRecords[i]);
                    UnloadFromExtensionPoint(extendingAddin.Context, extensionPointRecord);
                }
            }
            // register extensions of the same addin that extends the extension point
            UnloadFromExtensionPoint(declaringAddin, extensionPointRecord);
        }
        #endregion

        #region ExtensionBuilder/Extension
        // if there is any extension points has been loaded for which this addin extends, loads the extension builders and extensions of this addin [addinRecord] 
        // that extending the extension point.
        internal void LoadIntoLoadedExtensionPoints(DefaultAddinContext adnContext, AddinRecord addinRecord)
        {
            if (_runtimeExtensionLoader.LoadedExtensionPointCount == 0)
                return;
            for (int i = 0; i < _runtimeExtensionLoader.LoadedExtensionPointCount; i++)
            {
                var loadedExtensionPointRecord = _runtimeExtensionLoader.GetLoadedExtensionPoint(i);
                LoadIntoExtensionPoint(adnContext, loadedExtensionPointRecord);
            }
        }

        // register extensions of the specified addin (addinRecord) that extends the specified extenstion point.
        internal void LoadIntoExtensionPoint(IAddinContext adnContext, ExtensionPointRecord extensionPointRecord)
        {
            var addinRecord = adnContext.Addin.AddinRecord;

            var ebGroup = addinRecord.GetExtensionBuilderGroup(extensionPointRecord.Path);
            if (ebGroup != null && !ebGroup.Loaded)
            {
                _runtimeExtensionLoader.RegisterExtensionBuilders(ebGroup);
                ebGroup.Loaded = true;
            }

            var exGroup = addinRecord.GetExtensionGroup(extensionPointRecord.Path);
            if (exGroup != null && !exGroup.Loaded)
            {
                _runtimeExtensionLoader.LoadExtensions(adnContext, extensionPointRecord, exGroup);
                exGroup.Loaded = true;
            }
        }

        internal void UnloadFromLoadedExtensionPoints(DefaultAddinContext adnContext)
        {
            if (_runtimeExtensionLoader.LoadedExtensionPointCount == 0)
                return;
            for (int i = 0; i < _runtimeExtensionLoader.LoadedExtensionPointCount; i++)
            {
                var loadedExtensionPointRecord = _runtimeExtensionLoader.GetLoadedExtensionPoint(i);
                UnloadFromExtensionPoint(adnContext, loadedExtensionPointRecord);
            }
        }

        // register extensions that extends an extenstion point declared in another addin.
        void UnloadFromExtensionPoint(IAddinContext adnContext, ExtensionPointRecord extensionPointRecord)
        {
            var addinRecord = adnContext.Addin.AddinRecord;
            var exGroup = addinRecord.GetExtensionGroup(extensionPointRecord.Path);
            if (exGroup != null && exGroup.Loaded)
            {
                _runtimeExtensionLoader.UnloadExtensions(adnContext, exGroup);
                exGroup.Loaded = false;
            }

            var ebGroup = addinRecord.GetExtensionBuilderGroup(extensionPointRecord.Path);
            if (ebGroup != null && ebGroup.Loaded)
            {
                _runtimeExtensionLoader.UnregisterExtensionBuilders(ebGroup);
                ebGroup.Loaded = false;
            }
        }
        #endregion
    }
}
