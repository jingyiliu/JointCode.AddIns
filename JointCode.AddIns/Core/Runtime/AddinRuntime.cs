//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Core.Runtime
{
    /// <summary>
    /// Provides reflection capacity for addins.
    /// </summary>
    public sealed class AddinRuntime
    {
        static readonly Assembly[] EmptyAssemblies = new Assembly[0];

        readonly RuntimeAssemblyResolver _asmResolver;
        readonly Addin _thisAddin;
        Assembly[] _loadedAssemblies;

        internal AddinRuntime(RuntimeAssemblyResolver asmResolver, Addin thisAddin)
        {
            _asmResolver = asmResolver;
            _thisAddin = thisAddin;
            _loadedAssemblies = EmptyAssemblies;
        }

        #region Assembly
        /// <summary>
        /// Gets the loaded addin assemblies.
        /// </summary>
        public Assembly[] LoadedAssemblies { get { return _loadedAssemblies; } }

        /// <summary>
        /// Loads all addin assemblies.
        /// If the addin has not been started, this will starts the addin as well.
        /// </summary>
        /// <returns></returns>
        public Assembly[] LoadAssemblies()
        {
            LoadAssembliesThatIsUnloaded();
            return _loadedAssemblies;
        }

        Assembly[] LoadAssembliesThatIsUnloaded()
        {
            _thisAddin.ThrowIfAddinIsDisabled();
            if (!_thisAddin.Start())
                return null;

            if (_loadedAssemblies == EmptyAssemblies)
            {
                _loadedAssemblies = DoLoadAssembliesThatIsUnloaded();
                return _loadedAssemblies;
            }

            if (_loadedAssemblies.Length <= _thisAddin.AddinRecord.AssemblyFiles.Count)
            {
                var loadedAssemblies2 = DoLoadAssembliesThatIsUnloaded();
                if (loadedAssemblies2 == null)
                    return null;
                var allLoadedAssemblies = new Assembly[_loadedAssemblies.Length + loadedAssemblies2.Length];
                Array.Copy(_loadedAssemblies, allLoadedAssemblies, _loadedAssemblies.Length);
                Array.Copy(loadedAssemblies2, 0, allLoadedAssemblies, _loadedAssemblies.Length, loadedAssemblies2.Length);
                _loadedAssemblies = allLoadedAssemblies;
                return loadedAssemblies2;
            }

            return _loadedAssemblies;
        }

        Assembly[] DoLoadAssembliesThatIsUnloaded()
        {
            var addinRecord = _thisAddin.AddinRecord;
            if (addinRecord.AssemblyFiles == null)
                return null;

            List<Assembly> result = null;
            foreach (var assemblyFile in addinRecord.AssemblyFiles)
            {
                if (assemblyFile.Loaded)
                    continue;
                var assembly = _asmResolver.GetOrLoadAssembly(assemblyFile);
                if (assembly == null)
                    throw new InvalidOperationException("");
                assemblyFile.Loaded = true;
                result = result ?? new List<Assembly>();
                result.Add(assembly);
            }

            return result == null ? null : result.ToArray();

            //if (addinRecord.AssembliesRegistered)
            //return _asmResolver.LoadRegisteredAssemblies(addinRecord.AssemblyFiles);

            //addinRecord.AssembliesRegistered = true;
            //return _asmResolver.RegisterAndLoadAssemblies(_thisAddin, addinRecord.AssemblyFiles);
        }

        // register assemblies of the this addin to the assembly resolver, for getting them ready to be loaded into runtime.
        internal void RegisterAssemblies()
        {
            var addinRecord = _thisAddin.AddinRecord;
            if (addinRecord.AssembliesRegistered || addinRecord.AssemblyFiles == null)
                return;
            if (addinRecord.AssemblyFiles != null)
            {
                addinRecord.AssembliesRegistered = true;
                _asmResolver.RegisterAssemblies(_thisAddin, addinRecord.AssemblyFiles);
            }
        }

        // unregister the assemblies of the given addin from the assembly resolver, but remember that they are still loaded (if they already do), 
        // just unregistered, because there is no way to unload an assembly from the AppDomain, except unload the AppDomain itself.
        internal void UnregisterAssemblies()
        {
            var addinRecord = _thisAddin.AddinRecord;
            if (!addinRecord.AssembliesRegistered || addinRecord.AssemblyFiles == null)
                return;
            if (addinRecord.AssemblyFiles != null)
            {
                _asmResolver.UnregisterAssemblies(addinRecord.AssemblyFiles);
                foreach (var assemblyFile in addinRecord.AssemblyFiles)
                    assemblyFile.Loaded = false;
            }
            addinRecord.AssembliesRegistered = false;
        }
        #endregion

        #region Type
        internal Type GetType(int assemblyUid, string typeName)
        {
            var assembly = _asmResolver.GetOrLoadAssemblyByUid(assemblyUid);
            return assembly == null ? null : assembly.GetType(typeName);
        }

        public Type GetType(AddinTypeHandle addinTypeHandle)
        {
            Requires.Instance.NotNull(addinTypeHandle, "addinTypeHandle");

            var assembly = _asmResolver.GetOrLoadAssemblyByUid(addinTypeHandle.AssemblyUid);
            if (assembly == null)
                return null;
            var modules = assembly.GetLoadedModules(false);
            foreach (var module in modules)
            {
                try
                {
                    // Module.ResolveType Method (Int32, Type[], Type[])
                    // To resolve a metadata token for a TypeSpec whose signature contains ELEMENT_TYPE_VAR or ELEMENT_TYPE_MVAR, 
                    // use the ResolveType(Int32, Type[], Type[]) method overload, which allows you to supply the necessary context. 
                    // That is, when you are resolving a metadata token for a type that depends on the generic type parameters of the 
                    // generic type and/or the generic method in which the token is embedded, you must use the overload that allows 
                    // you to supply those type parameters.
                    return module.ResolveType(addinTypeHandle.MetadataToken);
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        public Type GetType(string typeName)
        {
            return GetType(typeName, false);
        }

        public Type GetType(string typeName, bool throwIfNotFound)
        {
            // 1. find loaded assemblies first
            var assemblies = _loadedAssemblies;
            if (assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }

            // 2. if not found, load the rest assemblies that is unloaded and search for the type
            assemblies = LoadAssembliesThatIsUnloaded();
            if (assemblies != null)
            {
                foreach (var assembly in assemblies)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }

            // 3. if not found, search parent addins for the type recursively.
            if (_thisAddin.ParentAddins != null)
            {
                foreach (var parentAddin in _thisAddin.ParentAddins)
                {
                    var type = parentAddin.Runtime.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }

            if (throwIfNotFound)
                throw GetTypeNotFoundException(typeName);
            return null;
        }

        ////@cache: if this value is true, the resulted type will be turned into a AddinTypeHandle, and then added to a Dictionary<string, AddinTypeHandle>, which will be persisted to a type mapping storage file
        ////        (like addin storage file), the application will read this type mapping storage file every time it starts up. 
        //public Type GetType(string typeName, bool cache, bool throwIfNotFound)
        //{
        //    return null;
        //}

        Exception GetTypeNotFoundException(string typeName)
        {
            return !_thisAddin.Header.Name.IsNullOrWhiteSpace() 
                ? new ArgumentException(string.Format("Could not find the specified type [{0}] in addin [{1}] [{2}]!", typeName, _thisAddin.Header.Name, _thisAddin.Header.AddinId.Guid))
                : new ArgumentException(string.Format("Could not find the specified type [{0}] in addin [{1}]!", typeName, _thisAddin.Header.AddinId.Guid));
        }
        #endregion

        #region Instance
        public object CreateInstance(string typeName, params object[] args)
        {
            return CreateInstance(typeName, false, args);
        }

        public object CreateInstance(string typeName, bool throwIfNotFound, params object[] args)
        {
            object result = null;
            var type = GetType(typeName, throwIfNotFound);
            if (type != null)
                result = Activator.CreateInstance(type, args);
            return result;
        }
        #endregion

        #region Resource
        public object GetResource(string resourceName)
        {
            return GetResource(resourceName, false);
        }

        public object GetResource(string resourceName, bool throwIfNotFound)
        { 
            //// 1. find loaded assemblies first
            //var assemblies = _loadedAssemblies;
            //if (assemblies != null)
            //{
            //    foreach (var assembly in assemblies)
            //    {
            //        var info = assembly.GetManifestResourceInfo(resourceName);
            //        info.
            //        if (type != null)
            //            return type;
            //    }
            //}

            //// 2. if not found, load the rest assemblies that is unloaded and search for the type
            //assemblies = LoadAssembliesThatIsUnloaded();
            //if (assemblies != null)
            //{
            //    foreach (var assembly in assemblies)
            //    {
            //        var type = assembly.GetType(typeName);
            //        if (type != null)
            //            return type;
            //    }
            //}

            //// 3. if not found, search parent addins for the type recursively.
            //if (_thisAddin.ParentAddins != null)
            //{
            //    foreach (var parentAddin in _thisAddin.ParentAddins)
            //    {
            //        var type = parentAddin.RuntimeSystem.GetType(typeName);
            //        if (type != null)
            //            return type;
            //    }
            //}

            //if (throwIfNotFound)
            //    throw GetTypeNotFoundException(typeName);
            return null;
        }
        #endregion
    }
}