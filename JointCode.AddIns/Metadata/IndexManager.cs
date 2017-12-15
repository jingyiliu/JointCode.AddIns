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
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata
{
    partial class IndexManager
    {
        static bool _changed;

        internal static int GetNextExtensionBuilderUid()
        {
            _changed = true;
            return UidProvider.GetNextExtensionBuilderUid();
        }

        internal static int GetNextExtensionPointUid()
        {
            _changed = true;
            return UidProvider.GetNextExtensionPointUid();
        }

        internal static int GetNextAssemblyUid()
        {
            _changed = true;
            return UidProvider.GetNextAssemblyUid();
        }

        internal static int GetNextAddinUid()
        {
            _changed = true;
            return UidProvider.GetNextAddinUid();
        }
    }

    partial class IndexManager
    {
        #region all assets managed by this instance
        List<AddinIndexRecord> _addins;
        List<AddinFilePack> _invalidAddinFilePacks;
        //// assemblies managed individually, including assemblis provided by application directory or other locations.
        //List<AssemblyFileRecord> _standaloneAssemblies;  
        #endregion

        #region persistence
        readonly Guid _guid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4);
        Storage _storage; 
        #endregion
        
        #region relationship mappers
        //key = assembly uid, value: addin that provides this assembly
        readonly Dictionary<int, AddinIndexRecordSet> _assemblyUid2AddinSets;
        //key = ExtensionPoint.Id, value: addin that defined this extension point
        readonly Dictionary<string, AddinIndexRecord> _ep2DeclaringAddins;
        #endregion
        
        internal IndexManager()
        { 
        	_addins = new List<AddinIndexRecord>();
        	_ep2DeclaringAddins = new Dictionary<string, AddinIndexRecord>();
            _assemblyUid2AddinSets = new Dictionary<int, AddinIndexRecordSet>();
        }

        internal Storage Storage
        {
            get { return _storage; } 
            set { _storage = value; }
        }

        internal bool Changed { get { return _changed; } }

        internal int AddinCount { get { return _addins == null ? 0 : _addins.Count; } }
        internal IEnumerable<AddinIndexRecord> Addins { get { return _addins; } }

        internal int InvalidAddinFilePackCount { get { return _invalidAddinFilePacks == null ? 0 : _invalidAddinFilePacks.Count; } }
        internal IEnumerable<AddinFilePack> InvalidAddinFilePacks { get { return _invalidAddinFilePacks; } }

        internal AddinIndexRecord GetAddin(int index)
        {
            return _addins[index];
        }
             
        #region Addin CUD
        //internal void UpdateAddin(AddinIndexRecord addin)
        //{
        //    _changed = true;
        //    for (int i = 0; i < _addins.Count; i++)
        //    {
        //        var oldAddinId = _addins[i].AddinHeader.AddinId;
        //        var newAddinId = addin.AddinHeader.AddinId;
        //        if (ReferenceEquals(oldAddinId.Tag, newAddinId.Tag) && oldAddinId.Guid == newAddinId.Guid)
        //        {
        //            _addins[i] = addin;
        //            break;
        //        }
        //    }
        //}

        internal void AddAddin(AddinIndexRecord addin)
        {
            _changed = true;
            _addins.Add(addin);
        }

        internal bool RemoveAddin(AddinIndexRecord addin)
        {
            _changed = true;
            _addins.Remove(addin);
            //RemoveRelationMap(addin);
            return _changed;
        }
        #endregion

        #region InvalidAddinFilePack CUD
        internal void AddInvalidAddinFilePack(AddinFilePack addinFilePack)
        {
            if (_invalidAddinFilePacks == null)
            {
                _changed = true;
                _invalidAddinFilePacks = _invalidAddinFilePacks ?? new List<AddinFilePack>();
                _invalidAddinFilePacks.Add(addinFilePack);
            }
            else
            {
                var addinDiretory = addinFilePack.AddinDirectory;
                for (int i = 0; i < _invalidAddinFilePacks.Count; i++)
                {
                    var invalidAddinFilePack = _invalidAddinFilePacks[i];
                    if (addinDiretory.Equals(invalidAddinFilePack.AddinDirectory, StringComparison.InvariantCultureIgnoreCase))
                        return;
                }
                _changed = true;
                _invalidAddinFilePacks.Add(addinFilePack);
            }
        }

        internal bool RemoveInvalidAddinFilePack(string addinDirectory)
        {
            if (InvalidAddinFilePackCount == 0)
                return false;
            for (int i = 0; i < _invalidAddinFilePacks.Count; i++)
            {
                var invalidAddinFilePack = _invalidAddinFilePacks[i];
                if (!addinDirectory.Equals(invalidAddinFilePack.AddinDirectory, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                _changed = true;
                _invalidAddinFilePacks.RemoveAt(i);
                return true;
            }
            return false;
        }
        #endregion
        
        #region Relation
        /// <summary>
        /// Get the addin that declared the specified extension point.
        /// </summary>
        internal AddinIndexRecord GetDeclaringAddin(string extensionPointId)
        {
            if (AddinCount == 0)
                return null;
            AddinIndexRecord result;
            return _ep2DeclaringAddins.TryGetValue(extensionPointId, out result) ? result : null;
        } 

        /// <summary>
        /// Get all addins that the sepecified addin refers to (recursively).
        /// </summary>
        /// <returns></returns>
        internal bool TryGetAllReferencedAddins(AddinIndexRecord addin, out List<AddinIndexRecord> result)
        {
            result = null;
            if (AddinCount == 0)
                return false;

            List<AddinIndexRecord> referencedAddins;
            if (!DoTryGetReferencedAddins(addin, out referencedAddins))
                return false;

            if (referencedAddins == null)
                return true;

            for (int i = 0; i < referencedAddins.Count; i++)
            {
                var referencedAddin = referencedAddins[i];
                List<AddinIndexRecord> subReferencedAddins;
                if (!DoTryGetReferencedAddins(referencedAddin, out subReferencedAddins))
                    return false;
                if (subReferencedAddins != null)
                {
                    foreach (var subReferencedAddin in subReferencedAddins)
                    {
                        if (!referencedAddins.Contains(subReferencedAddin))
                            referencedAddins.Add(subReferencedAddin);
                    }
                }
            }

            result = referencedAddins;
            return true;
        }
        
        /// <summary>
        /// Get a list of addins that the sepecified addin refers to directly (not recursively).
        /// </summary>
        internal bool TryGetReferencedAddins(AddinIndexRecord addin, out List<AddinIndexRecord> result)
        {
            result = null;
            return AddinCount == 0
                ? false
                : DoTryGetReferencedAddins(addin, out result);
        }
        
        // get the direct references of an addin (not recursively)
        // the references of an addin is determined until the first time this method is called.
        bool DoTryGetReferencedAddins(AddinIndexRecord addin, out List<AddinIndexRecord> result)
        {
            result = null;
            if (addin.ReferencedAssemblies == null)
                return true;

            result = new List<AddinIndexRecord>();

            foreach (var referencedAssembly in addin.ReferencedAssemblies)
            {
                AddinIndexRecordSet referencedAddinSet;
                if (!_assemblyUid2AddinSets.TryGetValue(referencedAssembly.Uid, out referencedAddinSet))
                    throw new Exception();

                var referencedAddin = SelectReferencedAddin(referencedAddinSet);
                if (referencedAddin == null)
                    return false;

                if (!result.Contains(referencedAddin))
                    result.Add(SelectReferencedAddin(referencedAddinSet));
            }

            return result.Count > 0;
        }

        static AddinIndexRecord SelectReferencedAddin(AddinIndexRecordSet referencedAddinSet)
        {
            // if any of the referenced addin is loaded, use it
            foreach (var referencedAddin in referencedAddinSet)
            {
                if (referencedAddin.AssembliesRegistered)
                    return referencedAddin;
            }
            // otherwise, select the first (top priority) referenced addin directly
            foreach (var referencedAddin in referencedAddinSet)
            {
                if (referencedAddin.RunningStatus == AddinRunningStatus.Enabled)
                    return referencedAddin;
            }
            return null;
        }

        /// <summary>
        /// Get a list of addins that extend the specified extension point (provide extensions for it), 
        /// sorted by the dependence relationship between them.
        /// </summary>
        /// <param name="extensionPoint">The extension point.</param>
        /// <returns></returns>
        internal List<AddinIndexRecord> TryGetSortedExtendingAddins(ExtensionPointRecord extensionPoint)
        {
            var result = new List<AddinIndexRecord>();
            for (int i = 0; i < _addins.Count; i++)
            {
                var addin = _addins[i];
                if (addin.RunningStatus == AddinRunningStatus.Enabled && addin.ExtendsExtensionPoint(extensionPoint.Uid))
                    result.Add(addin);
            }

            return result;
        }

        /// <summary>
        /// Get a list of addins that directly refers to or extends the sepecified addins, which will be affected by the
        /// status change of the later.
        /// </summary>
        /// <param name="changedAddins">A list of addins that should be checked for affected addins.</param>
        /// <returns></returns>
        internal List<AddinIndexRecord> TryGetAffectedAddins(List<AddinIndexRecord> changedAddins)
        {
            var result = new List<AddinIndexRecord>();
            foreach (var changedAddin in changedAddins)
            {
                if (changedAddin.ExtensionPoints != null)
                {
                    foreach (var ep in changedAddin.ExtensionPoints)
                    {
                        foreach (var existingAddin in _addins)
                        {
                            if (existingAddin.ExtendsExtensionPoint(ep.Uid) && !result.Contains(existingAddin))
                                result.Add(existingAddin);
                        }
                    }
                }
                if (changedAddin.AssemblyFiles != null)
                {
                    foreach (var assemblyFile in changedAddin.AssemblyFiles)
                    {
                        if (!AllAddinsProvidedAssemblyAreAffected(assemblyFile, changedAddins, result))
                            continue;
                        foreach (var existingAddin in _addins)
                        {
                            if (existingAddin.RefersToAssembly(assemblyFile.Uid) && !result.Contains(existingAddin))
                                result.Add(existingAddin);
                        }
                    }
                }
            }
            return result.Count > 0 ? result : null;
        }

        // whether all addins that provided the specified assembly file are in pending status.
        bool AllAddinsProvidedAssemblyAreAffected(AssemblyFileRecord asmFileOfChangedAddin, List<AddinIndexRecord> changedAddins, 
            List<AddinIndexRecord> affectedAddins)
        {
            AddinIndexRecordSet adns;
            if (!_assemblyUid2AddinSets.TryGetValue(asmFileOfChangedAddin.Uid, out adns))
                throw new InvalidOperationException();

            if (adns.Count == 1)
                return true;

            if (adns.Count > 1)
            {
                bool allPending = true;
                foreach (var adn in adns)
                {
                    if (!affectedAddins.Contains(adn) && !changedAddins.Contains(adn))
                    {
                        allPending = false;
                        break;
                    }
                }
                return allPending;
            }
            return false;
        }

        /// <summary>
        /// Get all addins that directly/indirectly refers to or extends the sepecified addin, which will be affected by the 
        /// status change of the later.
        /// </summary>
        internal List<AddinIndexRecord> TryGetAllAffectedAddins(List<AddinIndexRecord> changedAddins)
        {
            var result = TryGetAffectedAddins(changedAddins);
            if (result == null)
                return null;

            for (int i = 0; i < result.Count; i++)
            {
                var subItems = TryGetAffectedAddins(result);
                if (subItems == null)
                    continue;
                foreach (var subItem in subItems)
                {
                    if (!result.Contains(subItem))
                        result.Add(subItem);
                }
            }
            return result.Count > 0 ? result : null;
        }
        #endregion

        // builds and caches the relationship maps between addins
        internal void Build()
        {
            if (AddinCount == 0)
                return;
            for (int i = 0; i < _addins.Count; i++)
            {
                var addin = _addins[i];
                if (addin.RunningStatus != AddinRunningStatus.Enabled)
                    continue;
                AddRelationMap(_addins[i]);
            }
        }

        void AddRelationMap(AddinIndexRecord addin)
        {
            if (addin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in addin.AssemblyFiles)
                {
                    AddinIndexRecordSet addinSet;
                    if (!_assemblyUid2AddinSets.TryGetValue(assemblyFile.Uid, out addinSet))
                    {
                        addinSet = new AddinIndexRecordSet();
                        _assemblyUid2AddinSets.Add(assemblyFile.Uid, addinSet);
                    }
                    addinSet.Add(addin);
                }
            }

            if (addin.ExtensionPoints != null)
            {
                foreach (var extensionPoint in addin.ExtensionPoints)
                    _ep2DeclaringAddins.Add(extensionPoint.Id, addin);
            }
        }

        void RemoveRelationMap(AddinIndexRecord addin)
        {
            if (addin.ExtensionPoints != null)
            {
                foreach (var extensionPoint in addin.ExtensionPoints)
                    _ep2DeclaringAddins.Remove(extensionPoint.Id);
            }

            if (addin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in addin.AssemblyFiles)
                {
                    AddinIndexRecordSet addinSet;
                    if (!_assemblyUid2AddinSets.TryGetValue(assemblyFile.Uid, out addinSet))
                        continue;
                    if (addinSet.Remove(addin) && addinSet.Count == 0)
                        _assemblyUid2AddinSets.Remove(assemblyFile.Uid);
                }
            }
        }

        internal bool Read()
        {
        	if (!_storage.ContainsStream(_guid))
        		return false;

            try
            {
                using (var stream = _storage.OpenStream(_guid))
                {
                    UidProvider.Read(stream);
                    _addins = RecordHelpers.Read(stream, ref AddinIndexRecord.Factory);
                    _invalidAddinFilePacks = RecordHelpers.Read(stream, ref AddinFilePack.Factory);
                    var position = stream.Position;
                    var length = stream.ReadInt64();
                    if (position != length)
                        return false;
                }
                return _addins != null && _addins.Count > 0;
            }
            catch
            {
                // log
                throw;
            }
        }
        
        internal void Write()
        {
            if (!_changed)
                return;

            if (!_storage.ContainsStream(_guid))
                _storage.CreateStream(_guid);

            using (var stream = _storage.OpenStream(_guid))
            {
                UidProvider.Write(stream);
                RecordHelpers.Write(stream, _addins);
                RecordHelpers.Write(stream, _invalidAddinFilePacks);
                stream.WriteInt64(stream.Position);
                // 调整 Stream 的长度。
                // 因为更新后的长度可能小于先前的长度，此处通过调整 Stream 的大小，可以减少持久化文件的尺寸。
                stream.SetLength(stream.Position);
            }

            _changed = false;
        }
    }
}
