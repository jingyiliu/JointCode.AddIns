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
    public class FileConfiguration //: MarshalByRefObject
    {
        public const string DefaultAddinProbeDirectory = "AddIns";
        public const string DefaultAddinDataDirectory = "AddinData";
        public const string DefaultManifestFile = "addin.manifest";
        const string DefaultAddinInstallationDirectory = "Installation";
        const string DefaultAddinUpdateDirectory = "Update";
        const string DefaultPersistentFile = "addins.db";
        const string DefaultTransactionFile = "addins.transaction";

        //string _logFile;
        string _dataDir, _installationDir, _updateDir;
        string _persistentFile, _transactionFile;
        readonly string _manifestFile;
        readonly List<string> _probeDirectories = new List<string>();

        public FileConfiguration()
            : this(DefaultAddinDataDirectory, DefaultManifestFile, new [] { DefaultAddinProbeDirectory }) { }
        public FileConfiguration(string addinDataDir, string manifestFile, params string[] addinProbeDirs)
        {
            _manifestFile = manifestFile;
            Initialize(SystemHelper.AppName, SystemHelper.AppDirectory, addinDataDir, addinProbeDirs);
        }

        /// <summary>
        /// Gets the addin probe directories. 
        /// If a probe path is relative, then it is relative to the application install location.
        /// </summary>
        /// <remarks>For example, an application is installed to "C:/Program Files/MyApplication/", a probe directory of this 
        /// application is "Addins", an addin is installed to "MyAddin" directory, then the path to the addin directory
        /// will be "C:/Program Files/MyApplication/Addins/MyAddin/"</remarks>
        public IEnumerable<string> ProbeDirectories
        {
            get { return _probeDirectories; }
        }

        //插件数据目录，通过构造函数的 addinDataDir 参数指定。未指定时，默认为 DefaultAddinDataDirectory
        //可能位于应用程序目录中（使用相对路径）、用户指定的其他位置（使用绝对路径），或者用户个人文件夹中（如果不指定）
        public string DataDirectory
        {
            get { return _dataDir; }
        }

        //用于存储等待安装的插件安装文件的目录，位于插件数据目录中。
        public string InstallationDirectory
        {
            get { return _installationDir; }
        }

        //用于存储等待安装的插件更新文件的目录，位于插件数据目录中。
        public string UpdateDirectory
        {
            get { return _updateDir; }
        }

        //插件数据的持久化文件，位于插件数据目录中。
        public string PersistentFile
        {
            get { return _persistentFile; }
        }
        
        //插件数据的事务日志文件，位于插件数据目录中。
        public string TransactionFile
        {
            get { return _transactionFile; }
        }

        // 插件清单文件名
        public string ManifestFile
        {
            get { return _manifestFile; }
        }

        /// <summary>
        /// Initializes the file system of Addin framework.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="appDirectory">The application directory.</param>
        /// <param name="adnDataDirectory">The registry directory.
        /// If it is null, then it will use ApplicationData + EntryAssembly name;
        /// If it is relative, then it will be relative to the application directory.</param>
        /// <param name="addinProbeDirs">The addin probe directories.
        /// If the path is relative, then it is relative to the registry directory.</param>
        void Initialize(string appName, string appDirectory, string adnDataDirectory, string[] addinProbeDirs)
        {
            if (addinProbeDirs != null &&
                (addinProbeDirs.Length == 0 || (addinProbeDirs.Length == 1 && addinProbeDirs[0].IsNullOrWhiteSpace())))
                _probeDirectories.Add(Path.Combine(appDirectory, DefaultAddinProbeDirectory));
            else
                AddProbeDirectories(addinProbeDirs);

            //What if the specified AddinDataDirectory or required directories already exists?
            if (adnDataDirectory.IsNullOrWhiteSpace())
            {
                //如果 adnDataDirectory 为空，则 AddinDataDirectory 为用户个人文件夹
                adnDataDirectory = Path.Combine(Environment.SystemDirectory, Environment.UserName);
                adnDataDirectory = Path.Combine(adnDataDirectory, appName);
                _dataDir = Path.Combine(adnDataDirectory, DefaultAddinDataDirectory);
            }
            else if (!Path.IsPathRooted(adnDataDirectory))
            {
                //如果 adnDataDirectory 为相对路径
                _dataDir = Path.Combine(appDirectory, adnDataDirectory);
            }
            else
            {
                //如果 adnDataDirectory 为绝对路径
                _dataDir = adnDataDirectory;
            }

            _installationDir = Path.Combine(_dataDir, DefaultAddinInstallationDirectory);
            _updateDir = Path.Combine(_dataDir, DefaultAddinUpdateDirectory);

            _persistentFile = Path.Combine(_dataDir, DefaultPersistentFile);
            _transactionFile = Path.Combine(_dataDir, DefaultTransactionFile);
        }

        internal void AddProbeDirectories(string[] addinProbeDirs)
        {
            foreach (var adnProbeDir in addinProbeDirs)
            {
                if (adnProbeDir == null)
                    continue;

                string probeDirectory;
                if (Path.IsPathRooted(adnProbeDir) &&
                    adnProbeDir.StartsWith(SystemHelper.AppDirectory, StringComparison.InvariantCultureIgnoreCase))
                    probeDirectory = adnProbeDir.Substring(SystemHelper.AppDirectory.Length);
                else
                    probeDirectory = adnProbeDir;

                if (!_probeDirectories.Contains(probeDirectory))
                    _probeDirectories.Add(probeDirectory);
            }

            CheckProbeDirectories(addinProbeDirs);
        }

        //a probe directory can not be a subdirectory of another probe directory.
        static void CheckProbeDirectories(string[] addinProbeDirs)
        {
            if (addinProbeDirs.Length == 1)
                return;

            foreach (var adnProbeDir in addinProbeDirs)
            {
                foreach (var adnProbeDir2 in addinProbeDirs)
                {
                    if (adnProbeDir.StartsWith(adnProbeDir2))
                        throw new InvalidDataException
                            (string.Format("A probe directory can not be a subdirectory of another probe directory! The direcoty {0} is a subdirectory of {1}, which has been specified as a probe directory!", adnProbeDir, adnProbeDir2));
                }
            }
        }
    }
}
