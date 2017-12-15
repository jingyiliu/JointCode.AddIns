//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core.Runtime
{
    public sealed class RuntimeSystem
    {
        readonly RuntimeAssemblyResolver _asmResolver;

        internal RuntimeSystem(RuntimeAssemblyResolver asmResolver)
        {
            _asmResolver = asmResolver;
        }

        public Type GetType(TypeId typeId)
        {
            Requires.Instance.NotNull(typeId, "typeId");

            var assembly = _asmResolver.GetLoadedAssembly(typeId.AssemblyUid);
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
                    return module.ResolveType(typeId.MetadataToken);
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        internal Type GetInnerType(string typeName)
        {
            //if (_assemblyIDs == null)
            //    return null;

            //foreach (AddinAssembly adnAssembly in _assemblyIDs)
            //{
            //    if (adnAssembly.AssemblyKey == null)
            //        continue;

            //    Assembly assembly = TryLoadAssemblyFile(adnAssembly);
            //    if (assembly != null)
            //    {
            //        Type type = assembly.GetType(typeName);
            //        if (type != null)
            //            return type;
            //    }
            //}
            return null;
        }

        public Type GetType(string typeName)
        {
            return GetType(typeName, false);
        }

        public Type GetType(string typeName, bool throwIfNotFound)
        {
            //if (_assemblyIDs == null)
            //    return null;

            //foreach (var assemblyID in _assemblyIDs)
            //{
            //    if (!assemblyID.Loaded)
            //        continue;

            //    Type type = assemblyID.Assembly.GetType(typeName);
            //    if (type != null)
            //        return type;
            //}
            //foreach (var assemblyID in _assemblyIDs)
            //{
            //    if (assemblyID.Loaded || assemblyID.AssemblyKey == null)
            //        continue;

            //    Assembly assembly = TryLoadAssemblyFile(assemblyID);
            //    assemblyID.Assembly = assembly;
            //    if (assembly != null)
            //    {
            //        Type type = assembly.GetType(typeName);
            //        if (type != null)
            //            return type;
            //    }
            //}

            //if (throwIfNotFound)
            //    throw new Exception(string.Format("Could not find the specified type [{0}]!", typeName));
            return null;
        }

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

        public object GetResource(string resourceName)
        {
            return null;
        }
    }
}