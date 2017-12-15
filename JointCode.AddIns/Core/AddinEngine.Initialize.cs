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
using System.IO;
using System.Reflection;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.AddIns.Resolving;

namespace JointCode.AddIns.Core
{
    partial class AddinEngine
    {
        bool _initialized = false;

        /// <summary>
        /// Initializes the specified should refresh.
        /// </summary>
        /// <param name="shouldRefresh">
        /// if set to <c>true</c>, scan the addin probe directories for new and updated addins every time the application starts up.
        /// otherwise, initialize the addin framework directly.
        /// </param>
        public void Initialize(bool shouldRefresh)
        {
            if (_initialized)
                return;

            var storage = CreateStorage(_adnConfig.FileConfiguration);
            _indexManager = new IndexManager { Storage = storage };
            _bodyRepo = new BodyRepository { Storage = storage };
            var hasExistingAddins = _indexManager.Read();

            // remove addins waiting for delete.
            ProcessPendingAddins();

            if (shouldRefresh || !hasExistingAddins)
            {
                // get addin file packs.
                // excluding: those files that have been scanned last time, if they are unchanged
                var filePackResult = GetFilePackResult();
                if (filePackResult != null)
                {
                    //storage.Close(); // close the persistence file first
                    if (BuildDatabase(filePackResult))
                    {
                        storage = StorageHelper.CreateStorage
                            (_adnConfig.FileConfiguration.PersistentFile, _adnConfig.FileConfiguration.TransactionFile);
                        _indexManager.Storage = storage;
                        _bodyRepo.Storage = storage;
                        hasExistingAddins = _indexManager.Read();
                        if (hasExistingAddins)
                            _indexManager.Build();
                        _bodyRepo.ResetCache();
                    }
                }
                else if (hasExistingAddins)
                {
                    _indexManager.Build();
                }
            }
            else
            {
                _indexManager.Build();
            }

            _initialized = true;
        }

        static Storage.Storage CreateStorage(FileConfiguration fileConfig)
        {
            var persistentFile = fileConfig.PersistentFile;
            var transactionFile = fileConfig.TransactionFile;

            if (!File.Exists(persistentFile))
                IoHelper.CreateFile(fileConfig.DataDirectory, persistentFile);

            if (!File.Exists(transactionFile))
                IoHelper.CreateFile(fileConfig.DataDirectory, transactionFile);

            return StorageHelper.CreateStorage(persistentFile, transactionFile);
        }

        // @return value: whether new or updated addins found
        bool BuildDatabase(FilePackResult filePackResult)
        {
            var domainName = "addin";
            var dmManager = new DomainManager();
            var proxy = dmManager.CreateMarshalObject<AddinResolverProxy>(domainName);
            var result = proxy.Resolve(_adnConfig.MessageDialog, filePackResult,
                _adnConfig.FileConfiguration.PersistentFile, _adnConfig.FileConfiguration.TransactionFile);
            dmManager.UnloadDomain(domainName);
            return result;
        }

        FilePackResult GetFilePackResult()
        {
            if (_indexManager.AddinCount == 0 && _indexManager.InvalidAddinFilePackCount == 0)
                return FilePackService.GetFilePackResult(_adnConfig.FileConfiguration, null);

            var addinFilePacks = new List<AddinFilePack>(_indexManager.AddinCount + _indexManager.InvalidAddinFilePackCount);
            if (_indexManager.AddinCount > 0)
            {
                foreach (var addin in _indexManager.Addins)
                    addinFilePacks.Add(addin.AddinFilePack);
            }
            if (_indexManager.InvalidAddinFilePackCount > 0)
                addinFilePacks.AddRange(_indexManager.InvalidAddinFilePacks);
            
            return FilePackService.GetFilePackResult(_adnConfig.FileConfiguration, addinFilePacks);
        }

        // delete addins whose status is set to uninstalled, and update those updated addins
        void ProcessPendingAddins()
        {
            if (_indexManager.AddinCount == 0)
                return;

            List<AddinIndexRecord> uninstalledAddins; //addins that will be deleted
            List<AddinIndexRecord> updatedAddins; //addins that has been updated
            ReadPendingAddins(out uninstalledAddins, out updatedAddins);
            
            if (uninstalledAddins != null)
                UninstallPendingAddins(uninstalledAddins);

            if (updatedAddins != null)
                UpdatePendingAddins(updatedAddins);
        }

        void ReadPendingAddins(out List<AddinIndexRecord> uninstalledAddins, out List<AddinIndexRecord> updatedAddins)
        {
            uninstalledAddins = null;
            updatedAddins = null;

            foreach (var adnIndexRecord in _indexManager.Addins)
            {
                switch (adnIndexRecord.RunningStatus)
                {
                    case AddinRunningStatus.Uninstalled:
                        uninstalledAddins = uninstalledAddins ?? new List<AddinIndexRecord>();
                        uninstalledAddins.Add(adnIndexRecord);
                        break;
                    case AddinRunningStatus.Updated:
                        updatedAddins = updatedAddins ?? new List<AddinIndexRecord>();
                        updatedAddins.Add(adnIndexRecord);
                        break;
                    case AddinRunningStatus.Enabled: 
                    case AddinRunningStatus.Disabled:
                        break;
                    case AddinRunningStatus.Default:
                        throw new Exception("The addin status could not be Default!");
                    default:
                        throw new Exception("The addin status is out of range!");
                }
            }
        }

        // uninstall addins that is marked to be unloaded last time by:
        // 1. delete the addin files from the hard disk directly.
        // 2. remove the addin assets from the persistent file.
        void UninstallPendingAddins(List<AddinIndexRecord> uninstalledAddins)
        {
            var loadedAssms = GetLoadedAssemblies();
            foreach (var uninstalledAddin in uninstalledAddins) 
            {
            	// if any assembly of the addin has been loaded, it can not be uninstalled.
            }
        }

        List<string> GetLoadedAssemblies()
        {
            var assemblyPaths = new List<string>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm is System.Reflection.Emit.AssemblyBuilder)
                    continue; // skip dynamically built assemblies
                try
                {
                    Uri u;
                    if (!Uri.TryCreate(asm.CodeBase, UriKind.Absolute, out u))
                        continue;
                    assemblyPaths.Add(u.LocalPath);
                }
                catch
                { } 
            }
            return assemblyPaths;
        }

        // update addins that is marked to have updated version last time by:
        // 1. remove the old addin files from the addin directory.
        // 2. copy the updated file to the addin directory.
        // 3. resolve the updated addin.
        // 4. update the updated assets in the persistent file, or remove the old assets from it.
        void UpdatePendingAddins(List<AddinIndexRecord> updatedAddins)
        {
            //copy the updated Addin files to the right location

            //set the status of updated Addins to Enabled or Disabled accordingly
        }
    }
}
