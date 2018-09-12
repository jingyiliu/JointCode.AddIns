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

        // 获取指定插件探测 (Addin Probing) 目录下的可扫描插件文件
        internal static ScanFilePack GetScanFilePack(AddinFileSettings addinFileConfig, IEnumerable<AddinFilePack> existingFilePacks, 
            string addinProbingPath, string addinDirectory)
        {
            ValidateAddinProbingDirectory(addinFileConfig, addinProbingPath);
            return null;
        }

        static void ValidateAddinProbingDirectory(AddinFileSettings addinFileConfig, string addinProbingPath)
        {
            var match = false;
            foreach (var probingDirectory in addinFileConfig.AddinProbingDirectories)
            {
                if (probingDirectory.Equals(addinProbingPath, StringComparison.InvariantCultureIgnoreCase))
                    match = true;
            }

            if (!match)
                throw new InvalidOperationException("");
        }

        // 获取所有插件探测 (Addin Probing) 目录下的所有可扫描插件文件（新增或更改的文件）
        internal static ScanFilePackResult GetScanFilePackResult(AddinFileSettings addinFileSettings, IEnumerable<AddinFilePack> existingFilePacks)
        {
            var result = new ScanFilePackResult();
            var appDir = SystemHelper.AppDirectory;

            if (addinFileSettings.IndividualAddinDirectories != null)
            {
                foreach (var individualAddinDir in addinFileSettings.IndividualAddinDirectories)
                    GetScanFilePack(individualAddinDir, string.Empty, existingFilePacks, ref result);
            }

            foreach (var probingDirectory in addinFileSettings.AddinProbingDirectories)
                GetScanFilePacks(appDir, probingDirectory, existingFilePacks, ref result);

            if (result.ScanFilePacks == null || result.ScanFilePackCount == 0)
                return null;

            GetApplicationAssemblies(appDir, result);
            return result;
        }

        static void GetScanFilePacks(string appDir, string probingDir, 
            IEnumerable<AddinFilePack> existingFilePacks, ref ScanFilePackResult scanFilePackResult)
        {
            //Logger.Info(string.Format("Scanning directory [{0}]...", directory));
            if (!Path.IsPathRooted(probingDir)) //如果该探测路径是相对路径
                probingDir = Path.Combine(appDir, probingDir);

            if (!Directory.Exists(probingDir))
                return;

            //探测路径下的所有文件夹都被视为潜在的插件文件夹
            var addinDirectories = Directory.GetDirectories(probingDir);
            foreach (var addinDirectory in addinDirectories)
                GetScanFilePack(addinDirectory, probingDir, existingFilePacks, ref scanFilePackResult);
        }

        static void GetScanFilePack(string addinDirectory, string probingDir,
            IEnumerable<AddinFilePack> existingFilePacks, ref ScanFilePackResult scanFilePackResult)
        {
            ScanFilePack scanFilePack = null;
            var matchingFilePack = existingFilePacks != null ? GetMatchingFilePack(addinDirectory, existingFilePacks) : null;
            foreach (var fileScanner in _fileScanners)
            {
                scanFilePack = fileScanner.ScanForFilePack(probingDir, addinDirectory, AddinFileSettings.ManifestFileName, matchingFilePack);
                if (scanFilePack != null)
                    break;
            }
            if (scanFilePack != null)
                scanFilePackResult.AddScanFilePack(scanFilePack);
        }

        static AddinFilePack GetMatchingFilePack(string addinDirectory, IEnumerable<AddinFilePack> existingFilePacks)
        {
            foreach (var matchingFilePack in existingFilePacks)
            {
                if (addinDirectory.Equals(matchingFilePack.BaseDirectory, StringComparison.InvariantCultureIgnoreCase))
                    return matchingFilePack;
            }
            return null;
        }

        // 获取应用程序目录下所有程序集（排除 JointCode.Addins.dll 自身）
        static void GetApplicationAssemblies(string appDir, ScanFilePackResult scanFilePackResult)
        {
            //Logger.Info(string.Format("Scanning directory [{0}]...", directory));
            var files = Directory.GetFiles(appDir);
            foreach (var file in files)
            {
                if (FileScanner.IsAssembly(file) && !file.EndsWith(FileScanner.ThisAssemblyName, StringComparison.InvariantCultureIgnoreCase))
                    scanFilePackResult.AddApplicationAssembly(file);
            }
        }
    }
}