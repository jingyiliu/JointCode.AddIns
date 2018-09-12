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

namespace JointCode.AddIns.Core.FileScanning
{
    /// <summary>
    /// Represent all files of an addin.
    /// </summary>
    //[Serializable]
    class ScanFilePack
    {
        List<string> _assemblyFiles;
        List<string> _dataFiles;

        //插件探测目录，可能是绝对路径，也可能是相对于应用程序目录的相对路径
        internal string AddinProbingDirectory { get; set; }

        //插件目录，相对于插件探测目录。
        //在插件安装时，该目录下的所有文件都将被视为插件文件。在插件卸载时，这些文件也将被删除。
        internal string AddinDirectory { get; set; }

        internal string ManifestFile { get; set; }

        //插件程序集（包含公共和专有程序集）
        //位于插件目录中的文件。插件配置文件 (xml/manifiest) 和所有插件程序集都必须位于此目录下。
        internal IEnumerable<string> AssemblyFiles
        {
            get { return _assemblyFiles; }
        }

        //位于插件目录下的任何子目录中的文件。
        //此处的文件路径为绝对路径。
        internal IEnumerable<string> DataFiles
        {
            get { return _dataFiles; }
        }

        internal void AddAssemblyFile(string assemblyFile)
        {
        	_assemblyFiles = _assemblyFiles ?? new List<string>();
            _assemblyFiles.Add(assemblyFile);
        }

        internal void AddDataFile(string dataFile)
        {
        	_dataFiles = _dataFiles ?? new List<string>();
            _dataFiles.Add(dataFile);
        }
    }
}