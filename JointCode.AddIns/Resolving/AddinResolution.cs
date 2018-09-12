//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;
using System;
using System.Collections.Generic;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Resolving
{
    class AddinResolutionSet : List<AddinResolution> { }

    abstract class AddinResolution
    {
        protected ResolutionStatus _resolutionStatus = ResolutionStatus.Pending;

        List<ExtensionResolution> _extensions;
        List<ExtensionBuilderResolution> _extensionBuilders;

        List<AssemblyResolutionSet> _referencedAssemblySets; // assembly sets referenced by this addin's assemblies
        List<AddinResolutionSet> _referencedAddinSets; // addin sets referenced by this addin's assemblies
        List<AddinResolution> _extendedAddins; // addins extended by this addin
        List<ExtensionPointResolution> _extendedExtensionPoints; // extension points extended by this addin

        internal AddinOperationStatus OperationStatus { get; set; }
        //internal AddinStatus Status { get; set; }
        internal bool Enabled { get; set; }
        internal ResolutionStatus ResolutionStatus { get { return _resolutionStatus; } }

        internal AddinHeaderResolution AddinHeader { get; set; }
        internal AddinId AddinId { get { return AddinHeader.AddinId; } }
        internal Guid Guid { get { return AddinId.Guid; } }
		
        internal ManifestFileResolution ManifestFile { get; set; }
        internal List<DataFileResolution> DataFiles { get; set; }
        internal List<AssemblyResolution> Assemblies { get; set; }

        internal AddinActivatorResolution AddinActivator { get; set; }

        internal List<ExtensionPointResolution> ExtensionPoints { get; set; }
        internal List<ExtensionBuilderResolutionGroup> ExtensionBuilderGroups { get; set; }
        internal List<ExtensionResolutionGroup> ExtensionGroups { get; set; }

        #region Utils
        internal List<ExtensionResolution> GetAllExtensions()
        {
            if (_extensions != null)
                return _extensions;

            if (ExtensionGroups == null)
                return null;

            _extensions = new List<ExtensionResolution>();
            foreach (var exGroup in ExtensionGroups)
                _extensions.AddRange(exGroup.Children);

            for (int i = 0; i < _extensions.Count; i++)
            {
                var extension = _extensions[i];
                if (extension.Children != null)
                    _extensions.AddRange(extension.Children);
            }

            return _extensions;
        }

        internal List<ExtensionBuilderResolution> GetAllExtensionBuilders()
        {
            if (_extensionBuilders != null)
                return _extensionBuilders;

            if (ExtensionPoints == null && ExtensionBuilderGroups == null)
                return null;

            _extensionBuilders = new List<ExtensionBuilderResolution>();

            if (ExtensionPoints != null)
            {
                foreach (var extensionPoint in ExtensionPoints)
                {
                    if (extensionPoint.Children != null)
                        _extensionBuilders.AddRange(extensionPoint.Children);
                }

                for (int i = 0; i < _extensionBuilders.Count; i++)
                {
                    var extensionBuilder = _extensionBuilders[i];
                    if (extensionBuilder.Children != null)
                        _extensionBuilders.AddRange(extensionBuilder.Children);
                }
            }

            if (ExtensionBuilderGroups != null)
            {
                var startIndex = _extensionBuilders.Count;
                foreach (var exBuilderGroup in ExtensionBuilderGroups)
                    _extensionBuilders.AddRange(exBuilderGroup.Children);

                for (int i = startIndex; i < _extensionBuilders.Count; i++)
                {
                    var extensionBuilder = _extensionBuilders[i];
                    if (extensionBuilder.Children != null)
                        _extensionBuilders.AddRange(extensionBuilder.Children);
                }
            }

            return _extensionBuilders;
        } 
        #endregion
        
        #region Dependences
        //internal List<AssemblyResolution> ReferencedAssemblies { get { return _referencedAssemblies; } }
        internal List<AssemblyResolutionSet> ReferencedAssemblySets { get { return _referencedAssemblySets; } }
        internal List<AddinResolution> ExtendedAddins { get { return _extendedAddins; } }
        internal List<ExtensionPointResolution> ExtendedExtensionPoints { get { return _extendedExtensionPoints; } }

        /// <summary>
        /// Adds an assembly set that is referenced by the assemblies of this addin.
        /// </summary>
        /// <param name="referencedAssemblySet">The referenced assembly set.</param>
        internal void AddReferencedAssemblySet(AssemblyResolutionSet referencedAssemblySet)
        {
            // if the referenced assembly is provided by this addin itself, don't bother to resolve it at the runtime.
            foreach (var referencedAssembly in referencedAssemblySet)
            {
                if (ReferenceEquals(referencedAssembly.DeclaringAddin, this))
                    return;
            }

            _referencedAssemblySets = _referencedAssemblySets ?? new List<AssemblyResolutionSet>();
            if (_referencedAssemblySets.Contains(referencedAssemblySet))
                return;
            _referencedAssemblySets.Add(referencedAssemblySet);

            var addinSet = new AddinResolutionSet();

            if (Enabled)
            {
                // 如果插件初始状态定义为 Enabled，那么只要它引用的任何一个插件是 Enabled 的，插件最终解析状态就是 Enabled 的
                var shouldEnable = false;
                foreach (var referencedAssembly in referencedAssemblySet)
                {
                    addinSet.Add(referencedAssembly.DeclaringAddin);
                    if (referencedAssembly.DeclaringAddin.Enabled)
                        shouldEnable = true;
                }
                Enabled = shouldEnable;
            }
            else
            {
                foreach (var referencedAssembly in referencedAssemblySet)
                    addinSet.Add(referencedAssembly.DeclaringAddin);
            }
            
            _referencedAddinSets = _referencedAddinSets ?? new List<AddinResolutionSet>();
            _referencedAddinSets.Add(addinSet);
        }

        /// <summary>
        /// Adds an addin that is extended by this addin.
        /// </summary>
        /// <param name="extendedAddin">The extended addin.</param>
        internal void AddExtendedAddin(AddinResolution extendedAddin)
        {
            if (ReferenceEquals(extendedAddin, this))
                return;

            if (!extendedAddin.Enabled)
                Enabled = false;

            _extendedAddins = _extendedAddins ?? new List<AddinResolution>();
            if (!_extendedAddins.Contains(extendedAddin))
                _extendedAddins.Add(extendedAddin);
        }

        // todo: add extension points extended by the extension builders of this addin to the list
        /// <summary>
        /// Adds an extension point that is extended by this addin, including extension points that this addin provides extension builder or extensions to extend it.
        /// </summary>
        /// <param name="extendedExtensionPoint">The extended extension point.</param>
        internal void AddExtendedExtensionPoint(ExtensionPointResolution extendedExtensionPoint)
        {
            if (ReferenceEquals(extendedExtensionPoint.DeclaringAddin, this))
                return;

            if (!extendedExtensionPoint.DeclaringAddin.Enabled)
                Enabled = false;

            _extendedExtensionPoints = _extendedExtensionPoints ?? new List<ExtensionPointResolution>();
            if (!_extendedExtensionPoints.Contains(extendedExtensionPoint))
                _extendedExtensionPoints.Add(extendedExtensionPoint);
        }
        #endregion

        #region Resolve
        internal ResolutionStatus Resolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (_resolutionStatus != ResolutionStatus.Pending)
                return _resolutionStatus;

            if (AddinActivator != null)
            {
                var resolutionStatus = AddinActivator.Resolve(resolutionResult, convertionManager, ctx);
                if (resolutionStatus == ResolutionStatus.Failed)
                {
                    _resolutionStatus = ResolutionStatus.Failed;
                    return _resolutionStatus;
                }
            }

            if (ExtensionPoints != null)
            {
                foreach (var extensionPoint in ExtensionPoints)
                {
                    var resolutionStatus = extensionPoint.Resolve(resolutionResult, convertionManager, ctx);
                    if (resolutionStatus == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionStatus == ResolutionStatus.Failed)
                    {
                        _resolutionStatus = ResolutionStatus.Failed;
                        return _resolutionStatus;
                    }
                    else
                    { return ResolutionStatus.Pending; }
                }
            }

            var extensionBuilders = GetAllExtensionBuilders();
            if (extensionBuilders != null)
            {
                foreach (var extensionBuilder in extensionBuilders)
                {
                    var resolutionStatus = extensionBuilder.Resolve(resolutionResult, convertionManager, ctx);
                    if (resolutionStatus == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionStatus == ResolutionStatus.Failed)
                    {
                        _resolutionStatus = ResolutionStatus.Failed;
                        return _resolutionStatus;
                    }
                    else
                    { return ResolutionStatus.Pending; }
                }
            }

            var extensions = GetAllExtensions();
            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    var resolutionStatus = extension.Resolve(resolutionResult, convertionManager, ctx);
                    if (resolutionStatus == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionStatus == ResolutionStatus.Failed)
                    {
                        _resolutionStatus = ResolutionStatus.Failed;
                        return _resolutionStatus;
                    }
                    else
                    { return ResolutionStatus.Pending; }
                }
            }

            if (_referencedAssemblySets != null)
            {
                foreach (var referencedAssemblySet in _referencedAssemblySets)
                {
                    var resolutionStatus = referencedAssemblySet.Resolve(resolutionResult, convertionManager, ctx);
                    if (resolutionStatus == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionStatus == ResolutionStatus.Failed)
                    {
                        _resolutionStatus = ResolutionStatus.Failed;
                        return _resolutionStatus;
                    }
                    else
                    { return ResolutionStatus.Pending; }
                }
            }

            _resolutionStatus = ResolutionStatus.Success;
            return _resolutionStatus;
        }

        // @return: if the given addin is removed, does this addin still resolve successfully?
        internal bool CanResolveWithout(AddinResolution addin)
        {
            if (_extendedAddins != null && _extendedAddins.Contains(addin))
                return false;

            if (_referencedAddinSets != null)
            {
                for (int i = _referencedAddinSets.Count - 1; i >= 0; i--)
                {
                    var referencedAddinSet = _referencedAddinSets[i];
                    if (!referencedAddinSet.Remove(addin))
                        continue;
                    if (referencedAddinSet.Count == 0)
                        return false;

                    var referencedAssemblySet = _referencedAssemblySets[i];
                    for (int j = referencedAssemblySet.Count - 1; j >= 0; j--)
                    {
                        if (ReferenceEquals(referencedAssemblySet[i].DeclaringAddin, addin))
                            referencedAssemblySet.RemoveAt(j);
                    }
                }
            }

            return true;
        }
        #endregion

        internal abstract AddinRecord ToRecord();

        internal static AddinFilePack ToAddinFilePack(AddinResolution adnResolution)
        {
            var result = new AddinFilePack { ManifestFile = ToPersistentObject(adnResolution.ManifestFile) };
            if (adnResolution.Assemblies != null)
            {
                foreach (var asmResolution in adnResolution.Assemblies)
                    result.AddAssemblyFile(ToPersistentObject(asmResolution.AssemblyFile));
            }
            if (adnResolution.DataFiles != null)
            {
                foreach (var dfResolution in adnResolution.DataFiles)
                    result.AddDataFile(ToPersistentObject(dfResolution));
            }
            return result;
        }

        static ManifestFileRecord ToPersistentObject(ManifestFileResolution item)
        {
            return new ManifestFileRecord { FilePath = item.FilePath, Directory = item.Directory, FileHash = item.FileHash, LastWriteTime = item.LastWriteTime, FileLength = item.FileLength };
        }

        static DataFileRecord ToPersistentObject(DataFileResolution item)
        {
            return new DataFileRecord { FilePath = item.FilePath };
        }

        static AssemblyFileRecord ToPersistentObject(AssemblyFileResolution item)
        {
            if (item.Uid == UidStorage.InvalidAssemblyUid)
                item.Uid = UidStorage.GetNextAssemblyUid();
            return new AssemblyFileRecord { FilePath = item.FilePath, LastWriteTime = item.LastWriteTime, Uid = item.Uid };
        }
    }

    // new or updated addin
    // for this kind of addins, everything need to be resolved.
    class NewOrUpdatedAddinResolution : AddinResolution
    {
        internal NewOrUpdatedAddinResolution() { OperationStatus = AddinOperationStatus.NewOrUpdated; }

        internal override AddinRecord ToRecord() { return ToPersistentObject(this); }

        #region ToPersistentObject
        static AddinRecord ToPersistentObject(AddinResolution adnResolution)
        {
            if (adnResolution.AddinId.Uid == UidStorage.InvalidAddinUid)
                adnResolution.AddinId.Uid = UidStorage.GetNextAddinUid();

            var addinFilePack = ToAddinFilePack(adnResolution);
            var addinHeader = ToPersistentObject(adnResolution.AddinHeader);
            var addinActivator = adnResolution.AddinActivator == null ? null : adnResolution.AddinActivator.ToRecord();
            var addinRecord = new AddinRecord(addinHeader, addinFilePack, addinActivator) { Enabled = adnResolution.Enabled };

            if (adnResolution.ReferencedAssemblySets != null)
            {
                foreach (var referencedAssemblySet in adnResolution.ReferencedAssemblySets)
                    addinRecord.AddReferencedAssembly(new ReferencedAssemblyRecord
                    {
                        Uid = referencedAssemblySet.Uid,
                        Version = referencedAssemblySet.Version
                    });
            }

            if (adnResolution.ExtendedAddins != null)
            {
                foreach (var extendedAddin in adnResolution.ExtendedAddins)
                    addinRecord.AddExtendedAddin(new ExtendedAddinRecord
                    {
                        Uid = extendedAddin.AddinId.Uid,
                        Version = extendedAddin.AddinHeader.Version
                    });
            }

            if (adnResolution.ExtendedExtensionPoints != null)
            {
                foreach (var extendedExtensionPoint in adnResolution.ExtendedExtensionPoints)
                    addinRecord.AddExtendedExtensionPoint(extendedExtensionPoint.Uid);
            }

            if (adnResolution.ExtensionPoints != null)
            {
                foreach (var extensionPoint in adnResolution.ExtensionPoints)
                {
                    var epRecord = extensionPoint.ToRecord();
                    addinRecord.AddExtensionPoint(epRecord);
                }
            }

            if (adnResolution.ExtensionBuilderGroups != null)
            {
                foreach (var extensionBuilderGroup in adnResolution.ExtensionBuilderGroups)
                    addinRecord.AddExtensionBuilderGroup(extensionBuilderGroup.ToRecord());
            }

            if (adnResolution.ExtensionGroups != null)
            {
                foreach (var extensionGroup in adnResolution.ExtensionGroups)
                    addinRecord.AddExtensionGroup(extensionGroup.ToRecord());
            }

            return addinRecord;
        }

        static AddinHeaderRecord ToPersistentObject(AddinHeaderResolution item)
        {
            if (item.AddinId.Uid == UidStorage.InvalidAddinUid)
                item.AddinId.Uid = UidStorage.GetNextAddinUid();
            return new AddinHeaderRecord
            {
                AddinId = item.AddinId,
                AddinCategory = item.AddinCategory,
                CompatVersion = item.CompatVersion,
                Version = item.Version,
                //Enabled = item.Enabled,
                Name = item.Name,
                //Url = item.Url,
                Description = item.Description,
                InnerProperties = item.InnerProperties
            };
        }
        #endregion
    }

    // directly affected addin
    // for this kind of addins, everything need to be resolved.
    class DirectlyAffectedAddinResolution : NewOrUpdatedAddinResolution
    {
        internal DirectlyAffectedAddinResolution() { OperationStatus = AddinOperationStatus.DirectlyAffected; }
    }

    // indirectly affected addin
    // for an indirectly affected addin, just find the depended / referenced addins, and make sure they get resolved.
    class IndirectlyAffectedAddinResolution : AddinResolution
    {
        readonly AddinRecord _old;

        internal IndirectlyAffectedAddinResolution(AddinRecord old)
        {
            _old = old;
            OperationStatus = AddinOperationStatus.IndirectlyAffected;
        }

        internal override AddinRecord ToRecord()
        {
            return _old;
        }
    }

    class UnaffectedAddinResolution : AddinResolution
    {
        readonly AddinRecord _old;

        internal UnaffectedAddinResolution(AddinRecord old)
        {
            _old = old;
            OperationStatus = AddinOperationStatus.Unaffected;
        }

        internal override AddinRecord ToRecord()
        {
            return _old;
        }
    }
}