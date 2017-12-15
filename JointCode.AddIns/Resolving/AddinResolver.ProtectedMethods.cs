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
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving
{
    //messenger.ReportError(string.Format("Could not find the ExtensionTemplate with TemplatePath [{0}]!", extOperand.TemplatePath));
    //messenger.ReportError(string.Format("An extension with the Id [{0}] already exists under the parent path [{1}]!", extOperand.Id, extOperand.ParentPath));
    //messenger.ReportError(string.Format("The Id [{0}] has been already used by another ExtensionTemplate!", etOperand.Id));
    //messenger.ReportError(string.Format("An ExtensionPoint with the Id [{0}] already exists!", epOperand.Id));

    //messenger.ReportError(string.Format("The extension [{0}] is not a container extension. Could not add an extension to a non-container extension!", extOperand.ParentPath));
    //messenger.ReportError(string.Format("The extension [{0}] does not match its parent extension [{1}]!", extOperand.Path, extOperand.ParentPath));
    //messenger.ReportError(string.Format("Could not find the parent extension [{0}] for extension [{0}]!", extOperand.ParentPath, extOperand.Path));
    //messenger.ReportError(string.Format("The extension [{0}] does not match the ExtensionPoint [{1}]!", extOperand.Path, extOperand.ParentPath));
    //messenger.ReportError(string.Format("Could not find the ExtensionPoint [{0}] for extension [{1}]!", extOperand.ParentPath, extOperand.Path));
    //messenger.ReportError(string.Format("The extension [{0}] does not match its parent extension [{1}]!", extOperand.GetPath(_currentParentPath), _currentParentPath));
    //messenger.ReportError(string.Format("A required property which has been marked by ExtensionDataAttribute in the ExtensionTemplate with the name [{0}] and type [{1}] does not provided!", etMeta.Key, etMeta.Value.ToString()));

    partial class AddinResolver
    {
        // this method should split the existing addins into the following kind: 
        // 1. updated addins
        // 2. unaffected addins
        // 3. directly affected addins
        // 4. indirectly affected addins
        // then, decide how to register assets of these addins and whether they need resolution, according to each kind.
        // and finally, return the addin list that need to be resolved.
        protected List<AddinResolution> RegisterExistingAssets(IMessageDialog dialog, ResolutionContext ctx, AddinCollision addinCollision, 
            List<AddinResolution> adnResolutions)
        {
            // check whether there are updated addins.
            // and if there are any, mark their operation status as updated.
            List<AddinIndexRecord> updatedAddins = null;
            for (int i = 0; i < _indexManager.AddinCount; i++)
            {
                var addin = _indexManager.GetAddin(i);
                var addinId = addin.AddinId;
                AddinResolution adnResolution;

                if (!ctx.TryGetAddin(addinId, out adnResolution))
                    continue;

                updatedAddins = updatedAddins ?? new List<AddinIndexRecord>();
                updatedAddins.Add(addin);
                addin.OperationStatus = AddinOperationStatus.Updated;
            }

            // mark directly affected and indirectly affected addins.
            if (updatedAddins != null)
            {
                var directlyAffectedAddins = _indexManager.TryGetAffectedAddins(updatedAddins);
                if (directlyAffectedAddins != null)
                {
                    foreach (var directlyAffectedAddin in directlyAffectedAddins)
                    {
                        if (directlyAffectedAddin.OperationStatus != AddinOperationStatus.Updated)
                            directlyAffectedAddin.OperationStatus = AddinOperationStatus.DirectlyAffected;
                    }
                    var indirectlyAffectedAddins = _indexManager.TryGetAllAffectedAddins(directlyAffectedAddins);
                    if (indirectlyAffectedAddins != null)
                    {
                        foreach (var indirectlyAffectedAddin in indirectlyAffectedAddins)
                        {
                            if (indirectlyAffectedAddin.OperationStatus == AddinOperationStatus.Unaffected)
                                indirectlyAffectedAddin.OperationStatus = AddinOperationStatus.IndirectlyAffected;
                        }
                    }
                }
            }

            List<AddinResolution> resolableAddins = null;
            // decide how to register assets of these addins and whether to resolve these addins according to their operation status.
            for (int i = _indexManager.AddinCount - 1; i >= 0; i--)
            {
                var addin = _indexManager.GetAddin(i);
                var resolvableAddin = DoRegisterExistingAddin(dialog, ctx, addinCollision, addin, adnResolutions);
                if (resolvableAddin != null)
                {
                    resolableAddins = resolableAddins ?? new AddinResolutionSet();
                    resolableAddins.Add(resolvableAddin);
                }
            }

            return resolableAddins;
        }

        AddinResolution DoRegisterExistingAddin(IMessageDialog dialog, ResolutionContext ctx, AddinCollision addinCollision, 
            AddinIndexRecord existingAddin, List<AddinResolution> adnResolutions)
        {
            AddinBodyRecord addinBody;
            if (!_bodyRepo.TryGet(existingAddin.Guid, out addinBody))
                throw new InconsistentStateException();

            _indexManager.RemoveAddin(existingAddin);
            _bodyRepo.Remove(addinBody);

            if (existingAddin.OperationStatus == AddinOperationStatus.Updated)
            {
                var ebs = addinBody.GetAllExtensionBuilders();
                if (ebs != null)
                {
                    foreach (var eb in ebs)
                    {
                        if (eb.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
                            ctx.RegisterExtensionBuilderPath(eb.Uid, eb.GetPath());
                    }
                }
                return null;
            }

            var adnResolution = FromPersistentObject(new AddinRecord { AddinBody = addinBody, AddinIndex = existingAddin });
            TryRegisterAddin(dialog, ctx, adnResolution, addinCollision);
            DoRegisterExistingAssets(dialog, ctx, adnResolution);
            // if the operation status of an addin not equals to unaffected (i.e, directly/indirectly affected addin), it need to 
            // be resolved, so we add it to the addin resolution list.
            return existingAddin.OperationStatus == AddinOperationStatus.Unaffected ? null : adnResolution;
        }

        void DoRegisterExistingAssets(IMessageDialog dialog, ResolutionContext ctx, AddinResolution adnResolution)
        {
            if (adnResolution.Assemblies != null)
            {
                foreach (var asm in adnResolution.Assemblies)
                    ctx.RegisterAssembly(dialog, asm);
            }
            if (adnResolution.ExtensionPoints != null)
            {
                foreach (var ep in adnResolution.ExtensionPoints)
                    ctx.RegisterExtensionPoint(ep);
            }
            // get all extension builders defined under extension point and extension builder set
            var extensionBuilders = adnResolution.GetAllExtensionBuilders();
            if (extensionBuilders != null)
            {
                foreach (var eb in extensionBuilders)
                {
                    if (eb.ExtensionBuilderKind != ExtensionBuilderKind.Referenced)
                        ctx.RegisterExtensionBuilder(eb);
                }
            }
            var extensions = adnResolution.GetAllExtensions();
            if (extensions != null)
            {
                foreach (var ex in extensions)
                    ctx.RegisterExtension(ex);
            }
        }

        protected bool TryRegisterAddin(IMessageDialog dialog, ResolutionContext ctx, AddinResolution adnResolution,
            AddinCollision addinCollision)
        {
            AddinResolution existingAddin;
            if (ctx.TryRegisterAddin(dialog, adnResolution.AddinId, adnResolution, out existingAddin))
                return true;
            var key = new AddinCollisionKey(existingAddin.Guid);
            addinCollision.Add(key, adnResolution, existingAddin);
            return false;
        }

        void UnregisterAddin(ResolutionContext ctx, AddinResolution adnResolution)
        {
            ctx.UnregisterAddin(adnResolution.AddinId);
        }

        // @return: whether there is collisions.
        protected bool TryRegisterAssets(IMessageDialog dialog, ResolutionContext ctx, AddinResolution adnResolution,
           AddinCollision addinCollision)
        {
            var success = true;
            
            if (adnResolution.Assemblies != null)
            {
            	foreach (var asm in adnResolution.Assemblies) 
            		ctx.TryRegisterAssembly(dialog, asm);
            }

            if (adnResolution.ExtensionPoints != null) 
            {
                foreach (var extensionPoint in adnResolution.ExtensionPoints)
                {
                    ExtensionPointResolution existingExtensionPoint;
                    if (!ctx.TryRegisterExtensionPoint(dialog, extensionPoint, out existingExtensionPoint))
                    {
                        var key = new ExtensionPointCollisionKey(existingExtensionPoint.Id);
                        addinCollision.Add(key, adnResolution, existingExtensionPoint.DeclaringAddin);
                        success = false;
                    }
                }
            }

            // get all extension builders defined under extension point and extension builder set
            var extensionBuilders = adnResolution.GetAllExtensionBuilders();
            if (extensionBuilders != null)
            {
                foreach (var extensionBuilder in extensionBuilders)
                {
                    if (extensionBuilder.ExtensionBuilderKind == ExtensionBuilderKind.Referenced)
                        continue;
                    ExtensionBuilderResolution existingExtensionBuilder;
                    if (!ctx.TryRegisterExtensionBuilder(dialog, extensionBuilder, out existingExtensionBuilder))
                    {
                        var key = new ExtensionBuilderCollisionKey(existingExtensionBuilder.Id);
                        addinCollision.Add(key, adnResolution, existingExtensionBuilder.DeclaringAddin);
                        success = false;
                    }
                }
            }

            var extensions = adnResolution.GetAllExtensions();
            if (extensions != null) 
            {
                foreach (var extension in extensions)
                {
                    ExtensionResolution existingExtension;
                    if (!ctx.TryRegisterExtension(dialog, extension, out existingExtension))
                    {
                        var key = new ExtensionCollisionKey(existingExtension.Head.Path);
                        addinCollision.Add(key, adnResolution, existingExtension.DeclaringAddin);
                        success = false;
                    }
                }
            }
            
            return success;
        }

        void UnregisterAssets(ResolutionContext ctx, AddinResolution adnResolution)
        {
            var extensions = adnResolution.GetAllExtensions();
            if (extensions != null) 
            {
                foreach (var extension in extensions)
                    ctx.UnregisterExtension(extension);
            }

            var extensionBuilders = adnResolution.GetAllExtensionBuilders();
            if (extensionBuilders != null) 
            {
                foreach (var extensionBuilder in extensionBuilders)
                {
                    if (extensionBuilder.ExtensionBuilderKind != ExtensionBuilderKind.Referenced)
                        ctx.UnregisterExtensionBuilder(extensionBuilder);
                }
            }
            
            if (adnResolution.ExtensionPoints != null) 
            {
                foreach (var extensionPoint in adnResolution.ExtensionPoints)
                    ctx.UnregisterExtensionPoint(extensionPoint);
            }

            if (adnResolution.Assemblies != null) 
            {
            	foreach (var asm in adnResolution.Assemblies) 
            		ctx.UnregisterAssembly(asm);
            }
        }
    }

    partial class AddinResolver
    {
        protected List<AddinResolution> TryParseAddins(IMessageDialog dialog, IEnumerable<FilePack> filePacks)
        {
            var result = new List<AddinResolution>();
            foreach (var filePack in filePacks)
            {
                foreach (var addinParser in _addinParsers)
                {
                    AddinResolution adnResolution;
                    if (addinParser.TryParse(dialog, filePack, out adnResolution))
                    {
                        result.Add(adnResolution);
                        break;
                    }
                }
            }
            return result.Count > 0 ? result : null;
        }

        //// 确保新增插件和更新插件的依赖关系都存在，以准备将插件数据保存到持久化文件中。
        //// 同时按照依赖关系对新增插件进行排序（形成插件树的关系。例如，假设有 B 和 C 依赖 A，同时 D 依赖 B，则顺序为 A->B->C-D）。
        //// 防止元数据相互依赖
        // 1. an addin (a) references assemblies provided by another addin (b), but the addin (b) provides an extension for addin (a).
        // 2. an addin (a) references assemblies provided by another addin (b), and the addin (b) references assemblies provided by the addin (a).
        // 3. an addin (a) provides an extension for another addin (b), and the addin (b) provides an extension for addin (a).
        // no addins with [circular references] or [circular dependencies] will be returned.
        protected List<AddinResolution> TryResolveAddins(IMessageDialog dialog, ConvertionManager convertionManager, 
            ResolutionContext ctx, List<AddinResolution> adnResolutions)
        {
            var result = new List<AddinResolution>();
            int loop = 0, retryTimes = 0;
            while (true)
            {
                var idx = loop % adnResolutions.Count;
                var adnResolution = adnResolutions[idx];
                var resolutionStatus = adnResolution.Resolve(dialog, convertionManager, ctx);
                if (resolutionStatus.IsFailed())
                {
                    retryTimes = 0;
                    RemoveInvalidAddinsRecursively(ctx, adnResolutions, adnResolution);
                    dialog.AddError("");
                }
                else if (resolutionStatus.IsPending()) // refers to other addins that has not been resolved
                {
                    retryTimes += 1;
                }
                else if (resolutionStatus.IsSuccess())
                {
                    retryTimes = 0;
                    adnResolutions.RemoveAt(idx);
                    result.Add(adnResolution);
                }
                loop += 1;
                if (adnResolutions.Count == 0 || retryTimes > adnResolutions.Count)
                    break;
            }
            return result; 
        }

        protected void TrimConflictingAddins(AddinCollision addinCollision, List<AddinResolution> adnResolutions)
        {
            addinCollision.Trim();
            if (addinCollision.Count == 0)
                return;

            foreach (var addins in addinCollision.Items)
            {
                // try to find whether there is any existing addin.
                // if an colliding addin is an existing addin, remove the others.
                // otherwise, simplely keep the top addin (index equals to 0) in the list.
                var keptIndex = 0; // the index of addin to be kept.
                for (int i = 0; i < addins.Count; i++)
                {
                    var addin = addins[i];
                    if (addin.OperationStatus == AddinOperationStatus.Unaffected)
                    {
                        keptIndex = i;
                        break;
                    }
                }
                for (int i = 0; i < addins.Count; i++)
                {
                    if (addins.Count == 0)
                        break;
                    if (i == keptIndex)
                        continue;
                    RemoveInvalidAddinsRecursively2(adnResolutions, adnResolutions[i]);
                }
            }
        }

        // remove the @adnResolution and all addins depended on it from the @adnResolutions list recursively.
        void RemoveInvalidAddinsRecursively(ResolutionContext ctx, List<AddinResolution> adnResolutions, AddinResolution adnResolution)
        {
            if (!adnResolutions.Remove(adnResolution))
                return;
            UnregisterAssets(ctx, adnResolution);
            UnregisterAddin(ctx, adnResolution);
            _indexManager.AddInvalidAddinFilePack(NewAddinResolution.ToAddinFilePack(adnResolution));

            for (int i = 0; i < adnResolutions.Count; i++)
            {
                var otherResolution = adnResolutions[i];
                if (!otherResolution.CanResolveWithout(adnResolution))
                    RemoveInvalidAddinsRecursively(ctx, adnResolutions, otherResolution);
            }
        }

        void RemoveInvalidAddinsRecursively2(List<AddinResolution> adnResolutions, AddinResolution adnResolution)
        {
            if (!adnResolutions.Remove(adnResolution))
                return;
            _indexManager.AddInvalidAddinFilePack(NewAddinResolution.ToAddinFilePack(adnResolution));

            for (int i = 0; i < adnResolutions.Count; i++)
            {
                var otherResolution = adnResolutions[i];
                if (!otherResolution.CanResolveWithout(adnResolution))
                    RemoveInvalidAddinsRecursively2(adnResolutions, otherResolution);
            }
        }

        protected bool ResolutionFailed(IMessageDialog dialog, ResolutionContext ctx, List<AddinResolution> adnResolutions)
        {
            if (adnResolutions.Count > 0)
                return false;
            ctx.Dispose();
            DoPersist();
            if (dialog.HasMessage)
                dialog.Show();
            return true;
        }

        void DoPersist()
        {
            if (!_indexManager.Changed && !_bodyRepo.Changed)
                return;

            var storage = _indexManager.Storage;
            storage.StartTransaction();
            try
            {
                _indexManager.Write();
                _bodyRepo.Flush();
                storage.CommitTransaction();
            }
            catch
            {
                storage.RollbackTransaction();
            }
        }
    }

    partial class AddinResolver
    {
        #region FromPersistentObject
        static AddinResolution FromPersistentObject(AddinRecord addinRecord)
        {
            var addinIndex = addinRecord.AddinIndex;
            var addinBody = addinRecord.AddinBody;

            AddinResolution result;
            switch (addinIndex.OperationStatus)
            {
                case AddinOperationStatus.Unaffected:
                    result = new UnaffectedAddinResolution();
                    break;
                case AddinOperationStatus.IndirectlyAffected:
                    result = new IndirectlyAffectedAddinResolution(addinRecord);
                    break;
                case AddinOperationStatus.DirectlyAffected:
                    result = new DirectlyAffectedAddinResolution();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            result.AddinHeader = addinIndex.AddinHeader;
            result.ManifestFile = addinIndex.ManifestFile;
            result.RunningStatus = addinIndex.RunningStatus;

            #region AddinIndexRecord
            if (addinIndex.AssemblyFiles != null)
            {
                result.Assemblies = new List<AssemblyResolution>();
                foreach (var assemblyFile in addinIndex.AssemblyFiles)
                    result.Assemblies.Add(AssemblyResolution.CreateAddinAssembly(result, assemblyFile));
            }
            //if (addinIndex.ReferencedAssemblies != null) { }
            //if (addinIndex.ExtendedAddins != null) { } 
            #endregion

            switch (addinIndex.OperationStatus)
            {
                case AddinOperationStatus.Unaffected:
                    FromUnaffectedPersistentObject(result, addinBody);
                    break;
                case AddinOperationStatus.IndirectlyAffected:
                    FromIndirectlyAffectedPersistentObject(result, addinBody);
                    break;
                case AddinOperationStatus.DirectlyAffected:
                    FromDirectlyAffectedPersistentObject(result, addinBody);
                    break;
            }

            return result;
        }

        #region DirectlyAffected
        static void FromDirectlyAffectedPersistentObject(AddinResolution result, AddinBodyRecord addinBody)
        {
            if (addinBody.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinBody.ExtensionPoints)
                    result.ExtensionPoints.Add(FromDirectlyAffectedPersistentObject(result, extensionPoint));
            }

            if (addinBody.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinBody.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromDirectlyAffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinBody.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinBody.ExtensionGroups)
                    result.ExtensionGroups.Add(FromDirectlyAffectedPersistentObject(result, extensionGroup));
            } 
        }

        static ExtensionPointResolution FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new DirectlyAffectedExtensionPointResolution(addin, item)
            {
                Id = item.Id,
                TypeName = item.TypeName,
                Description = item.Description
            };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromDirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolutionGroup FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecordGroup item)
        {
            var result = new ExtensionBuilderResolutionGroup { ParentPath = item.ParentPath };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromDirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolution FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecord item)
        {
            ExtensionBuilderResolution result;
            if (item.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
            {
                result = new DirectlyAffectedDeclaredExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                    TypeName = item.TypeName,
                    Description = item.Description, 
                };
            }
            else
            {
                result = new DirectlyAffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                };
            }
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromDirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionResolutionGroup FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionRecordGroup item)
        {
            var result = new ExtensionResolutionGroup { ParentPath = item.ParentPath, RootIsExtensionPoint = item.RootIsExtensionPoint };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromDirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }

        static ExtensionResolution FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionRecord item)
        {
            var head = new ExtensionHeadResolution
            {
                Id = item.Head.Id,
                SiblingId = item.Head.SiblingId,
                RelativePosition = item.Head.RelativePosition,
                ExtensionBuilderUid = item.Head.ExtensionBuilderUid, 
                ParentPath = item.Head.ParentPath
            };

            var data = new ExtensionDataResolution();
            if (item.Data.Items != null)
            {
                foreach (var kv in item.Data.Items)
                    data.AddSerializableHolder(kv.Key, kv.Value);
            }

            var result = new DirectlyAffectedExtensionResolution(addin) { Head = head, Data = data };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromDirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }  
        #endregion

        #region IndirectlyAffected
        static void FromIndirectlyAffectedPersistentObject(AddinResolution result, AddinBodyRecord addinBody)
        {
            if (addinBody.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinBody.ExtensionPoints)
                    result.ExtensionPoints.Add(FromIndirectlyAffectedPersistentObject(result, extensionPoint));
            }

            if (addinBody.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinBody.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromIndirectlyAffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinBody.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinBody.ExtensionGroups)
                    result.ExtensionGroups.Add(FromIndirectlyAffectedPersistentObject(result, extensionGroup));
            }
        }

        static ExtensionPointResolution FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new IndirectlyAffectedExtensionPointResolution(addin, item)
            {
                Id = item.Id,
                TypeName = item.TypeName,
                Description = item.Description
            };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromIndirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolutionGroup FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecordGroup item)
        {
            var result = new ExtensionBuilderResolutionGroup { ParentPath = item.ParentPath };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromIndirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolution FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecord item)
        {
            ExtensionBuilderResolution result;
            if (item.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
            {
                result = new IndirectlyAffectedDeclaredExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                    TypeName = item.TypeName,
                    Description = item.Description
                };
            }
            else
            {
                result = new IndirectlyAffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                };
            }
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromIndirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionResolutionGroup FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionRecordGroup item)
        {
            var result = new ExtensionResolutionGroup { ParentPath = item.ParentPath, RootIsExtensionPoint = item.RootIsExtensionPoint };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromIndirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }

        static ExtensionResolution FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionRecord item)
        {
            var head = new ExtensionHeadResolution
            {
                Id = item.Head.Id,
                SiblingId = item.Head.SiblingId,
                RelativePosition = item.Head.RelativePosition,
                ExtensionBuilderUid = item.Head.ExtensionBuilderUid,
                ParentPath = item.Head.ParentPath
            };

            var data = new ExtensionDataResolution();
            if (item.Data.Items != null)
            {
                foreach (var kv in item.Data.Items)
                    data.AddSerializableHolder(kv.Key, kv.Value);
            }

            var result = new IndirectlyAffectedExtensionResolution(addin, item) { Head = head, Data = data };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromIndirectlyAffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }
        #endregion

        #region Unaffected
        static void FromUnaffectedPersistentObject(AddinResolution result, AddinBodyRecord addinBody)
        {
            if (addinBody.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinBody.ExtensionPoints)
                    result.ExtensionPoints.Add(FromUnaffectedPersistentObject(result, extensionPoint));
            }

            if (addinBody.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinBody.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromUnaffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinBody.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinBody.ExtensionGroups)
                    result.ExtensionGroups.Add(FromUnaffectedPersistentObject(result, extensionGroup));
            }
        }

        static ExtensionPointResolution FromUnaffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new UnaffectedExtensionPointResolution(addin, item)
            {
                Id = item.Id,
                TypeName = item.TypeName,
                Description = item.Description
            };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromUnaffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolutionGroup FromUnaffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecordGroup item)
        {
            var result = new ExtensionBuilderResolutionGroup { ParentPath = item.ParentPath };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromUnaffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionBuilderResolution FromUnaffectedPersistentObject(AddinResolution addin, ExtensionBuilderRecord item)
        {
            ExtensionBuilderResolution result;
            if (item.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
            {
                result = new UnaffectedDeclaredExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                    TypeName = item.TypeName,
                    Description = item.Description
                };
            }
            else
            {
                result = new UnaffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Id = item.Id,
                    ParentPath = item.ParentPath,
                    ExtensionPointId = item.ExtensionPointId,
                };
            }
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childBuilder = FromUnaffectedPersistentObject(addin, child);
                    result.AddChild(childBuilder);
                }
            }
            return result;
        }

        static ExtensionResolutionGroup FromUnaffectedPersistentObject(AddinResolution addin, ExtensionRecordGroup item)
        {
            var result = new ExtensionResolutionGroup { ParentPath = item.ParentPath, RootIsExtensionPoint = item.RootIsExtensionPoint };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromUnaffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }

        static ExtensionResolution FromUnaffectedPersistentObject(AddinResolution addin, ExtensionRecord item)
        {
            var head = new ExtensionHeadResolution
            {
                Id = item.Head.Id,
                SiblingId = item.Head.SiblingId,
                RelativePosition = item.Head.RelativePosition,
                ExtensionBuilderUid = item.Head.ExtensionBuilderUid,
                ParentPath = item.Head.ParentPath
            };

            var data = new ExtensionDataResolution();
            if (item.Data.Items != null)
            {
                foreach (var kv in item.Data.Items)
                    data.AddSerializableHolder(kv.Key, kv.Value);
            }

            var result = new UnaffectedExtensionResolution(addin) { Head = head, Data = data };
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childExtension = FromUnaffectedPersistentObject(addin, child);
                    result.AddChild(childExtension);
                }
            }
            return result;
        }
        #endregion

        #endregion

        protected void PersistAddinRecords(ResolutionContext ctx, List<AddinResolution> adnResolutions)
        {
            // assign assembly uid first, because we needs to assign the same uid for same assembly if there is any 
            // pre-existing addins that provides them.
            var assemblySets = ctx.AssemblySets;
            foreach (var assemblySet in assemblySets)
                AssignUidForAssemblySet(assemblySet);

            for (int i = 0; i < adnResolutions.Count; i++)
            {
                var adnResolution = adnResolutions[i];
                var adnRecord = adnResolution.ToRecord();
                _bodyRepo.Add(adnRecord.AddinBody);
                // the addin might has been resolved fail the last time.
                _indexManager.RemoveInvalidAddinFilePack(adnRecord.AddinIndex.AddinDirectory);
                _indexManager.AddAddin(adnRecord.AddinIndex);
            }

            DoPersist();
        }

        static void AssignUidForAssemblySet(AssemblyResolutionSet assemblySet)
        {
            var existingUid = UidProvider.InvalidAssemblyUid;
            foreach (var assembly in assemblySet)
            {
                if (assembly.Uid == UidProvider.InvalidAssemblyUid)
                    continue;
                if (existingUid != UidProvider.InvalidAssemblyUid && existingUid != assembly.Uid)
                    throw new Exception();
                existingUid = assembly.Uid;
            }
            existingUid = existingUid != UidProvider.InvalidAssemblyUid ? existingUid : IndexManager.GetNextAssemblyUid();
            foreach (var assembly in assemblySet)
                assembly.Uid = existingUid;
        }
    }
}