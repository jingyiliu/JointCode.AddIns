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
    [Serializable]
    class FilePackResult
    {
        List<string> _applicationAssemblies; 
        List<FilePack> _addinFilePacks;

        internal int ApplicationAssemblyCount { get { return _addinFilePacks == null ? 0 : _addinFilePacks.Count; } }

        //应用程序本身自带的程序集，这些程序集将向所有插件公开
        internal IEnumerable<string> ApplicationAssemblies
        {
            get { return _applicationAssemblies; }
        }

        internal int AddinFilePackCount { get { return _addinFilePacks == null ? 0 : _addinFilePacks.Count; } }

        internal IEnumerable<FilePack> AddinFilePacks
        {
            get { return _addinFilePacks; }
        }

        internal void AddApplicationAssembly(string applicationAssembly)
        {
            _applicationAssemblies = _applicationAssemblies ?? new List<string>();
            _applicationAssemblies.Add(applicationAssembly);
        }

        internal void AddAddinFilePack(FilePack addinFilePack)
        {
            _addinFilePacks = _addinFilePacks ?? new List<FilePack>();
            _addinFilePacks.Add(addinFilePack);
        }
    }
}
