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
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Loading;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core
{
    public partial class AddinEngine
    {
        readonly Dictionary<Guid, Addin> _guid2Addins;
        readonly AddinConfiguration _adnConfig;
        readonly RuntimeSystem _runtimeSystem;
        readonly RuntimeAssemblyResolver _asmResolver;
        readonly ExtensionSystemLoader _exSystemLoader;
        IndexManager _indexManager;
        BodyRepository _bodyRepo;

        public AddinEngine(AddinConfiguration addinConfig)
        {
            _adnConfig = addinConfig;
            _guid2Addins = new Dictionary<Guid, Addin>();
            _asmResolver = new RuntimeAssemblyResolver();
            _runtimeSystem = new RuntimeSystem(_asmResolver);
            _exSystemLoader = new ExtensionSystemLoader(_asmResolver);
        }

        /// <summary>
        /// Tries to load the extension point identified by the type of <see cref="TExtensionRoot"/>.
        /// If no matching extension point found, or the addin which declared the extension point has been
        /// disabled, nothing will happen.
        /// </summary>
        /// <typeparam name="TExtensionRoot">The type of the extension root.</typeparam>
        /// <param name="extensionRoot">The extension root.</param>
        /// <returns></returns>
        public bool TryLoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        {
            var extensionPointId = _adnConfig.NameConvention.GetExtensionPointName(typeof (TExtensionRoot));
            var addin = DoLoadExtensionPoint(extensionPointId, extensionRoot, false);
            return addin != null ? _exSystemLoader.TryLoadExtensionPoint(addin.AddinContext, extensionPointId, extensionRoot) : false;
        }

        public bool TryLoadExtensionPoint(string extensionPointId, object extensionRoot)
        {
            var addin = DoLoadExtensionPoint(extensionPointId, extensionRoot, false);
            return addin != null ? _exSystemLoader.TryLoadExtensionPoint(addin.AddinContext, extensionPointId, extensionRoot) : false;
        }

        public void LoadExtensionPoint(string extensionPointId, object extensionRoot)
        {
            var addin = DoLoadExtensionPoint(extensionPointId, extensionRoot, true);
            _exSystemLoader.LoadExtensionPoint(addin.AddinContext, extensionPointId, extensionRoot);
        }

        Addin DoLoadExtensionPoint(string extensionPointId, object extensionRoot, bool fastfail)
        {
            Requires.Instance.NotNull(extensionPointId, "extensionPointId");
            Requires.Instance.NotNull(extensionRoot, "extensionRoot");

            // get the addin that defined this extension point
            var addinIndex = _indexManager.GetDeclaringAddin(extensionPointId);
            if (addinIndex == null)
            {
                if (fastfail)
                    throw new ArgumentException();
                return null;
            }

            FindAndRegisterReferencedAssemblies(addinIndex);
            var addin = GetOrCreateAddin(addinIndex);

            var epRecord = addin.AddinBodyRecord.GetExtensionPoint(extensionPointId);
            Requires.Instance.EnsureNotNull(epRecord, "epRecord");
            _exSystemLoader.RegisterExtensionPoint(epRecord, extensionRoot.GetType());

            RegisterExtensionsForExtensionPoint(addin.AddinBodyRecord, epRecord);

            // register assets of extending addins
            var extendingAddinIndexes = _indexManager.TryGetSortedExtendingAddins(epRecord);
            if (extendingAddinIndexes != null)
            {
                foreach (var extendingAddinIndex in extendingAddinIndexes)
                {
                    FindAndRegisterReferencedAssemblies(extendingAddinIndex);
                    var extendedAddin = GetOrCreateAddin(extendingAddinIndex);
                    RegisterExtensionsForExtensionPoint(extendedAddin.AddinBodyRecord, epRecord);
                }
            }

            return addin;
        }

        Addin GetOrCreateAddin(AddinIndexRecord addinIndex)
        {
            Addin addin;
            if (!_guid2Addins.TryGetValue(addinIndex.Guid, out addin))
            {
                AddinBodyRecord addinBody;
                if (!_bodyRepo.TryGet(addinIndex.Guid, out addinBody))
                    return null;
                addin = new Addin(_guid2Addins, _runtimeSystem, addinIndex) { AddinBodyRecord = addinBody };
                _guid2Addins.Add(addinIndex.Guid, addin);
            }
            else if (addin.AddinBodyRecord == null)
            {
                AddinBodyRecord addinBody;
                if (!_bodyRepo.TryGet(addinIndex.AddinId.Guid, out addinBody))
                    return null;
                addin.AddinBodyRecord = addinBody;
            }
            return addin;
        }

        void FindAndRegisterReferencedAssemblies(AddinIndexRecord addinIndex)
        {
            List<AddinIndexRecord> referencedAddinIndexes;
            if (!_indexManager.TryGetAllReferencedAddins(addinIndex, out referencedAddinIndexes))
                throw new InvalidOperationException();

            if (referencedAddinIndexes != null)
            {
                foreach (var referencedAddinIndex in referencedAddinIndexes)
                {
                    Addin referencedAddin;
                    if (!_guid2Addins.TryGetValue(addinIndex.Guid, out referencedAddin))
                    {
                        referencedAddin = new Addin(_guid2Addins, _runtimeSystem, addinIndex);
                        _guid2Addins.Add(addinIndex.Guid, referencedAddin);
                    }
                    RegisterAddinAssemblies(referencedAddinIndex);
                }
            }

            // register assemblies of the declaring addin
            RegisterAddinAssemblies(addinIndex);
        }

        // register assemblies of the given addin to the assembly resolver, for getting them ready to be loaded into runtime.
        void RegisterAddinAssemblies(AddinIndexRecord addinIndex)
        {
            if (addinIndex.AssembliesRegistered || addinIndex.AssemblyFiles == null)
                return;
            if (addinIndex.AssemblyFiles != null)
                _asmResolver.RegisterAssemblies(addinIndex.AddinDirectory, addinIndex.AssemblyFiles);
            addinIndex.AssembliesRegistered = true;
        }

        void RegisterExtensionsForExtensionPoint(AddinBodyRecord addinBody, ExtensionPointRecord epRecord)
        {
            var ebGroup = addinBody.GetExtensionBuilderGroup(epRecord.Id);
            if (ebGroup != null)
			    _exSystemLoader.RegisterExtensionBuilderGroup(ebGroup);

            var exGroup = addinBody.GetExtensionGroup(epRecord.Id);
			if (exGroup != null)
                _exSystemLoader.RegisterExtensionGroup(epRecord, exGroup);
        }
    }
}
