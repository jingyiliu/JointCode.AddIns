//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.AddIns.Parsing;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;
using JointCode.Common.Extensions;
using System;
using System.Collections.Generic;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Extension;

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
        protected List<AddinResolution> RegisterExistingAssets(ResolutionResult resolutionResult, ResolutionContext ctx, AddinCollision addinCollision)
        {
            // =================================================
            // 1. 首先确定 AddinStorage 中各个现有 addin 的状态：未受影响的、已更新的、间接受影响的
            // check whether there are updated addins.
            // and if there are any, mark their operation status as updated.
            List<AddinRecord> updatedAddins = null;
            for (int i = AddinStorage.AddinRecordCount - 1; i >= 0; i--)
            {
                var existingAddin = AddinStorage.Get(i);
                var addinId = existingAddin.AddinId;
                AddinResolution adnResolution;
                // 如果 ResolutionContext 中已存在相同 guid 的插件，则表明这是一个更新的插件
                if (!ctx.TryGetAddin(addinId, out adnResolution))
                    continue;

                AddinStorage.Remove(existingAddin);
                //AddinRelationManager.RemoveRelationMap(existingAddin);
                //adnResolution.OperationStatus = AddinOperationStatus.Updated;

                updatedAddins = updatedAddins ?? new List<AddinRecord>();
                updatedAddins.Add(existingAddin);
            }

            if (AddinStorage.AddinRecordCount == 0)
                return null; // all addins are updated addins.

            // mark directly affected and indirectly affected addins.
            List<AddinRecord> directlyAffectedAddins = null, indirectlyAffectedAddins = null;
            if (updatedAddins != null)
            {
                directlyAffectedAddins = AddinRelationManager.TryGetAffectingAddins(updatedAddins);
                if (directlyAffectedAddins != null)
                {
                    indirectlyAffectedAddins = AddinRelationManager.TryGetAllAffectingAddins(directlyAffectedAddins);
                    //if (indirectlyAffectedAddins != null)
                    //{
                    //    for (int i = indirectlyAffectedAddins.Count - 1; i >= 0; i--)
                    //    {
                    //        if (directlyAffectedAddins.Contains(indirectlyAffectedAddins[i]))
                    //            indirectlyAffectedAddins.RemoveAt(i);
                    //    }
                    //}
                }
            }

            // =================================================
            // 2. 根据 AddinStorage 中各个现有 addin 的状态，将它们注册到 ResolutionContext
            if (updatedAddins != null)
            {
                foreach (var updatedAddin in updatedAddins)
                {
                    var ebs = updatedAddin.GetAllExtensionBuilders();
                    if (ebs != null)
                    {
                        foreach (var eb in ebs)
                        {
                            if (eb.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
                                // 将已更新插件的 ExtensionBuilderPath 映射注册到 context。
                                // 因为在解析 Extension 时，是根据 ExtensionBuilderPath 查找 ExtensionBuilder 的，但对于 [directlyAffectedAddins 插件的 Extension] 来说，它们并不
                                // 保存自身依赖的 updateAddins 的 ExtensionBuilder 的 ExtensionBuilderPath。
                                // 所以，只能在解析前先将 updateAddins 的 ExtensionBuilder 的 ExtensionBuilderPath 注册到 context，后面解析 [directlyAffectedAddins 插件的 Extension]
                                // 时，才能通过 uid 获得目标 ExtensionBuilder 的 path，继而找到 ExtensionBuilder
                                ctx.RegisterExtensionBuilderPath(eb.Uid, eb.GetPath()); 
                        }
                    }
                }
            }

            List<AddinResolution> resolableAddins = null;
            // decide how to register assets of these addins and whether to resolve these addins according to their operation status.
            if (directlyAffectedAddins != null)
            {
                foreach (var directlyAffectedAddin in directlyAffectedAddins)
                {
                    AddinStorage.Remove(directlyAffectedAddin);
                    var resolvableAddin = DoRegisterExistingAddin(resolutionResult, ctx, addinCollision, directlyAffectedAddin, AddinOperationStatus.DirectlyAffected);
                    resolableAddins = resolableAddins ?? new AddinResolutionSet();
                    resolableAddins.Add(resolvableAddin);
                }
            }

            if (indirectlyAffectedAddins != null)
            {
                foreach (var indirectlyAffectedAddin in indirectlyAffectedAddins)
                {
                    AddinStorage.Remove(indirectlyAffectedAddin);
                    var resolvableAddin = DoRegisterExistingAddin(resolutionResult, ctx, addinCollision, indirectlyAffectedAddin, AddinOperationStatus.IndirectlyAffected);
                    resolableAddins = resolableAddins ?? new AddinResolutionSet();
                    resolableAddins.Add(resolvableAddin);
                }
            }

            // since the updated, directly affected and indirectly affected, they are all removed, so the rest is unaffected
            for (int i = AddinStorage.AddinRecordCount - 1; i >= 0; i--)
            {
                var unaffectedAddin = AddinStorage.Get(i);
                AddinStorage.Remove(unaffectedAddin);
                var resolvableAddin = DoRegisterExistingAddin(resolutionResult, ctx, addinCollision, unaffectedAddin, AddinOperationStatus.Unaffected);
                resolableAddins = resolableAddins ?? new AddinResolutionSet();
                resolableAddins.Add(resolvableAddin);
            }

            return resolableAddins;
        }

        // 根据 existingAddin 的状态，将其 Assets 注册到 ResolutionContext
        AddinResolution DoRegisterExistingAddin(ResolutionResult resolutionResult, ResolutionContext ctx, AddinCollision addinCollision, 
            AddinRecord existingAddin, AddinOperationStatus operationStatus)
        {
            //AddinRecord addinRecord;
            //if (!AddinRelationManager.TryGetAddin(existingAddin.Guid, out addinRecord))
            //    throw new InconsistentStateException();

            ////AddinRelationManager.RemoveAddin(existingAddin);
            //AddinStorage.Remove(existingAddin);

            //if (operationStatus == AddinOperationStatus.NewOrUpdated)
            //{
            //    var ebs = existingAddin.GetAllExtensionBuilders();
            //    if (ebs != null)
            //    {
            //        foreach (var eb in ebs)
            //        {
            //            if (eb.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
            //                ctx.RegisterExtensionBuilderPath(eb.Uid, eb.GetPath());
            //        }
            //    }
            //    return null;
            //}

            var adnResolution = FromPersistentObject(existingAddin, operationStatus);
            TryRegisterAddin(resolutionResult, ctx, adnResolution, addinCollision);
            DoRegisterExistingAssets(resolutionResult, ctx, adnResolution);
            // if the operation status of an addin not equals to unaffected (i.e, directly/indirectly affected addin), it need to 
            // be resolved, so we add it to the addin resolution list.
            return adnResolution;
            //return operationStatus == AddinOperationStatus.Unaffected ? null : adnResolution;
        }

        void DoRegisterExistingAssets(ResolutionResult resolutionResult, ResolutionContext ctx, AddinResolution adnResolution)
        {
            if (adnResolution.Assemblies != null)
            {
                foreach (var asm in adnResolution.Assemblies)
                    ctx.RegisterAssembly(resolutionResult, asm);
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

        protected bool TryRegisterAddin(ResolutionResult resolutionResult, ResolutionContext ctx, AddinResolution adnResolution,
            AddinCollision addinCollision)
        {
            AddinResolution existingAddin;
            if (ctx.TryRegisterAddin(resolutionResult, adnResolution.AddinId, adnResolution, out existingAddin))
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
        protected bool TryRegisterAssets(ResolutionResult resolutionResult, ResolutionContext ctx, AddinResolution adnResolution,
           AddinCollision addinCollision)
        {
            var success = true;
            
            if (adnResolution.Assemblies != null)
            {
                for (int i = 0; i < adnResolution.Assemblies.Count; i++)
                {
                    var asm = adnResolution.Assemblies[i];
                    if (ctx.TryRegisterAssembly(resolutionResult, asm))
                        continue;
                    // if the assembly loading (with mono.cecil) is failed, it's because this is not a valid managed assembly (might be a native library), 
                    // just remove it from the assembly list, and add it to the data file list instead.
                    adnResolution.Assemblies.RemoveAt(i);
                    i -= 1;
                    adnResolution.DataFiles.Add(new DataFileResolution{ FilePath = asm.AssemblyFile.FilePath });
                }
            	//foreach (var asm in adnResolution.Assemblies) 
            	//	ctx.TryRegisterAssembly(resolutionResult, asm);
            }

            if (adnResolution.ExtensionPoints != null) 
            {
                foreach (var extensionPoint in adnResolution.ExtensionPoints)
                {
                    ExtensionPointResolution existingExtensionPoint;
                    if (!ctx.TryRegisterExtensionPoint(resolutionResult, extensionPoint, out existingExtensionPoint))
                    {
                        var key = new ExtensionPointCollisionKey(existingExtensionPoint.Name);
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
                    if (!ctx.TryRegisterExtensionBuilder(resolutionResult, extensionBuilder, out existingExtensionBuilder))
                    {
                        var key = new ExtensionBuilderCollisionKey(existingExtensionBuilder.Name);
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
                    if (!ctx.TryRegisterExtension(resolutionResult, extension, out existingExtension))
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
        protected List<AddinResolution> TryParseAddins(INameConvention nameConvention, ResolutionResult resolutionResult, IEnumerable<ScanFilePack> filePacks)
        {
            var result = new List<AddinResolution>();
            foreach (var filePack in filePacks)
            {
                foreach (var addinParser in _addinParsers)
                {
                    AddinManifest addinManifest;
                    if (addinParser.TryParse(filePack, out addinManifest))
                    {
                        AddinResolution adnResolution;
                        if (addinManifest.Introspect(nameConvention, resolutionResult) && addinManifest.TryParse(resolutionResult, out adnResolution))
                        {
                            result.Add(adnResolution);
                            break;
                        }
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
        protected List<AddinResolution> TryResolveAddins(ResolutionResult resolutionResult, ConvertionManager convertionManager, 
            ResolutionContext ctx, List<AddinResolution> adnResolutions)
        {
            var result = new List<AddinResolution>();
            int index = 0, retryTimes = 0;

            while (true)
            {
                if (index >= adnResolutions.Count)
                    index = 0;
                var adnResolution = adnResolutions[index];

                var resolutionStatus = adnResolution.Resolve(resolutionResult, convertionManager, ctx);

                if (resolutionStatus.IsFailed())
                {
                    retryTimes = 0;
                    index += 1;
                    RemoveInvalidAddinsRecursively(ctx, adnResolutions, adnResolution);

                    if (adnResolution.AddinHeader.Name.IsNullOrWhiteSpace())
                        resolutionResult.AddError("Failed to resolve addin [" + adnResolution.AddinId.Guid + "]");
                    else
                        resolutionResult.AddError("Failed to resolve addin [" + adnResolution.AddinHeader.Name + "] [" + adnResolution.AddinId.Guid + "]");
                }
                else if (resolutionStatus.IsPending()) // refers to other addins that has not been resolved
                {
                    retryTimes += 1;
                    index += 1;
                }
                else if (resolutionStatus.IsSuccess())
                {
                    retryTimes = 0;
                    adnResolutions.RemoveAt(index);
                    result.Add(adnResolution);
                }

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
            AddinStorage.TryRemove(adnResolution.Guid);
            AddinStorage.AddInvalidAddinFilePack(AddinResolution.ToAddinFilePack(adnResolution));

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

            AddinStorage.TryRemove(adnResolution.Guid);
            AddinStorage.AddInvalidAddinFilePack(AddinResolution.ToAddinFilePack(adnResolution));

            for (int i = 0; i < adnResolutions.Count; i++)
            {
                var otherResolution = adnResolutions[i];
                if (!otherResolution.CanResolveWithout(adnResolution))
                    RemoveInvalidAddinsRecursively2(adnResolutions, otherResolution);
            }
        }

        protected bool ResolutionFailed(ResolutionResult resolutionResult, ResolutionContext ctx, List<AddinResolution> adnResolutions)
        {
            if (adnResolutions.Count > 0)
                return false;
            ctx.Dispose();
            PersistAddinStorage(resolutionResult);
            return true;
        }

        protected void PersistAddinStorage(ResolutionResult resolutionResult)
        {
            if (!AddinStorage.Changed)
                return;

            //var storage = AddinRelationManager.Storage;
            //storage.StartTransaction();
            //try
            //{
            //    AddinRelationManager.Write();
            //    AddinStorage.Flush();
            //    storage.CommitTransaction();
            //}
            //catch
            //{
            //    storage.RollbackTransaction();
            //}

            var result = AddinStorage.Write();
            if (!result)
                resolutionResult.AddError("Failed to write to the addin storage file!");
        }
    }

    partial class AddinResolver
    {
        #region FromPersistentObject
        static AddinResolution FromPersistentObject(AddinRecord addinRecord, AddinOperationStatus operationStatus)
        {
            AddinResolution result;
            switch (operationStatus)
            {
                case AddinOperationStatus.Unaffected:
                    result = new UnaffectedAddinResolution(addinRecord);
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

            result.AddinHeader = addinRecord.AddinHeader;
            result.ManifestFile = addinRecord.ManifestFile;
            result.Enabled = addinRecord.Enabled;

            #region AddinRecord
            if (addinRecord.AssemblyFiles != null)
            {
                result.Assemblies = new List<AssemblyResolution>();
                foreach (var assemblyFile in addinRecord.AssemblyFiles)
                    result.Assemblies.Add(AssemblyResolution.CreateAddinAssembly(result, assemblyFile));
            }
            //if (addinIndex.ReferencedAssemblies != null) { }
            //if (addinIndex.ExtendedAddins != null) { } 
            #endregion

            switch (operationStatus)
            {
                case AddinOperationStatus.Unaffected:
                    FromUnaffectedPersistentObject(result, addinRecord);
                    break;
                case AddinOperationStatus.IndirectlyAffected:
                    FromIndirectlyAffectedPersistentObject(result, addinRecord);
                    break;
                case AddinOperationStatus.DirectlyAffected:
                    FromDirectlyAffectedPersistentObject(result, addinRecord);
                    break;
            }

            return result;
        }

        #region DirectlyAffected
        static void FromDirectlyAffectedPersistentObject(AddinResolution result, AddinRecord addinExtension)
        {
            if (addinExtension.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinExtension.ExtensionPoints)
                    result.ExtensionPoints.Add(FromDirectlyAffectedPersistentObject(result, extensionPoint));
            }

            if (addinExtension.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinExtension.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromDirectlyAffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinExtension.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinExtension.ExtensionGroups)
                    result.ExtensionGroups.Add(FromDirectlyAffectedPersistentObject(result, extensionGroup));
            } 
        }

        static ExtensionPointResolution FromDirectlyAffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new DirectlyAffectedExtensionPointResolution(addin, item)
            {
                Name = item.Name,
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
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
                    TypeName = item.TypeName,
                    Description = item.Description, 
                };
            }
            else
            {
                result = new DirectlyAffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
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

            DirectlyAffectedExtensionResolution result;
            if (item.Data != null && item.Data.Items != null)
            {
                var data = new ExtensionDataResolution();
                foreach (var kv in item.Data.Items)
                    data.AddDataHolder(kv.Key, kv.Value);
                result = new DirectlyAffectedExtensionResolution(addin) { Head = head, Data = data };
            }
            else
            {
                result = new DirectlyAffectedExtensionResolution(addin) { Head = head };
            }
            
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
        static void FromIndirectlyAffectedPersistentObject(AddinResolution result, AddinRecord addinExtension)
        {
            if (addinExtension.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinExtension.ExtensionPoints)
                    result.ExtensionPoints.Add(FromIndirectlyAffectedPersistentObject(result, extensionPoint));
            }

            if (addinExtension.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinExtension.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromIndirectlyAffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinExtension.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinExtension.ExtensionGroups)
                    result.ExtensionGroups.Add(FromIndirectlyAffectedPersistentObject(result, extensionGroup));
            }
        }

        static ExtensionPointResolution FromIndirectlyAffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new IndirectlyAffectedExtensionPointResolution(addin, item)
            {
                Name = item.Name,
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
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
                    TypeName = item.TypeName,
                    Description = item.Description
                };
            }
            else
            {
                result = new IndirectlyAffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
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

            IndirectlyAffectedExtensionResolution result;
            if (item.Data != null && item.Data.Items != null)
            {
                var data = new ExtensionDataResolution();
                foreach (var kv in item.Data.Items)
                    data.AddDataHolder(kv.Key, kv.Value);
                result = new IndirectlyAffectedExtensionResolution(addin, item) { Head = head, Data = data };
            }
            else
            {
                result = new IndirectlyAffectedExtensionResolution(addin, item) { Head = head };
            }
            
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
        static void FromUnaffectedPersistentObject(AddinResolution result, AddinRecord addinExtension)
        {
            if (addinExtension.ExtensionPoints != null)
            {
                result.ExtensionPoints = new List<ExtensionPointResolution>();
                foreach (var extensionPoint in addinExtension.ExtensionPoints)
                    result.ExtensionPoints.Add(FromUnaffectedPersistentObject(result, extensionPoint));
            }

            if (addinExtension.ExtensionBuilderGroups != null)
            {
                result.ExtensionBuilderGroups = new List<ExtensionBuilderResolutionGroup>();
                foreach (var extensionBuilderGroup in addinExtension.ExtensionBuilderGroups)
                    result.ExtensionBuilderGroups.Add(FromUnaffectedPersistentObject(result, extensionBuilderGroup));
            }

            if (addinExtension.ExtensionGroups != null)
            {
                result.ExtensionGroups = new List<ExtensionResolutionGroup>();
                foreach (var extensionGroup in addinExtension.ExtensionGroups)
                    result.ExtensionGroups.Add(FromUnaffectedPersistentObject(result, extensionGroup));
            }
        }

        static ExtensionPointResolution FromUnaffectedPersistentObject(AddinResolution addin, ExtensionPointRecord item)
        {
            var result = new UnaffectedExtensionPointResolution(addin, item)
            {
                Name = item.Name,
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
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
                    TypeName = item.TypeName,
                    Description = item.Description
                };
            }
            else
            {
                result = new UnaffectedReferencedExtensionBuilderResolution(addin, item)
                {
                    Name = item.Name,
                    ParentPath = item.ParentPath,
                    ExtensionPointName = item.ExtensionPointName,
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

            UnaffectedExtensionResolution result;
            if (item.Data != null && item.Data.Items != null)
            {
                var data = new ExtensionDataResolution();
                foreach (var kv in item.Data.Items)
                    data.AddDataHolder(kv.Key, kv.Value);
                result = new UnaffectedExtensionResolution(addin, item) {Head = head, Data = data};
            }
            else
            {
                result = new UnaffectedExtensionResolution(addin, item) {Head = head};
            }
            
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

        protected void StoreUnresolvableAddins(List<AddinResolution> adnResolutions)
        {
            for (int i = 0; i < adnResolutions.Count; i++)
            {
                var adnResolution = adnResolutions[i];
                AddinStorage.TryRemove(adnResolution.Guid);
                var filePack = AddinResolution.ToAddinFilePack(adnResolution);
                AddinStorage.AddInvalidAddinFilePack(filePack);
            }
        }

        protected void StoreResolvedAddins(ResolutionResult resolutionResult, ResolutionContext ctx, List<AddinResolution> adnResolutions)
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
                // the addin might have been failed to resolve at the last time, if that is true, it will be in the invalid addin list, 
                // so we need to remove it from the invalid file packs, if it is there.
                AddinStorage.RemoveInvalidAddinFilePack(adnRecord.BaseDirectory);
                AddinStorage.Add(adnRecord);
            }
        }

        static void AssignUidForAssemblySet(AssemblyResolutionSet assemblySet)
        {
            var existingUid = UidStorage.InvalidAssemblyUid;
            foreach (var assembly in assemblySet)
            {
                if (assembly.Uid == UidStorage.InvalidAssemblyUid)
                    continue;
                if (existingUid != UidStorage.InvalidAssemblyUid && existingUid != assembly.Uid)
                    throw new Exception();
                existingUid = assembly.Uid;
            }
            existingUid = existingUid != UidStorage.InvalidAssemblyUid ? existingUid : UidStorage.GetNextAssemblyUid();
            foreach (var assembly in assemblySet)
                assembly.Uid = existingUid;
        }
    }
}