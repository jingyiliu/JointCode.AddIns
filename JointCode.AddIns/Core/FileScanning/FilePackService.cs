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
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core.FileScanning
{
    static class FilePackService
    {
        static readonly List<FileScanner> _fileScanners = new List<FileScanner>();

        static FilePackService() { _fileScanners.Add(new XmlBasedFileScanner()); }

        internal static FilePackResult GetFilePackResult(FileConfiguration fileConfig, IEnumerable<AddinFilePack> addinFilePacks)
        {
            var result = new FilePackResult();
            var appDir = SystemHelper.AppDirectory;

            foreach (var probeDirectory in fileConfig.ProbeDirectories)
                GetScannableFilePacks(fileConfig, appDir, probeDirectory, addinFilePacks, ref result);

            if (result.AddinFilePacks == null || result.AddinFilePackCount == 0)
                return null;

            GetApplicationAssemblies(appDir, result);
            return result;
        }

        static void GetScannableFilePacks(FileConfiguration fileConfig, string appDir, string probeDir, 
            IEnumerable<AddinFilePack> addinFilePacks, ref FilePackResult filePackResult)
        {
            //Logger.Info(string.Format("Scanning directory [{0}]...", directory));
            if (!Path.IsPathRooted(probeDir)) //如果该探测路径是相对路径
                probeDir = Path.Combine(appDir, probeDir);

            if (!Directory.Exists(probeDir))
                return;

            //探测路径下的所有文件夹都被视为潜在的插件文件夹
            var addinDirectories = Directory.GetDirectories(probeDir);
            foreach (var addinDirectory in addinDirectories)
            {
                FilePack filePack = null;
                var addinFilePack = addinFilePacks != null ? GetMatchingFilePack(addinDirectory, addinFilePacks) : null;
                foreach (var fileScanner in _fileScanners)
                {
                    filePack = fileScanner.GetFilePack(probeDir, addinDirectory, fileConfig.ManifestFile, addinFilePack);
                    if (filePack != null)
                        break;
                }
                if (filePack != null)
                    filePackResult.AddAddinFilePack(filePack);
            }
        }

        static AddinFilePack GetMatchingFilePack(string addinDirectory, IEnumerable<AddinFilePack> addinFilePacks)
        {
            foreach (var addinFilePack in addinFilePacks)
            {
                if (addinDirectory.Equals(addinFilePack.AddinDirectory, StringComparison.InvariantCultureIgnoreCase))
                    return addinFilePack;
            }
            return null;
        }

        static void GetApplicationAssemblies(string appDir, FilePackResult filePackResult)
        {
            //Logger.Info(string.Format("Scanning directory [{0}]...", directory));
            var files = Directory.GetFiles(appDir);
            foreach (var file in files)
            {
                if (FileScanner.IsAssembly(file))
                    filePackResult.AddApplicationAssembly(file);
            }
        }
    }
}