//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata;
using JointCode.Common.Conversion;
using System.Collections.Generic;

namespace JointCode.AddIns.Resolving
{
    // scan addin probing directories for new and updated addins.
    class DefaultAddinResolver : AddinResolver
    {
        internal DefaultAddinResolver(AddinStorage addinStorage, AddinRelationManager addinRelationManager, ConvertionManager convertionManager)
            : base(addinStorage, addinRelationManager, convertionManager) { }

        internal override ResolutionResult Resolve(INameConvention nameConvention, ResolutionContext ctx, ScanFilePackResult scanFilePackResult)
        {
            var resolutionResult = new ResolutionResult();
            // try parsing the new (or updated) addin manifests (configuration)
            var adnResolutions = TryParseAddins(nameConvention, resolutionResult, scanFilePackResult.ScanFilePacks);
            if (adnResolutions == null)
            {
                resolutionResult.NewAddinsFound = false;
                return resolutionResult;
            }

            var addinCollision = new AddinCollision();

            // try to register id of new addins at first, so that we can tell whether there are
            // any updated addins when registering that of the existing addins.
            foreach (var adnResolution in adnResolutions)
                TryRegisterAddin(resolutionResult, ctx, adnResolution, addinCollision);

            // register all assets of existing addins to the context (skipping updated addins)
            List<AddinResolution> resolableAddins = null;
            if (AddinStorage.AddinRecordCount > 0)
                resolableAddins = RegisterExistingAssets(resolutionResult, ctx, addinCollision);

            // try to register assets of new and updated addins to the context
            foreach (var adnResolution in adnResolutions)
                TryRegisterAssets(resolutionResult, ctx, adnResolution, addinCollision);

            if (resolableAddins != null)
                adnResolutions.AddRange(resolableAddins);

            // tries to resolve all addin, and make sure:
            // 1. there is no cirular dependencies between the resolved addins.
            // 2. the resolved addin list is sorted by the dependency.
            var resolvedAddins = TryResolveAddins(resolutionResult, ConvertionManager, ctx, adnResolutions);

            if (adnResolutions.Count > 0)
                StoreUnresolvableAddins(adnResolutions); // 剩余的 adnResolutions 即为未成功解析的插件，此处也要将它们持久化

            if (ResolutionFailed(resolutionResult, ctx, resolvedAddins))
            {
                resolutionResult.NewAddinsFound = false;
                return resolutionResult;
            }

            // if there is any conflicting addins, trim them and all addins that depends on them.
            if (addinCollision.Count > 0)
            {
                TrimConflictingAddins(addinCollision, resolvedAddins); // recursively
                if (ResolutionFailed(resolutionResult, ctx, resolvedAddins))
                {
                    resolutionResult.NewAddinsFound = false;
                    return resolutionResult;
                }
            }

            // save all resolvable addin records to persistent file.
            StoreResolvedAddins(resolutionResult, ctx, resolvedAddins);

            PersistAddinStorage(resolutionResult);

            ctx.Dispose();

            resolutionResult.NewAddinsFound = true;
            return resolutionResult;
        }
    }
}
