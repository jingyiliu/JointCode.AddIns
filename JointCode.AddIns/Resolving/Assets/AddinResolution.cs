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
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving.Assets
{
    class AddinResolutionSet : List<AddinResolution> { }

    abstract class AddinResolution
    {
        ResolutionStatus _resolutionStatus = ResolutionStatus.Pending;

        List<ExtensionResolution> _extensions;
        List<ExtensionBuilderResolution> _extensionBuilders;

        List<AssemblyResolutionSet> _referencedAssemblySets; // assembly sets referenced by this addin's assemblies
        List<AddinResolutionSet> _referencedAddinSets; // addin sets referenced by this addin's assemblies
        List<AddinResolution> _extendedAddins; // addins extended by this addin
        List<ExtensionPointResolution> _extendedExtensionPoints; // extension points extended by this addin

        internal AddinOperationStatus OperationStatus { get; set; }
        internal AddinRunningStatus RunningStatus { get; set; }
        internal ResolutionStatus ResolutionStatus { get { return _resolutionStatus; } }

        internal AddinHeaderResolution AddinHeader { get; set; }
        internal AddinId AddinId { get { return AddinHeader.AddinId; } }
        internal Guid Guid { get { return AddinHeader.AddinId.Guid; } }
		
        internal ManifestFileResolution ManifestFile { get; set; }
        internal List<DataFileResolution> DataFiles { get; set; }
        internal List<AssemblyResolution> Assemblies { get; set; }
		
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

            if (RunningStatus == AddinRunningStatus.Enabled)
            {
                var shouldEnable = false;
                foreach (var referencedAssembly in referencedAssemblySet)
                {
                    addinSet.Add(referencedAssembly.DeclaringAddin);
                    if (referencedAssembly.DeclaringAddin.RunningStatus == AddinRunningStatus.Enabled)
                        shouldEnable = true;
                }
                RunningStatus = shouldEnable ? AddinRunningStatus.Enabled : AddinRunningStatus.Disabled;
            }
            else
            {
                foreach (var referencedAssembly in referencedAssemblySet)
                {
                    addinSet.Add(referencedAssembly.DeclaringAddin);
                    if (RunningStatus == AddinRunningStatus.Enabled && referencedAssembly.DeclaringAddin.RunningStatus == AddinRunningStatus.Disabled)
                        RunningStatus = AddinRunningStatus.Disabled;
                }
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

            if (RunningStatus == AddinRunningStatus.Enabled && extendedAddin.RunningStatus == AddinRunningStatus.Disabled)
                RunningStatus = AddinRunningStatus.Disabled;

            _extendedAddins = _extendedAddins ?? new List<AddinResolution>();
            if (!_extendedAddins.Contains(extendedAddin))
                _extendedAddins.Add(extendedAddin);
        }

        /// <summary>
        /// Adds an extension point that is extended by this addin.
        /// </summary>
        /// <param name="extendedExtensionPoint">The extended extension point.</param>
        internal void AddExtendedExtensionPoint(ExtensionPointResolution extendedExtensionPoint)
        {
            if (ReferenceEquals(extendedExtensionPoint.DeclaringAddin, this))
                return;

            if (RunningStatus == AddinRunningStatus.Enabled && extendedExtensionPoint.DeclaringAddin.RunningStatus == AddinRunningStatus.Disabled)
                RunningStatus = AddinRunningStatus.Disabled;

            _extendedExtensionPoints = _extendedExtensionPoints ?? new List<ExtensionPointResolution>();
            if (!_extendedExtensionPoints.Contains(extendedExtensionPoint))
                _extendedExtensionPoints.Add(extendedExtensionPoint);
        }
        #endregion

        #region Resolve
        internal ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (_resolutionStatus != ResolutionStatus.Pending)
                return _resolutionStatus;

            if (ExtensionPoints != null)
            {
                foreach (var extensionPoint in ExtensionPoints)
                {
                    var resolutionResult = extensionPoint.Resolve(dialog, convertionManager, ctx);
                    if (resolutionResult == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionResult == ResolutionStatus.Failed)
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
                    var resolutionResult = extensionBuilder.Resolve(dialog, convertionManager, ctx);
                    if (resolutionResult == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionResult == ResolutionStatus.Failed)
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
                    var resolutionResult = extension.Resolve(dialog, convertionManager, ctx);
                    if (resolutionResult == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionResult == ResolutionStatus.Failed)
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
                    var resolutionResult = referencedAssemblySet.Resolve(dialog, convertionManager, ctx);
                    if (resolutionResult == ResolutionStatus.Success)
                    { continue; }
                    else if (resolutionResult == ResolutionStatus.Failed)
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
    }

    // new or updated addin
    // for this kind of addins, everything need to be resolved.
    class NewAddinResolution : AddinResolution
    {
        internal NewAddinResolution() { OperationStatus = AddinOperationStatus.New; }

        internal override AddinRecord ToRecord() { return ToPersistentObject(this); }

        #region ToPersistentObject
        static AddinRecord ToPersistentObject(AddinResolution adnResolution)
        {
            var addinFilePack = ToAddinFilePack(adnResolution);
            var addinIndexRecord = new AddinIndexRecord(addinFilePack)
            {
                RunningStatus = adnResolution.RunningStatus,
                AddinHeader = ToPersistentObject(adnResolution.AddinHeader),
            };
            var addinBodyRecord = new AddinBodyRecord(adnResolution.Guid);

            if (adnResolution.ReferencedAssemblySets != null)
            {
                foreach (var referencedAssemblySet in adnResolution.ReferencedAssemblySets)
                    addinIndexRecord.AddReferencedAssembly(new ReferencedAssemblyRecord
                    {
                        Uid = referencedAssemblySet.Uid,
                        Version = referencedAssemblySet.Version
                    });
            }

            if (adnResolution.ExtendedAddins != null)
            {
                foreach (var extendedAddin in adnResolution.ExtendedAddins)
                    addinIndexRecord.AddExtendedAddin(new ExtendedAddinRecord
                    {
                        Uid = extendedAddin.AddinId.Uid,
                        Version = extendedAddin.AddinHeader.Version
                    });
            }

            if (adnResolution.ExtendedExtensionPoints != null)
            {
                foreach (var extendedExtensionPoint in adnResolution.ExtendedExtensionPoints)
                    addinIndexRecord.AddExtendedExtensionPoint(extendedExtensionPoint.Uid);
            }

            if (adnResolution.ExtensionPoints != null)
            {
                foreach (var extensionPoint in adnResolution.ExtensionPoints)
                {
                    //if (extensionPoint.Children == null)
                    //    continue;
                    var epRecord = extensionPoint.ToRecord();
                    addinIndexRecord.AddExtensionPoint(ToBaseObject(epRecord));
                    addinBodyRecord.AddExtensionPoint(epRecord);
                }
            }

            if (adnResolution.ExtensionBuilderGroups != null)
            {
                foreach (var extensionBuilderGroup in adnResolution.ExtensionBuilderGroups)
                    addinBodyRecord.AddExtensionBuilderGroup(extensionBuilderGroup.ToRecord());
            }

            if (adnResolution.ExtensionGroups != null)
            {
                foreach (var extensionGroup in adnResolution.ExtensionGroups)
                    addinBodyRecord.AddExtensionGroup(extensionGroup.ToRecord());
            }

            return new AddinRecord { AddinIndex = addinIndexRecord, AddinBody = addinBodyRecord };
        }

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

        static AddinHeaderRecord ToPersistentObject(AddinHeaderResolution item)
        {
            if (item.AddinId.Uid == UidProvider.InvalidAddinUid)
                item.AddinId.Uid = IndexManager.GetNextAddinUid();
            return new AddinHeaderRecord
            {
                AddinId = item.AddinId,
                AddinCategory = item.AddinCategory,
                CompatVersion = item.CompatVersion,
                Version = item.Version,
                Enabled = item.Enabled,
                FriendName = item.FriendName,
                Properties = item.Properties,
                Url = item.Url,
                Description = item.Description
            };
        }

        static ManifestFileRecord ToPersistentObject(ManifestFileResolution item)
        {
            return new ManifestFileRecord { FilePath = item.FilePath, Directory = item.Directory, FileHash = item.FileHash, LastWriteTime = item.LastWriteTime };
        }

        static DataFileRecord ToPersistentObject(DataFileResolution item)
        {
            return new DataFileRecord { FilePath = item.FilePath };
        }

        static AssemblyFileRecord ToPersistentObject(AssemblyFileResolution item)
        {
            if (item.Uid == UidProvider.InvalidAssemblyUid)
                item.Uid = IndexManager.GetNextAssemblyUid();
            return new AssemblyFileRecord { FilePath = item.FilePath, LastWriteTime = item.LastWriteTime, Uid = item.Uid };
        }

        static BaseExtensionPointRecord ToBaseObject(ExtensionPointRecord item)
        {
            return new BaseExtensionPointRecord { Id = item.Id, Uid = item.Uid };
        }
        #endregion
    }

    // directly affected addin
    // for this kind of addins, everything need to be resolved.
    class DirectlyAffectedAddinResolution : NewAddinResolution
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
        internal UnaffectedAddinResolution() { OperationStatus = AddinOperationStatus.Unaffected; }

        internal override AddinRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    }
}