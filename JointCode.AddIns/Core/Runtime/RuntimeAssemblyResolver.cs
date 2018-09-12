//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using JointCode.AddIns.Core.Storage;
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
        class RuntimeAssemblySet : IEnumerable<RuntimeAssembly>
        {
            Assembly _assembly;
            readonly List<RuntimeAssembly> _runtimeAssemblies = new List<RuntimeAssembly>();

            internal int Count { get { return _runtimeAssemblies.Count; } }

            internal RuntimeAssembly this[int index] { get { return _runtimeAssemblies[index]; } }

            //internal AssemblyKey AssemblyKey { get { return _runtimeAssemblies[0].AssemblyKey; } }

            internal void Add(RuntimeAssembly runtimeAssembly)
            {
                _runtimeAssemblies.Add(runtimeAssembly);
            }

            internal bool Remove(RuntimeAssembly runtimeAssembly)
            {
                //if (_runtimeAssemblies.Count > 1)
                //{
                //    var result = _runtimeAssemblies.Remove(runtimeAssembly);
                //    if (result && runtimeAssembly.Assembly != null)
                //    {
                //        for (int i = 0; i < _runtimeAssemblies.Count; i++)
                //            _runtimeAssemblies[i].SetLoadStatus(false);
                //    }
                //    return result;
                //}
                //else
                //{
                return _runtimeAssemblies.Remove(runtimeAssembly);
                //}
            }

            internal void RemoveAt(int index)
            {
                //if (_runtimeAssemblies.Count > 1)
                //{
                //    var runtimeAssembly = _runtimeAssemblies[index];
                //    _runtimeAssemblies.RemoveAt(index);
                //    if (runtimeAssembly.Assembly != null)
                //    {
                //        for (int i = 0; i < _runtimeAssemblies.Count; i++)
                //            _runtimeAssemblies[i].SetLoadStatus(false);
                //    }
                //}
                //else
                //{
                _runtimeAssemblies.RemoveAt(index);
                //}
            }

            internal Assembly LoadAssembly(AssemblyLoadPolicy assemblyLoadPolicy)
            {
                if (_assembly != null)
                    return _assembly;

                var selectedRuntimeAssembly = _runtimeAssemblies[0]; // use an assembly selecting policy ???
                _assembly = selectedRuntimeAssembly.Assembly ?? selectedRuntimeAssembly.LoadAssembly(assemblyLoadPolicy); 

                //if (_runtimeAssemblies.Count > 1)
                //{
                //    // mark the other assemblies as Loaded, because they are all the same assembly.
                //    for (int i = 1; i < _runtimeAssemblies.Count; i++)
                //        _runtimeAssemblies[i].SetLoadStatus(true);
                //}

                return _assembly;
            }

            public IEnumerator<RuntimeAssembly> GetEnumerator()
            {
                return _runtimeAssemblies.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    partial class RuntimeAssemblyResolver : IDisposable
    {
        Dictionary<int, RuntimeAssemblySet> _uid2AssemblySets;
        readonly Dictionary<AssemblyKey, RuntimeAssemblySet> _key2AssemblySets;
        readonly List<RuntimeAssembly> _unregisteredAssemblies;
        readonly AddinFramework _framework;

        internal RuntimeAssemblyResolver(AddinFramework framework)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            //AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad; // 当前应用程序域中有新程序集被加载时，将其加入 _key2AssemblySets 和 _uid2AssemblySets

            _framework = framework;
            _key2AssemblySets = new Dictionary<AssemblyKey, RuntimeAssemblySet>(AssemblyKey.EqualityComparer);
            _uid2AssemblySets = new Dictionary<int, RuntimeAssemblySet>();
            _unregisteredAssemblies = new List<RuntimeAssembly>();

            PrepareShadowCopyDirectoryIfNeccessary();
            AddAppDomainLoadedAssemblies();
        }

        // 将当前应用程序域中已加载的程序集加入 _key2AssemblySets，防止一个程序集被加载两次（因为使用了 Assembly.Load(byte[]) 和 Assembly.LoadFile 可能有这个问题），
        // 这样可能会在运行时出现一些意外的错误（例如相同类型的对象赋值会出现 InvalidCastException）。
        void AddAppDomainLoadedAssemblies()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                var key = RuntimeAssemblyKey.Create(asm.GetName()); 
                var runtimeAssembly = new AppRuntimeAssembly(key, asm);
                RuntimeAssemblySet runtimeAssemblies;
                if (!_key2AssemblySets.TryGetValue(key, out runtimeAssemblies))
                {
                    runtimeAssemblies = new RuntimeAssemblySet();
                    _key2AssemblySets.Add(key, runtimeAssemblies);
                }
                runtimeAssemblies.Add(runtimeAssembly);
            }
        }

        void PrepareShadowCopyDirectoryIfNeccessary()
        {
            if (!_framework.UseShadowCopy)
                return;
          
            var shadowCopyDir = new DirectoryInfo(_framework.ShadowCopyDirectory);
            if (!shadowCopyDir.Exists)
            {
                shadowCopyDir.Create();
            }
            else
            {
                //清空影子复制目录中的dll文件
                foreach (var fileInfo in shadowCopyDir.GetFiles())
                    fileInfo.Delete();
            }
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        internal void Reset()
        {
            _uid2AssemblySets = new Dictionary<int, RuntimeAssemblySet>();
        }

        //internal Assembly[] LoadedAssemblies { get { return null; } }

        internal Assembly GetOrLoadAssembly(AssemblyFileRecord fileRecord)
        {
            RuntimeAssemblySet runtimeAssemblies;
            return _uid2AssemblySets.TryGetValue(fileRecord.Uid, out runtimeAssemblies) ? runtimeAssemblies.LoadAssembly(_framework.AssemblyLoadPolicy) : null;
        }

        internal Assembly GetOrLoadAssemblyByUid(int uid)
        {
            RuntimeAssemblySet runtimeAssemblies;
            return _uid2AssemblySets.TryGetValue(uid, out runtimeAssemblies) ? runtimeAssemblies.LoadAssembly(_framework.AssemblyLoadPolicy) : null;
        }

        //// Gets assemblies for assembly files that has been already registered.
        //internal List<Assembly> LoadRegisteredAssemblies(IEnumerable<AssemblyFileRecord> assemblyFiles)
        //{
        //    if (assemblyFiles == null)
        //        return null;
        //    var result = new List<Assembly>();
        //    foreach (var assemblyFile in assemblyFiles)
        //    {
        //        var assembly = GetOrLoadAssemblyByUid(assemblyFile.Uid);
        //        if (assembly == null)
        //            throw new InvalidOperationException("");
        //        result.Add(assembly);
        //    }
        //    return result;
        //}

        //internal List<Assembly> RegisterAndLoadAssemblies(Addin addin, IEnumerable<AssemblyFileRecord> assemblyFiles)
        //{
        //    if (assemblyFiles == null)
        //        return null;
        //    var result = new List<Assembly>();
        //    DoRegisterAssemblies(addin, assemblyFiles, (runtimeAssemblies) => result.Add(runtimeAssemblies.LoadAssembly()));
        //    return result;
        //}

        internal void RegisterAssemblies(Addin addin, IEnumerable<AssemblyFileRecord> assemblyFiles)
        {
            if (assemblyFiles == null)
                return;
            DoRegisterAssemblies(addin, assemblyFiles);
        }

        void DoRegisterAssemblies(Addin addin, IEnumerable<AssemblyFileRecord> assemblyFiles)
        {
            if (_framework.UseShadowCopy)
            {
                // 先将插件程序集文件复制到阴影目录
                foreach (var assemblyFile in assemblyFiles)
                {
                    if (assemblyFile.Uid == UidStorage.InvalidAssemblyUid)
                        throw new InvalidOperationException();

                    assemblyFile.SetDirectory(addin.File.BaseDirectory);
                    assemblyFile.SetShadowCopyDirectory(_framework.ShadowCopyDirectory);

                    try
                    {
                        // 仅当文件不存在时才复制。
                        // 因为插件系统在运行过程中，可能会添加、升级或删除插件，如果有多个插件共用相同程序集，或者升级插件时有一部分程序集并未更改，
                        // 且该程序集已加载到内存，那么此时该程序集可能已被锁定，因此不能覆盖它。
                        if (!File.Exists(assemblyFile.LoadPath)) 
                            File.Copy(assemblyFile.FullPath, assemblyFile.LoadPath, false);
                    }
                    catch (Exception ex) // 在某些情况下会出现"正由另一进程使用，因此该进程无法访问该文件"错误，所以先重命名再复制
                    {
                        //try
                        //{
                        //    File.Move(assemblyFile.LoadPath, assemblyFile.LoadPath + Guid.NewGuid().ToString("N") + ".locked");
                        //}
                        //catch (Exception ex2)
                        //{
                        //    throw;
                        //}
                        //File.Copy(assemblyFile.FullPath, assemblyFile.LoadPath, false);
                        throw;
                    }
                }

                // 现在将插件程序集注册到 AssemblyResolver，等待应用程序在需要时加载它们
                foreach (var assemblyFile in assemblyFiles)
                    DoLoadAssembly(addin, assemblyFile);
            }
            else
            {
                foreach (var assemblyFile in assemblyFiles)
                {
                    if (assemblyFile.Uid == UidStorage.InvalidAssemblyUid)
                        throw new InvalidOperationException();
                    assemblyFile.SetDirectory(addin.File.BaseDirectory);
                    DoLoadAssembly(addin, assemblyFile);
                }
            }
        }

        void DoLoadAssembly(Addin addin, AssemblyFileRecord assemblyFile)
        {
            AssemblyName assemblyName;
            try
            {
                assemblyName = AssemblyName.GetAssemblyName(assemblyFile.LoadPath);
            }
            catch
            {
                return;
            }

            var assemblyKey = RuntimeAssemblyKey.Create(assemblyName);
            var runtimeAssemblies = GetOrAddRuntimeAssemblySet(assemblyKey, assemblyFile);
            var runtimeAssembly = GetOrCreateRuntimeAssembly(addin, assemblyKey, assemblyFile);
            runtimeAssemblies.Add(runtimeAssembly);
        }

        RuntimeAssemblySet GetOrAddRuntimeAssemblySet(RuntimeAssemblyKey assemblyKey, AssemblyFileRecord assemblyFile)
        {
            RuntimeAssemblySet runtimeAssemblies;
            if (!_key2AssemblySets.TryGetValue(assemblyKey, out runtimeAssemblies))
            {
                runtimeAssemblies = new RuntimeAssemblySet();
                _uid2AssemblySets.Add(assemblyFile.Uid, runtimeAssemblies);
                _key2AssemblySets.Add(assemblyKey, runtimeAssemblies);
            }
            else
            {
                if (!_uid2AssemblySets.ContainsKey(assemblyFile.Uid))
                    _uid2AssemblySets.Add(assemblyFile.Uid, runtimeAssemblies);
            }
            return runtimeAssemblies;
        }

        // the addin might has been stopped, and then started again, so we needs to avoid loading its assemblies twice.
        RuntimeAssembly GetOrCreateRuntimeAssembly(Addin addin, RuntimeAssemblyKey assemblyKey, AssemblyFileRecord assemblyFile)
        {
            if (_unregisteredAssemblies.Count == 0)
                return new AddinRuntimeAssembly(addin, assemblyKey, assemblyFile);
            for (int i = 0; i < _unregisteredAssemblies.Count; i++)
            {
                var unregisteredAssembly = _unregisteredAssemblies[i];
                if (unregisteredAssembly.AssemblyFile.LoadPath != assemblyFile.LoadPath)
                    continue;
                _unregisteredAssemblies.RemoveAt(i);
                unregisteredAssembly.AssemblyFile = assemblyFile;
                return unregisteredAssembly;
            }
            return new AddinRuntimeAssembly(addin, assemblyKey, assemblyFile);
        }

        // unregister the assemblies, but remember that they are still loaded, just unregistered, 
        // because there is no way to unload an assembly from the AppDomain, except unload the AppDomain itself.
        internal void UnregisterAssemblies(IEnumerable<AssemblyFileRecord> assemblyFiles)
        {
            if (assemblyFiles == null)
                return;

            foreach (var assemblyFile in assemblyFiles)
            {
                if (assemblyFile.Uid == UidStorage.InvalidAssemblyUid)
                    throw new InvalidOperationException(string.Format("The assembly file [{0}] has not been assigned a valid uid yet!", assemblyFile.LoadPath));

                RuntimeAssemblySet runtimeAssemblies;
                if (!_uid2AssemblySets.TryGetValue(assemblyFile.Uid, out runtimeAssemblies)) 
            		continue;

                bool assemblyRegistered = false;
                RuntimeAssembly runtimeAssembly = null;
                for (int i = runtimeAssemblies.Count - 1; i >= 0; i--)
                {
                    runtimeAssembly = runtimeAssemblies[i];
                    if (!ReferenceEquals(runtimeAssembly.AssemblyFile, assemblyFile))
                        continue;
                    assemblyRegistered = true;
                    runtimeAssemblies.RemoveAt(i);
                    break;
                }

                if (!assemblyRegistered)
                    throw new InvalidOperationException(string.Format("The assembly file [{0}] has not been registered to assembly resolver before!", assemblyFile.LoadPath));

                _unregisteredAssemblies.Add(runtimeAssembly);

                if (runtimeAssemblies.Count != 0)
                    continue;

                _key2AssemblySets.Remove(runtimeAssembly.AssemblyKey);
                _uid2AssemblySets.Remove(runtimeAssembly.Uid);
            }
        }

        Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyKey = RuntimeAssemblyKey.Create(args.Name);
            RuntimeAssemblySet runtimeAssemblies;
            return _key2AssemblySets.TryGetValue(assemblyKey, out runtimeAssemblies)
                ? runtimeAssemblies.LoadAssembly(_framework.AssemblyLoadPolicy)
                : null;
        }
    }
}
