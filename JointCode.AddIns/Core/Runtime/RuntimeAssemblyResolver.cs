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
using System.Globalization;
using System.Reflection;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Core.Runtime
{
    partial class RuntimeAssemblyResolver
    {
    	class RuntimeAssemblyKey : AssemblyKey
	    {
            private RuntimeAssemblyKey(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken) 
                : base(name, version, cultrue, publicKeyToken) { }
	    	internal static RuntimeAssemblyKey Create(AssemblyName assemblyName)
	        {
                return new RuntimeAssemblyKey(assemblyName.Name, assemblyName.Version, assemblyName.CultureInfo, assemblyName.GetPublicKeyToken());
	    	}
            internal static RuntimeAssemblyKey Create(string assemblyName)
	        {
	            try
	            {
	                var aName = new AssemblyName(assemblyName);
                    return new RuntimeAssemblyKey(aName.Name, aName.Version, aName.CultureInfo, aName.GetPublicKeyToken());
	            }
	            catch (Exception ex)
	            {
	                throw new ArgumentException(assemblyName, ex);
	            }
	        }
	    }

        // Different addins might provide a same assembly (assemblies with same name and version), 
        // but only one of them will be loaded into memory at runtime.
        class RuntimeAssemblySet : List<RuntimeAssembly> { }
    }

    partial class RuntimeAssemblyResolver
    {
        readonly Dictionary<AssemblyKey, RuntimeAssemblySet> _name2AssemblySets;
        readonly Dictionary<int, RuntimeAssemblySet> _uid2AssemblySets;

        internal RuntimeAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            _name2AssemblySets = new Dictionary<AssemblyKey, RuntimeAssemblySet>(new AssemblyKeyEqualityComparer());
            _uid2AssemblySets = new Dictionary<int, RuntimeAssemblySet>();
        }

        internal Assembly GetLoadedAssembly(int uid)
        {
            RuntimeAssemblySet assemblySet;
            return _uid2AssemblySets.TryGetValue(uid, out assemblySet) ? GetLoadedAssembly(assemblySet) : null;
        }

        internal void RegisterAssemblies(string addinDirectory, IEnumerable<AssemblyFileRecord> assemblies)
        {
            if (assemblies == null)
                return;

            foreach (var assembly in assemblies)
            {
                if (assembly.Uid == UidProvider.InvalidAssemblyUid)
                    throw new InvalidOperationException();

                assembly.SetDirectory(addinDirectory);
            	AssemblyName assemblyName;
            	try 
            	{
                    assemblyName = AssemblyName.GetAssemblyName(assembly.FullPath);
            	} 
            	catch
            	{
            		continue;
            	}

                var assemblyKey = RuntimeAssemblyKey.Create(assemblyName);
                RuntimeAssemblySet assemblySet;
                if (!_uid2AssemblySets.TryGetValue(assembly.Uid, out assemblySet))
                {
                    assemblySet = new RuntimeAssemblySet();
                    _uid2AssemblySets.Add(assembly.Uid, assemblySet);
                    _name2AssemblySets.Add(assemblyKey, assemblySet);
                }
                var asm = new RuntimeAssembly(assemblyKey, assembly);
                assemblySet.Add(asm);
            }
        }

        internal void UnregisterAssemblies(IEnumerable<AssemblyFileRecord> assemblies)
        {
            if (assemblies == null)
                return;

            foreach (var assembly in assemblies)
            {
                if (assembly.Uid == UidProvider.InvalidAssemblyUid)
                    throw new InvalidOperationException();

                RuntimeAssemblySet assemblySet;
                if (!_uid2AssemblySets.TryGetValue(assembly.Uid, out assemblySet)) 
            		continue;

                RuntimeAssembly runtimeAssembly = null;
                for (int i = 0; i < assemblySet.Count; i++)
                {
                    runtimeAssembly = assemblySet[i];
                    if (!ReferenceEquals(runtimeAssembly.AssemblyFile, assembly))
                        continue;
                    assemblySet.RemoveAt(i);
                    break;
                }
                if (assemblySet.Count != 0)
                    continue;

                if (runtimeAssembly != null)
                {
                    _name2AssemblySets.Remove(runtimeAssembly.AssemblyKey);
                    _uid2AssemblySets.Remove(runtimeAssembly.Uid);
                }
            }
        }

        Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyKey = RuntimeAssemblyKey.Create(args.Name);
            RuntimeAssemblySet assemblySet;
            return _name2AssemblySets.TryGetValue(assemblyKey, out assemblySet)
                ? GetLoadedAssembly(assemblySet)
                : null;
        }

        static Assembly GetLoadedAssembly(RuntimeAssemblySet assemblySet)
        {
            foreach (var assembly in assemblySet)
            {
                if (assembly.Loaded)
                    return assembly.Assembly;
            }
            return assemblySet[0].LoadAssembly();
        }
    }
}
