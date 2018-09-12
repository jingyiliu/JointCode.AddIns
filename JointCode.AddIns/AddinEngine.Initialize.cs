//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.AddIns.Resolving;
using System.Collections.Generic;

namespace JointCode.AddIns
{
    public partial class AddinEngine
    {
        bool _initialized = false;

        void VerifyInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("The addin engine has not been initialized yet! Please call the Initialize method to set it up first!");
        }

        /// <summary>
        /// Initializes the <see cref="AddinEngine"/>, but do not start any addins yet.
        /// This way you gain greater controls upon the addin system, for example you can find an addin by its name, subscribe to events of that addin, and then start it later.
        /// </summary>
        /// <param name="shouldRefresh">
        /// if this value is set to <c>true</c>, the <seealso cref="AddinEngine"/> will scan the addin probing directories for 
        /// new and updated addins every time this method is called.
        /// otherwise, it will only scan the addin probing directories once.
        /// </param>
        /// <returns></returns>
        public bool Initialize(bool shouldRefresh)
        {
            if (_initialized)
                return true;

            _addinStorage = new AddinStorage(_addinFramework.FileSettings.StorageFilePath);
            var hasAddins = _addinStorage.ReadOrReset();

            if (hasAddins)
                _addinRelationManager.ResetRelationMaps(_addinStorage.AddinRecords);

            if (shouldRefresh || !hasAddins)
            {
                var filePackResult = GetScanFilePacks(_addinStorage);
                if (filePackResult != null)
                {
                    var result = ParseAndResolveAddins(filePackResult);
                    if (result.HasMessage)
                        _addinFramework.MessageDialog.Show(result.GetFormattedString(), "Addin resolution information");
                    if (result.NewAddinsFound)
                    {
                        _addinStorage.Reset();
                        hasAddins = _addinStorage.Read();
                        if (hasAddins)
                            _addinRelationManager.ResetRelationMaps(_addinStorage.AddinRecords);
                    }
                }
            }

            if (!hasAddins)
                return false;

            //_addinRelationManager.ResetRelationMaps(_addinStorage.AddinRecords);
            CreateAddins();
            _initialized = true;

            return true;
        }

        // @return value: whether new or updated addins found.
        ResolutionResult ParseAndResolveAddins(ScanFilePackResult scanFilePackResult)
        {
            //var domainName = "addin";
            //var dmManager = new DomainManager();
            //var proxy = dmManager.CreateMarshalObject<AddinResolverProxy>(domainName);
            var proxy = new AddinResolverProxy();
            var result = proxy.Resolve(_addinFramework.NameConvention, _addinFramework.FileSettings, _addinFramework.AssemblyLoadPolicy, _addinStorage, _addinRelationManager, scanFilePackResult);
            //dmManager.UnloadDomain(domainName);
            return result;
        }

        // tries to get addin files that needed to be scaned or re-scaned (manifest or any of assembly files updated).
        // excluding: those files that have been scanned the last time, if they are unchanged.
        ScanFilePackResult GetScanFilePacks(AddinStorage addinStorage)
        {
            if (addinStorage == null || (addinStorage.AddinRecordCount == 0 && addinStorage.InvalidAddinFilePackCount == 0))
                return FilePackService.GetScanFilePackResult(_addinFramework.FileSettings, null);
            var existingFilePacks = GetExistingAddinFilePacks(addinStorage);
            return FilePackService.GetScanFilePackResult(_addinFramework.FileSettings, existingFilePacks);
        }

        IEnumerable<AddinFilePack> GetExistingAddinFilePacks(AddinStorage addinStorage)
        {
            var existingFilePacks = new List<AddinFilePack>(addinStorage.AddinRecordCount + addinStorage.InvalidAddinFilePackCount);
            if (addinStorage.AddinRecordCount > 0)
            {
                foreach (var addin in addinStorage.AddinRecords)
                    existingFilePacks.Add(addin.AddinFilePack);
            }
            if (addinStorage.InvalidAddinFilePackCount > 0)
                existingFilePacks.AddRange(addinStorage.InvalidAddinFilePacks);

            return existingFilePacks;
        }

        // 创建所有插件，无论它们是否被禁用
        void CreateAddins()
        {
            foreach (var addinRecord in _addinStorage.AddinRecords)
                GetOrCreateAddin(addinRecord);
        }
    }
}
