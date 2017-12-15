//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving
{
    // scan addin probe directories for new and updated addins.
    class DefaultAddinResolver : AddinResolver
    {
        internal DefaultAddinResolver(IndexManager indexManager, BodyRepository bodyRepo, ConvertionManager convertionManager)
            : base(indexManager, bodyRepo, convertionManager) { }

        internal override bool Resolve(IMessageDialog dialog, FilePackResult filePackResult)
        {
            // try parsing the new (or updated) addin manifests (configuration)
            var adnResolutions = TryParseAddins(dialog, filePackResult.AddinFilePacks);
            if (adnResolutions == null)
                return false;
            
            var ctx = new ResolutionContext();
            var addinCollision = new AddinCollision();

            // try to register id of new addins at first, so that we can tell whether there are
            // any updated addins when registering that of the existing addins.
            foreach (var adnResolution in adnResolutions)
                TryRegisterAddin(dialog, ctx, adnResolution, addinCollision);

            // register all assets of existing addins to the context (skipping updated addins)
            List<AddinResolution> resolableAddins = null;
            if (_indexManager.AddinCount > 0)
                resolableAddins = RegisterExistingAssets(dialog, ctx, addinCollision, adnResolutions);

            // try to register assets of new and updated addins to the context
            foreach (var adnResolution in adnResolutions)
                TryRegisterAssets(dialog, ctx, adnResolution, addinCollision);

            if (resolableAddins != null)
                adnResolutions.AddRange(resolableAddins);

            // tries to resolve all addin, and make sure:
            // 1. there is no cirular dependencies between the resolved addins.
            // 2. the resolved addin list is sorted by the dependency.
            adnResolutions = TryResolveAddins(dialog, _convertionManager, ctx, adnResolutions);
            if (ResolutionFailed(dialog, ctx, adnResolutions))
                return false;

            // if there is any conflicting addins, trim them and all addins that depends on them.
            if (addinCollision.Count > 0)
            {
                TrimConflictingAddins(addinCollision, adnResolutions);
                if (ResolutionFailed(dialog, ctx, adnResolutions))
                    return false;
            }

            // save all new and/or updated addin records to persistent file.
            PersistAddinRecords(ctx, adnResolutions);

            ctx.Dispose();
            return true;
        }
    }
}
