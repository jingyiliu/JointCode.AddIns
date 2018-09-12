//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Metadata.Assets;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Metadata
{
    class AddinRelationManager
    {
        //readonly List<ApplicationAssemblyRecord> _applicationAssemblies;
        List<AddinRecord> _addinRecords;
        //Dictionary<Guid, AddinRecord> _guid2AddinRecords;
        //Dictionary<int, AddinRecord> _uid2AddinRecords;

        #region relationship mappers
        //key = assembly uid, value: addins that provides this assembly
        Dictionary<int, AddinRecordSet> _assemblyUid2AddinSets;
        //key = extension point path (ExtensionPoint.Id), value: extension point, which contains the addin that defined this extension point
        Dictionary<string, ExtensionPointRecord> _path2ExtensionPoints;
        #endregion
        
        internal AddinRelationManager()
        {
            Reset();
        }

        internal void Reset()
        {
            //_applicationAssemblies = new List<ApplicationAssemblyRecord>();
            _addinRecords = new List<AddinRecord>();
            //_guid2AddinRecords = new Dictionary<Guid, AddinRecord>();
            //_uid2AddinRecords = new Dictionary<int, AddinRecord>();
            _path2ExtensionPoints = new Dictionary<string, ExtensionPointRecord>();
            _assemblyUid2AddinSets = new Dictionary<int, AddinRecordSet>();
        }

        ///// <summary>
        ///// Determines whether the depended assembly located at the application base directory.
        ///// </summary>
        ///// <param name="assemblyDependency"></param>
        ///// <returns></returns>
        //internal bool HasApplicationAssembly(AssemblyDependency assemblyDependency)
        //{
        //    return false;
        //}

        //internal void AddApplicationAssembly(ApplicationAssemblyRecord applicationAssembly)
        //{
        //    _applicationAssemblies.Add(applicationAssembly);
        //}

        //internal void AddApplicationAssemblies(IEnumerable<ApplicationAssemblyRecord> applicationAssemblies)
        //{
        //    foreach (var applicationAssembly in applicationAssemblies)
        //        _applicationAssemblies.Add(applicationAssembly);
        //}

        internal bool TryGetAddin(Guid guid, out AddinRecord addin)
        {
            for (int i = 0; i < _addinRecords.Count; i++)
            {
                var addinRecord = _addinRecords[i];
                if (addinRecord.Guid == guid)
                {
                    addin = addinRecord;
                    return true;
                }
            }
            addin = null;
            return false;
        }

        AddinRecord GetAddinByUid(int uid)
        {
            for (int i = 0; i < _addinRecords.Count; i++)
            {
                var addinRecord = _addinRecords[i];
                if (addinRecord.Uid == uid)
                    return addinRecord;
            }
            return null;
        }

        /// <summary>
        /// Get the extension point by the specified path.
        /// </summary>
        internal ExtensionPointRecord GetExtensionPointByPath(string extensionPointPath)
        {
            ExtensionPointRecord result;
            return _path2ExtensionPoints.TryGetValue(extensionPointPath, out result) ? result : null;
        }

        #region DependencyDescription: ReferencedAddins
        /// <summary>
        /// Get all addins that the sepecified addin refers to (recursively).
        /// </summary>
        /// <returns></returns>
        internal bool TryGetAllReferencedAddins(AddinRecord addin, out List<AddinRecord> result)
        {
            result = null;

            List<AddinRecord> referencedAddins;
            if (!DoTryGetReferencedAddins(addin, out referencedAddins))
                return false;

            for (int i = 0; i < referencedAddins.Count; i++)
            {
                var referencedAddin = referencedAddins[i];
                List<AddinRecord> subReferencedAddins;
                if (!DoTryGetReferencedAddins(referencedAddin, out subReferencedAddins))
                    continue;
                foreach (var subReferencedAddin in subReferencedAddins)
                {
                    if (!referencedAddins.Contains(subReferencedAddin))
                        referencedAddins.Add(subReferencedAddin);
                }
            }

            result = referencedAddins;
            return true;
        }

        /// <summary>
        /// Get a list of addins that the sepecified addin refers to directly (not recursively).
        /// </summary>
        internal bool TryGetReferencedAddins(AddinRecord addin, out List<AddinRecord> result)
        {
            return DoTryGetReferencedAddins(addin, out result);
        }

        // get the direct references of an addin (not recursively)
        // the references of an addin is determined until the first time this method is called.
        bool DoTryGetReferencedAddins(AddinRecord addin, out List<AddinRecord> result)
        {
            result = null;
            //if (addin.ReferencedAssemblies == null)
            //    return false;

            //result = new List<AddinRecord>();

            //foreach (var referencedAssembly in addin.ReferencedAssemblies)
            //{
            //    AddinRecordSet referencedAddinSet;
            //    if (!_assemblyUid2AddinSets.TryGetValue(referencedAssembly.Uid, out referencedAddinSet))
            //        throw new InconsistentStateException(string.Format("Can not find the referenced addin for an assembly of addin [{0}], which has been resolved successfully!", addin.AddinId.Guid));

            //    var referencedAddin = SelectReferencedAddin(referencedAddinSet);
            //    if (referencedAddin == null)
            //        throw new InconsistentStateException(string.Format("Can not find the referenced addin for an assembly of addin [{0}] which has been resolved successfully, because none of referenced addins is in enabled status!", addin.AddinId.Guid));

            //    if (!result.Contains(referencedAddin))
            //        result.Add(SelectReferencedAddin(referencedAddinSet));
            //}

            //return result.Count > 0;

            return false;
        }

        static AddinRecord SelectReferencedAddin(AddinRecordSet referencedAddinSet)
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
                if (referencedAddin.Enabled)
                    return referencedAddin;
            }
            return null;
        }
        #endregion

        #region DependencyDescription: ExtendedAddins
        /// <summary>
        /// Get a list of addins that the sepecified addin extends directly (not recursively).
        /// </summary>
        internal bool TryGetExtendedAddins(AddinRecord addin, out List<AddinRecord> result)
        {
            if (addin.ExtendedAddins == null || addin.ExtendedAddins.Count == 0)
            {
                result = null;
                return false;
            }
            result = new List<AddinRecord>(addin.ExtendedAddins.Count);
            for (int i = 0; i < addin.ExtendedAddins.Count; i++)
            {
                var ea = addin.ExtendedAddins[i];
                var ad = GetAddinByUid(ea.Uid);
                result.Add(ad);
            }
            return true;
        }

        /// <summary>
        /// Get a list of addins that extend the specified extension point (provide extensions for it), 
        /// sorted by the dependence relationship between them.
        /// </summary>
        /// <param name="extensionPoint">The extension point.</param>
        /// <returns></returns>
        internal AddinRecordSet TryGetSortedExtendingAddins(ExtensionPointRecord extensionPoint)
        {
            var result = new AddinRecordSet();
            for (int i = 0; i < _addinRecords.Count; i++)
            {
                var addin = _addinRecords[i];
                if (addin.Enabled && addin.ExtendsExtensionPoint(extensionPoint.Uid))
                    result.Add(addin);
            }

            return result;
        }
        #endregion

        #region 运行时 DependedAddins
        /// <summary>
        /// Get all addins that the sepecified addin directly/indirectly depended (refers to or extends), whose status change will affect this addin.
        /// </summary>
        internal AddinRecordSet TryGetAllDependedAddins(AddinRecord addin)
        {
            var result = TryGetDependedAddins(addin);
            if (result == null)
                return null;

            for (int i = 0; i < result.Count; i++)
            {
                var subItems = TryGetDependedAddins(result[i]);
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

        /// <summary>
        /// Get a list of addins that the sepecified addin directly depended (refers to or extends), whose status change will affect this addin.
        /// </summary>
        internal AddinRecordSet TryGetDependedAddins(AddinRecord addin)
        {
            var result = new AddinRecordSet();
            if (addin.ExtendedAddins != null)
            {
                foreach (var extendedAddin in addin.ExtendedAddins)
                    result.Add(GetAddinByUid(extendedAddin.Uid));
            }
            if (addin.ReferencedAssemblies != null)
            {
                foreach (var referencedAssembly in addin.ReferencedAssemblies)
                {
                    var referencedAddins = _assemblyUid2AddinSets[referencedAssembly.Uid];
                    var referencedAddin = referencedAddins.SelectFirst();
                    if (!result.Contains(referencedAddin))
                        result.Add(referencedAddin);
                }
            }
            return result.Count > 0 ? result : null;
        } 
        #endregion

        #region 运行时 AffectingAddins
        /// <summary>
        /// Get all addins that directly/indirectly refers to or extends the sepecified addin, which will be affected by the 
        /// status change of the later.
        /// </summary>
        internal AddinRecordSet TryGetAllAffectingAddins(AddinRecord addin)
        {
            var result = TryGetAffectingAddins(addin);
            if (result == null)
                return null;

            for (int i = 0; i < result.Count; i++)
            {
                var subItems = TryGetAffectingAddins(result[i]);
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

        /// <summary>
        /// Get a list of addins that directly refers to or extends the sepecified addin, which will be affected by the
        /// status change of the later.
        /// </summary>
        /// <param name="addin">The addin that should be checked for affected addins.</param>
        /// <returns></returns>
        internal AddinRecordSet TryGetAffectingAddins(AddinRecord addin)
        {
            var result = new AddinRecordSet();

            // 获取当前扩展了指定插件 (addin) 的现有插件
            foreach (var existingAddin in _addinRecords)
            {
                if (!existingAddin.Enabled)
                    continue;
                if (existingAddin.ExtendsAddin(addin.Uid))
                    result.Add(existingAddin);
            }

            // 获取当前引用了该插件 (addin) 程序集的其他现有插件
            if (addin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in addin.AssemblyFiles)
                {
                    if (!IsDefaultSelectedAddinForAssembly(assemblyFile, addin))
                        continue; // 如果当前插件 (addin) 并非被选为默认提供该程序集 (assemblyFile) 的插件，则不会有其他插件依赖当前插件
                    foreach (var existingAddin in _addinRecords)
                    {
                        if (!existingAddin.RefersToAssembly(assemblyFile.Uid))
                             continue;
                        if (!result.Contains(existingAddin))
                            result.Add(existingAddin);
                    }
                }
            }

            return result.Count > 0 ? result : null;
        }

        // when other addins refers to the specified [assemblyFile], is the specified [addin] is the default selected addin
        bool IsDefaultSelectedAddinForAssembly(AssemblyFileRecord assemblyFile, AddinRecord addin)
        {
            AddinRecordSet adns;
            if (!_assemblyUid2AddinSets.TryGetValue(assemblyFile.Uid, out adns))
                throw new InvalidOperationException();
            return ReferenceEquals(adns.SelectFirstOrNull(), addin);
        }
        #endregion

        #region 解析时 AffectingAddins（判断直接和间接受更新影响的插件）
        /// <summary>
        /// Get all addins that directly/indirectly refers to or extends the sepecified addin, which will be affected by the 
        /// status change of the later.
        /// </summary>
        internal List<AddinRecord> TryGetAllAffectingAddins(List<AddinRecord> changedAddins)
        {
            var result = TryGetAffectingAddins(changedAddins);
            if (result == null)
                return null;

            for (int i = 0; i < result.Count; i++)
                DoTryGetAffectingAddins(result, changedAddins, result[i]);

            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Get a list of addins that directly refers to or extends the sepecified addins, which will be affected by the
        /// status change of the later.
        /// </summary>
        /// <param name="changedAddins">A list of addins that should be checked for affected addins.</param>
        /// <returns></returns>
        internal List<AddinRecord> TryGetAffectingAddins(List<AddinRecord> changedAddins)
        {
            var result = new List<AddinRecord>();
            foreach (var changedAddin in changedAddins)
                DoTryGetAffectingAddins(result, changedAddins, changedAddin);
            return result.Count > 0 ? result : null;
        }

        void DoTryGetAffectingAddins(List<AddinRecord> result, List<AddinRecord> changedAddins, AddinRecord changedAddin)
        {
            // 获取当前扩展了指定插件 (changedAddin) 的现有插件
            foreach (var existingAddin in _addinRecords)
            {
                if (!existingAddin.ExtendsAddin(changedAddin.Uid))
                    continue;
                if (!result.Contains(existingAddin) && !changedAddins.Contains(existingAddin))
                    result.Add(existingAddin);
            }

            // 获取当前引用了指定插件 (changedAddin) 的程序集的其他现有插件
            if (changedAddin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in changedAddin.AssemblyFiles)
                {
                    //if (AllAddinsProvidedAssemblyAreAffected(assemblyFile, changedAddins, result))
                    //    continue; // 如果所有提供当前程序集的插件都已发生变更
                    if (!IsDefaultSelectedAddinForAssembly(assemblyFile, changedAddin))
                        continue; // 如果当前插件 (changedAddin) 并非被选为默认提供该程序集 (assemblyFile) 的插件，则不会有其他插件依赖当前插件
                    foreach (var existingAddin in _addinRecords)
                    {
                        if (!existingAddin.RefersToAssembly(assemblyFile.Uid))
                            continue;
                        if (!result.Contains(existingAddin) && !changedAddins.Contains(existingAddin))
                            result.Add(existingAddin);
                    }
                }
            }
        }

        //// whether all addins that provided the specified assembly file are in pending status.
        //bool AllAddinsProvidedAssemblyAreAffected(AssemblyFileRecord assemblyFileOfChangedAddin, List<AddinRecord> changedAddins,
        //    List<AddinRecord> affectedAddins)
        //{
        //    AddinRecordSet adns;
        //    if (!_assemblyUid2AddinSets.TryGetValue(assemblyFileOfChangedAddin.Uid, out adns))
        //        throw new InvalidOperationException();

        //    if (adns.Count == 1)
        //        return true;

        //    if (adns.Count > 1)
        //    {
        //        bool allPending = true;
        //        foreach (var adn in adns)
        //        {
        //            if (!affectedAddins.Contains(adn) && !changedAddins.Contains(adn))
        //            {
        //                allPending = false;
        //                break;
        //            }
        //        }
        //        return allPending;
        //    }
        //    return false;
        //} 
        #endregion

        #region RelationMap
        // builds and caches the relationship maps between addins
        internal void ResetRelationMaps(IEnumerable<AddinRecord> addins)
        {
            Reset();
            foreach (var addin in addins)
                AddRelationMap(addin);
        }

        internal void AddRelationMap(AddinRecord addin)
        {
            DoAddRelationMap(addin);
        }

        void DoAddRelationMap(AddinRecord addin)
        {
            //_uid2AddinRecords.Add(addin.Uid, addin);
            //_guid2AddinRecords.Add(addin.Guid, addin);
            _addinRecords.Add(addin);

            if (addin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in addin.AssemblyFiles)
                {
                    AddinRecordSet addinSet;
                    if (!_assemblyUid2AddinSets.TryGetValue(assemblyFile.Uid, out addinSet))
                    {
                        addinSet = new AddinRecordSet();
                        _assemblyUid2AddinSets.Add(assemblyFile.Uid, addinSet);
                    }
                    addinSet.Add(addin);
                }
            }

            if (addin.ExtensionPoints != null)
            {
                foreach (var extensionPoint in addin.ExtensionPoints)
                {
                    extensionPoint.AddinRecord = addin;
                    _path2ExtensionPoints.Add(extensionPoint.Path, extensionPoint);
                }
            }
        }

        internal void RemoveRelationMap(AddinRecord addin)
        {
            DoRemoveRelationMap(addin);
        }

        void DoRemoveRelationMap(AddinRecord addin)
        {
            _addinRecords.Remove(addin);
            //_guid2AddinRecords.Remove(addin.Guid);
            //_uid2AddinRecords.Remove(addin.Uid);

            if (addin.ExtensionPoints != null)
            {
                foreach (var extensionPoint in addin.ExtensionPoints)
                    _path2ExtensionPoints.Remove(extensionPoint.Path);
            }

            if (addin.AssemblyFiles != null)
            {
                foreach (var assemblyFile in addin.AssemblyFiles)
                {
                    AddinRecordSet addinSet;
                    if (!_assemblyUid2AddinSets.TryGetValue(assemblyFile.Uid, out addinSet))
                        continue;
                    if (addinSet.Remove(addin) && addinSet.Count == 0)
                        _assemblyUid2AddinSets.Remove(assemblyFile.Uid);
                }
            }
        } 
        #endregion
    }
}
