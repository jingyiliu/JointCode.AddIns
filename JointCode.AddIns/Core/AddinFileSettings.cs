//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using System.Collections.Generic;
using JointCode.Common.Extensions;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core
{
    //[Serializable]
    public class AddinFileSettings //: MarshalByRefObject
    {
        public const string ManifestFileName = "addin.manifest";
        public const string LogFileName = "addins.log";
        public const string DefaultStorageFileName = "addins.db";

        public const string DefaultAddinProbingDirectory = "AddIns";
        public const string DefaultAddinDataDirectory = "AddinData";
        const string DefaultAddinInstallationDirectory = "Installation";
        //const string DefaultAddinUpdateDirectory = "Update";

        string _dataDir, _installationDir;
        string _storageFilePath;
        string[] _addinProbingDirectories, _individualAddinDirectories;

        public AddinFileSettings()
            : this(DefaultAddinDataDirectory, null, new [] { DefaultAddinProbingDirectory }) { }

        /// <summary>
        /// Create an instance of <see cref="AddinFileSettings"/>
        /// </summary>
        /// <param name="addinDataDirectory">
        /// The directory used to store the addin associated data files. 
        /// If this value is null or empty, it will be set to the <see cref="AddinFileSettings.DefaultAddinDataDirectory"/> under the application base directory.
        /// If the path is relative, it is relative to the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>, which is usually the application base directory.
        /// </param>
        /// <param name="individualAddinDirectories">
        /// Individual directories that the addin scanner used to find the addins.
        /// If the path is relative, it is relative to the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>, which is usually the application base directory.
        /// </param>
        /// <param name="addinProbingDirectories">
        /// Directories where the addin scanner used to probe for the addins. 
        /// For example, if an addin probing directory is 'AddIns', then every subdirectory of it will be treated as a possible addin directory, i.e, if an addin 
        /// manifest file is found there, and the manifest is valid, then it's an addin. 
        /// Notes that the addin scanning is not recursivley, it means that the subdirectories of the subdirectory will not be searched for addins.
        /// If the path is relative, it is relative to the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>, which is usually the application base directory.
        /// </param>
        public AddinFileSettings(string addinDataDirectory, string[] individualAddinDirectories, string[] addinProbingDirectories)
        {
            Initialize(SystemHelper.AppDirectory, addinDataDirectory, individualAddinDirectories, addinProbingDirectories);
        }

        /// <summary>
        /// Gets the directories that the addin scanner used to find the addins.
        /// </summary>
        public string[] IndividualAddinDirectories { get { return _individualAddinDirectories; } }

        /// <summary>
        /// Gets the directories where the addin scanner used to probe for the addins. 
        /// </summary>
        /// <remarks>For example, an application is installed to "C:/Program Files/MyApplication/", a probing directory of this 
        /// application is "Addins", an addin is installed to "MyAddin" directory, then the path to the addin directory
        /// will be "C:/Program Files/MyApplication/Addins/MyAddin/"</remarks>
        public string[] AddinProbingDirectories { get { return _addinProbingDirectories; } }

        //插件数据目录，通过构造函数的 addinDataDir 参数指定。未指定时，默认为 DefaultAddinDataDirectory
        //可能位于应用程序目录中（使用相对路径）、用户指定的其他位置（使用绝对路径），或者用户个人文件夹中（如果不指定）
        public string DataDirectory { get { return _dataDir; } }

        //用于存储等待安装的插件安装文件的目录，位于插件数据目录中。
        public string InstallationDirectory { get { return _installationDir; } }

        //插件数据的持久化文件，位于插件数据目录中。
        public string StorageFilePath { get { return _storageFilePath; } }

        ////用于存储等待安装的插件更新文件的目录，位于插件数据目录中。
        //public string UpdateDirectory { get { return _updateDir; } }

        void Initialize(string appDirectory, string addinDataDirectory, string[] individualAddinDirs, string[] addinProbingDirs)
        {
            //What if the specified AddinDataDirectory or required directories already exists?
            if (addinDataDirectory.IsNullOrWhiteSpace())
            {
                ////如果 adnDataDirectory 为空，则 AddinDataDirectory 为用户个人文件夹
                //adnDataDirectory = Path.Combine(Environment.SystemDirectory, Environment.UserName);
                //adnDataDirectory = Path.Combine(adnDataDirectory, appName);
                _dataDir = Path.Combine(appDirectory, DefaultAddinDataDirectory);
            }
            else if (!Path.IsPathRooted(addinDataDirectory))
            {
                //如果 adnDataDirectory 为相对路径
                _dataDir = Path.Combine(appDirectory, addinDataDirectory);
            }
            else
            {
                //如果 adnDataDirectory 为绝对路径
                _dataDir = addinDataDirectory;
            }

            _storageFilePath = Path.Combine(_dataDir, DefaultStorageFileName);
            //_transactionFile = Path.Combine(_dataDir, DefaultTransactionFile);
            _installationDir = Path.Combine(_dataDir, DefaultAddinInstallationDirectory);
            //_updateDir = Path.Combine(_dataDir, DefaultAddinUpdateDirectory);

            _addinProbingDirectories = addinProbingDirs == null || addinProbingDirs.Length == 0
                ? new[] {Path.Combine(appDirectory, DefaultAddinProbingDirectory)}
                : AddProbingDirectories(appDirectory, addinProbingDirs).ToArray();

            _individualAddinDirectories = individualAddinDirs == null || individualAddinDirs.Length == 0
                ? null
                : AddDirectories(appDirectory, individualAddinDirs, "addin", true).ToArray();
        }

        List<string> AddProbingDirectories(string appDirectory, string[] addinProbingDirs)
        {
            var result = new List<string>();
            foreach (var adnProbingDir in addinProbingDirs)
            {
                if (adnProbingDir.IsNullOrWhiteSpace())
                    continue;

                var probingDirectory = Path.IsPathRooted(adnProbingDir)
                    ? adnProbingDir
                    : Path.Combine(appDirectory, adnProbingDir);

                if (!result.Contains(probingDirectory))
                    result.Add(probingDirectory);
            }

            CheckDirectories(addinProbingDirs, "addin probing", true);

            if (result.Count == 0)
                result.Add(Path.Combine(appDirectory, DefaultAddinDataDirectory));

            return result;
        }

        List<string> AddDirectories(string appDirectory, string[] dirs, string dirName, bool useAn)
        {
            var result = new List<string>();
            foreach (var dir in dirs)
            {
                if (dir.IsNullOrWhiteSpace())
                    continue;

                var directory = Path.IsPathRooted(dir)
                    ? dir
                    : Path.Combine(appDirectory, dir);

                if (!result.Contains(directory))
                    result.Add(directory);
            }

            CheckDirectories(dirs, dirName, useAn);

            return result.Count > 0 ? result : null;
        }

        //a directory can not be a subdirectory of another directory.
        static void CheckDirectories(string[] dirs, string dirType, bool useAn)
        {
            if (dirs.Length == 1)
                return;
            foreach (var dir in dirs)
            {
                foreach (var dir2 in dirs)
                {
                    if (ReferenceEquals(dir, dir2))
                        continue;
                    if (dir.StartsWith(dir2))
                        throw new InvalidDataException
                            (string.Format("{3} {2} directory can not be a subdirectory of another {2} directory! The direcoty {0} is a subdirectory of {1}, which has been specified as {4} {2} directory!", dir, dir2, dirType, useAn ? "An" : "A", useAn ? "an" : "a"));
                }
            }
        }
    }
}
