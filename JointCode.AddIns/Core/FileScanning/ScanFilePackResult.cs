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
    //[Serializable]
    class ScanFilePackResult
    {
        List<string> _applicationAssemblies; 
        List<ScanFilePack> _scanFilePacks;

        internal int ApplicationAssemblyCount { get { return _applicationAssemblies == null ? 0 : _applicationAssemblies.Count; } }

        //应用程序本身自带的程序集，这些程序集将向所有插件公开
        internal IEnumerable<string> ApplicationAssemblies {  get { return _applicationAssemblies; } }

        internal int ScanFilePackCount { get { return _scanFilePacks == null ? 0 : _scanFilePacks.Count; } }

        internal IEnumerable<ScanFilePack> ScanFilePacks { get { return _scanFilePacks; } }

        internal void AddApplicationAssembly(string applicationAssembly)
        {
            _applicationAssemblies = _applicationAssemblies ?? new List<string>();
            _applicationAssemblies.Add(applicationAssembly);
        }

        internal void AddScanFilePack(ScanFilePack scanFilePack)
        {
            _scanFilePacks = _scanFilePacks ?? new List<ScanFilePack>();
            _scanFilePacks.Add(scanFilePack);
        }
    }
}
