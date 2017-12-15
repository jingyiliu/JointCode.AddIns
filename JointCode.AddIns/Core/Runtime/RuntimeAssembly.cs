//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Reflection;
using System.Globalization;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Core.Runtime
{
    class RuntimeAssembly
    {
//        Version _compatVersion;
        Assembly _assembly;
        readonly AssemblyKey _assemblyKey;
        readonly AssemblyFileRecord _assemblyFile;

        internal RuntimeAssembly(AssemblyKey assemblyKey, AssemblyFileRecord assemblyFile)
        {
            _assemblyKey = assemblyKey;
            _assemblyFile = assemblyFile;
        }

        internal Assembly Assembly
        {
            get { return _assembly; }
        }

        internal int Uid
        {
            get { return _assemblyFile.Uid; }
        }

        internal string FullPath
        {
            get { return _assemblyFile.FullPath; }
        }

        internal bool Loaded
        {
            get { return _assembly != null; }
        }

        internal AssemblyKey AssemblyKey
        {
            get { return _assemblyKey; }
        }

        internal string Name
        {
            get { return _assemblyKey.Name; }
        }

        internal Version Version
        {
            get { return _assemblyKey.Version; }
        }

        internal CultureInfo CultureInfo
        {
            get { return _assemblyKey.CultureInfo; }
        }

        internal byte[] PublicKeyToken
        {
            get { return _assemblyKey.PublicKeyToken; }
        }

        internal AssemblyFileRecord AssemblyFile
        {
            get { return _assemblyFile; }
        }

        internal Assembly LoadAssembly()
        {
            if (_assembly != null)
                return _assembly;
            
            try
            {
                // tries to use different policies to load assembly (Load/LoadFrom/LoadFile) according to the @_assemblyFile?????
                _assembly = Assembly.LoadFile(FullPath);
            }
            catch
            {
                // log
            }

            return _assembly;
        }
    }
}
