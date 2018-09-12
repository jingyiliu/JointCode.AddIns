using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Dependencies;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Resolving;
using JointCode.Common;
using System.Collections.Generic;

namespace JointCode.AddIns
{
    public partial class AddinEngine
    {
        /// <summary>
        /// Install an addin at runtime.
        /// </summary>
        /// <param name="addinProbingPath"></param>
        /// <param name="addinDirectory"></param>
        public void Install(string addinProbingPath, string addinDirectory)
        {
            DoInstallOrUpdate(addinProbingPath, addinDirectory, ParseAndResolveAddin);
        }

        // @return value: whether new or updated addins found.
        ResolutionResult ParseAndResolveAddin(ScanFilePack filePack)
        {
            var scanFilePackResult = new ScanFilePackResult();
            scanFilePackResult.AddScanFilePack(filePack);
            var proxy = new AddinResolverProxy();
            return proxy.Resolve(_addinFramework.NameConvention, _addinFramework.FileSettings, _addinFramework.AssemblyLoadPolicy, _addinStorage, _addinRelationManager, scanFilePackResult);
        }

        /// <summary>
        /// Update an addin at runtime.
        /// </summary>
        /// <param name="addinProbingPath"></param>
        /// <param name="addinDirectory"></param>
        public void Update(string addinProbingPath, string addinDirectory)
        {
            DoInstallOrUpdate(addinProbingPath, addinDirectory, ParseAndUpdateAddin);
        }

        // @return value: whether new or updated addins found.
        ResolutionResult ParseAndUpdateAddin(ScanFilePack filePack)
        {
            var scanFilePackResult = new ScanFilePackResult();
            scanFilePackResult.AddScanFilePack(filePack);
            var proxy = new AddinResolverProxy();
            return proxy.Resolve(_addinFramework.NameConvention, _addinFramework.FileSettings, _addinFramework.AssemblyLoadPolicy, _addinStorage, _addinRelationManager, scanFilePackResult);
        }

        void DoInstallOrUpdate(string addinProbingPath, string addinDirectory, MyFunc<ScanFilePack, ResolutionResult> func)
        {
            VerifyInitialized();

            var filePack = GetScanFilePack(addinProbingPath, addinDirectory);
            if (filePack == null)
                return;

            var result = func(filePack);

            if (result.HasMessage)
                _addinFramework.MessageDialog.Show(result.GetFormattedString(), "Information");

            if (result.NewAddinsFound)
            {
                _addinStorage = new AddinStorage(_addinFramework.FileSettings.StorageFilePath);
                var hasAddins = _addinStorage.ReadOrReset();
                if (hasAddins)
                    _addinRelationManager.ResetRelationMaps(_addinStorage.AddinRecords);
            }
        }

        // tries to get addin files that needed to be scaned or re-scaned (manifest or any of assembly files updated).
        // excluding: those files that have been scanned the last time, if they are unchanged.
        ScanFilePack GetScanFilePack(string addinProbingPath, string addinDirectory)
        {
            var existingFilePacks = GetExistingAddinFilePacks(_addinStorage);
            return FilePackService.GetScanFilePack(_addinFramework.FileSettings,
                existingFilePacks, addinProbingPath, addinDirectory);
        }

        /// <summary>
        /// Uninstall an addin at runtime.
        /// </summary>
        /// <param name="addinId"></param>
        public void Uninstall(AddinId addinId)
        {
            VerifyInitialized();

            Addin addin;
            if (_addinFramework.Repository.TryGetAddin(ref addinId._guid, out addin))
                return;

            // 卸载受影响的插件
            var affectedAddins = _addinRelationManager.TryGetAffectingAddins(addin.AddinRecord);
            if (affectedAddins != null)
            {
                if (!_addinFramework.MessageDialog.Confirm
                    ("The following addins is depending on the addin {0}, if you uninstall this addin, they will be uninstalled too. Do you really want to uninstall these addins?", "Confirmation"))
                    return;
            }

            // 卸载指定插件
            DoUninstall(addin.AddinRecord);
        }

        void DoUninstall(AddinRecord addinRecord)
        { }

        /// <summary>
        /// Gets a dependency description for the specified addin.
        /// </summary>
        /// <param name="addinId"></param>
        /// <returns></returns>
        public DependencyDescription GetDependencyDescription(AddinId addinId)
        {
            VerifyInitialized();

            Addin addin;
            if (_addinFramework.Repository.TryGetAddin(ref addinId._guid, out addin))
                return null;

            var addinRecord = addin.AddinRecord;

            List<AddinRecord> dependedAddins;
            if (_addinRelationManager.TryGetReferencedAddins(addinRecord, out dependedAddins))
            {
                List<AddinRecord> extendedAddins;
                if (_addinRelationManager.TryGetExtendedAddins(addinRecord, out extendedAddins))
                    dependedAddins.AddRange(extendedAddins);
            }
            else
            {
                if (_addinRelationManager.TryGetExtendedAddins(addinRecord, out dependedAddins))
                    return null;
            }

            var addinDependencies = new AddinDependency[dependedAddins.Count];
            for (int i = 0; i < addinDependencies.Length; i++)
            {
                var dependedAddin = dependedAddins[i];
                addinDependencies[i] = new AddinDependency(dependedAddin.Guid,
                    dependedAddin.AddinHeader.Version, dependedAddin.AddinHeader.CompatVersion);
            }

            var appAssemblyResolver = new DependedApplicationAssemblyResolver();
            var appAssemblyDependencies = appAssemblyResolver.GetRequiredApplicationAssemblyDependencies(addinRecord);

            return new DependencyDescription
            {
                ExtendedAddins = addinDependencies,
                ApplicationAssemblies = appAssemblyDependencies
            };
        }

        /// <summary>
        /// Determins whether an addin can be installed to the application, i.e, whether its dependencies is satisfied.
        /// </summary>
        /// <param name="dependencyDescription"></param>
        /// <returns></returns>
        public bool CanInstall(DependencyDescription dependencyDescription)
        {
            VerifyInitialized();
            return false;
        }
    }
}