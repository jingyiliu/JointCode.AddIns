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
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Data;
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Resolving
{
    class ResolutionContext : IDisposable
    {
        struct TypeKey
        {
            readonly string _assemblyFullName;
            readonly int _typeMetadataToken;

            internal TypeKey(string assemblyFullName, int typeMetadataToken)
            {
                _assemblyFullName = assemblyFullName;
                _typeMetadataToken = typeMetadataToken;
            }

            internal string AssemblyFullName { get { return _assemblyFullName; } }
            internal int TypeMetadataToken { get { return _typeMetadataToken; } }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                try
                {
                    var val = (TypeKey) obj;
                    return val._typeMetadataToken == _typeMetadataToken && val._assemblyFullName == _assemblyFullName;
                }
                catch
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return _assemblyFullName.GetHashCode() ^ _typeMetadataToken;
            }
        }

        readonly Dictionary<TypeKey, DataTransformer> _typeKey2Transformers = new Dictionary<TypeKey, DataTransformer>();
        readonly Dictionary<Guid, AddinResolution> _guid2Addins = new Dictionary<Guid, AddinResolution>();
    	readonly Dictionary<AssemblyKey, AssemblyResolutionSet> _key2AssemblySets = new Dictionary<AssemblyKey, AssemblyResolutionSet>();
    	
        readonly Dictionary<string, ExtensionBuilderResolution> _path2ExtensionBuilders = new Dictionary<string, ExtensionBuilderResolution>();
        readonly Dictionary<string, ExtensionPointResolution> _path2ExtensionPoints = new Dictionary<string, ExtensionPointResolution>();
        readonly Dictionary<string, ExtensionResolution> _path2Extensions = new Dictionary<string, ExtensionResolution>();

        readonly Dictionary<int, ExtensionBuilderResolution> _uid2ExtensionBuilders = new Dictionary<int, ExtensionBuilderResolution>();
        readonly Dictionary<int, string> _uid2UpdatedExtensionBuilderPaths = new Dictionary<int, string>();

        /// <summary>
        /// All assembly sets registered to this instance.
        /// </summary>
        internal IEnumerable<AssemblyResolutionSet> AssemblySets { get { return _key2AssemblySets.Values; } }

        internal void RegisterDataTransformer(string assemblyFullName, int typeMetadataToken, DataTransformer transformer)
        {
            _typeKey2Transformers.Add(new TypeKey(assemblyFullName, typeMetadataToken), transformer);
        }

        internal bool TryGetDataTransformer(string assemblyFullName, int typeMetadataToken,
            out DataTransformer transformer)
        {
            return _typeKey2Transformers.TryGetValue(
                new TypeKey(assemblyFullName, typeMetadataToken),
                out transformer);
        }

        #region Addin
        public void RegisterAddin(ObjectId addinId, AddinResolution addin)
        {
            _guid2Addins.Add(addinId.Guid, addin);
        }
        
        public bool TryRegisterAddin(ResolutionResult resolutionResult, ObjectId addinId, AddinResolution newAddin, out AddinResolution existingAddin)
        {
            if (_guid2Addins.TryGetValue(addinId.Guid, out existingAddin))
            {
                resolutionResult.AddError(string.Format("An addin with the identity [{0}] already exists!", addinId.Guid));
                return false;
            }
            _guid2Addins.Add(addinId.Guid, newAddin);
            return true;
        }

        public void UnregisterAddin(ObjectId addinId)
        {
            _guid2Addins.Remove(addinId.Guid);
        }
        
        public bool TryGetAddin(ObjectId addinId, out AddinResolution addin)
        {
        	return _guid2Addins.TryGetValue(addinId.Guid, out addin);
        }
        #endregion

        #region Assembly
        //public void RegisterApplicationAssembly(ResolutionResult resolutionResult, AssemblyResolution assembly)
        //{
        //    TryLoadAndRegisterAssembly(dialog, assembly);
        //}

        //public bool TryRegisterApplicationAssembly(ResolutionResult resolutionResult, string assembly)
        //{
        //    var asm = new AssemblyResolution(assembly);
        //    return TryLoadAndRegisterAssembly(dialog, asm);
        //}

        public void RegisterAssembly(ResolutionResult resolutionResult, AssemblyResolution assembly)
        {
        	TryLoadAndRegisterAssembly(resolutionResult, assembly);
        }
        
        public bool TryRegisterAssembly(ResolutionResult resolutionResult, AssemblyResolution assembly)
        {
            return TryLoadAndRegisterAssembly(resolutionResult, assembly);
        }
        
        bool TryLoadAndRegisterAssembly(ResolutionResult resolutionResult, AssemblyResolution assembly)
        {
        	if (!assembly.TryLoad()) 
			{
			    resolutionResult.AddError("Failed to load assembly file " + assembly.AssemblyFile.FilePath);
				return false;
			}
        	
			AssemblyResolutionSet assemblySet;
			if (!_key2AssemblySets.TryGetValue(assembly.AssemblyKey, out assemblySet)) 
			{
				assemblySet = new AssemblyResolutionSet();
				_key2AssemblySets[assembly.AssemblyKey] = assemblySet;
			}

            //if (assembly.AssemblyFile.Uid != UidStorage.InvalidAssemblyUid) 
            //    _uid2AssemblySets.Add(assembly.AssemblyFile.Uid, assemblySet);
			
			assemblySet.Add(assembly);
			
			return true;
        }
        
        public void UnregisterAssembly(AssemblyResolution assembly)
        {
            if (assembly.AssemblyKey == null)
                return;

        	AssemblyResolutionSet assemblySet;
            if (!_key2AssemblySets.TryGetValue(assembly.AssemblyKey, out assemblySet))
                return;

            //if (assembly.AssemblyFile.Uid == UidStorage.InvalidAssemblyUid) 
            //{
            //    if (!_key2AssemblySets.TryGetValue(assembly.AssemblyKey, out assemblySet))
            //        return;
            //}
            //else
            //{
            //    if (!_uid2AssemblySets.TryGetValue(assembly.AssemblyFile.Uid, out assemblySet))
            //        return;
            //}

        	if (assemblySet.Remove(assembly) && assemblySet.Count == 0) 
        	{
        		_key2AssemblySets.Remove(assembly.AssemblyKey);
                //if (assembly.AssemblyFile.Uid != UidStorage.InvalidAssemblyUid) 
                //    _uid2AssemblySets.Remove(assembly.AssemblyFile.Uid);
        	}
        }

        // Gets the required assembly references of @assembly that needs resolution. 
        // By required, we mean assemblies that is not provided by runtime (.net, mono, etc) or the JointCode.AddIns.dll itself. 
        // By default it refers to the GAC assemblies.
        public bool TryGetRequiredAssemblyReferences(ResolutionResult resolutionResult, AssemblyResolution assembly, out List<AssemblyResolutionSet> result)
        {
        	var assemblyKeys = assembly.GetRequiredAssemblyReferences();
            if (assemblyKeys == null)  // get all referenced assemblies that is required.
        	{
        		result = null;
        		return true;
        	}

            result = new List<AssemblyResolutionSet>(assemblyKeys.Count);
        	for (int i = 0; i < assemblyKeys.Count; i++) 
        	{
        		AssemblyResolutionSet assemblySet;
        		if (_key2AssemblySets.TryGetValue(assemblyKeys[i], out assemblySet)) 
        		{
                    result.Add(assemblySet);
        		}
        		else
        		{
		            resolutionResult.AddError("!!!!!!!!!!!");
                    return false;
        		}
        	}
        	
        	return true;
        }

        public bool TryGetAssemblySet(AssemblyKey assemblyKey, out AssemblyResolutionSet assemblySet)
        {
            return _key2AssemblySets.TryGetValue(assemblyKey, out assemblySet);
        }

        ///// <summary>
        ///// Tries to get a probable assembly (a runtime or application assembly).
        ///// </summary>
        //public bool TryGetProbableAssembly(string assemblyName, out AssemblyResolution result)
        //{
        //    result = AssemblyResolution.GetProbableAssembly(assemblyName);
        //    return result != null;
        //}
        #endregion
        
        #region Type
        /// <summary>
        /// Gets an unique type that defined in an addin.
        /// </summary>
        public TypeResolution GetUniqueAddinType(AddinResolution addin, string typeName)
        {
        	TypeResolution result;
        	return DoTryGetUniqueAddinType(addin, typeName, false, out result) ? result : null;
        }

        /// <summary>
        /// Gets an type that defined in an addin.
        /// </summary>
        public TypeResolution GetAddinType(AddinResolution addin, string typeName)
        {
        	TypeResolution result;
        	return DoTryGetAddinType(addin, typeName, false, out result) ? result : null;
        }

        /// <summary>
        /// Tries to get an type that defined in an addin.
        /// </summary>
        public bool TryGetAddinType(AddinResolution addin, string typeName, out TypeResolution type)
        {
        	return DoTryGetAddinType(addin, typeName, false, out type);
        }

        /// <summary>
        /// Tries to get an type that defined in an probable assembly (a runtime or application assembly).
        /// </summary>
        public bool TryGetProbableType(string assemblyName, string typeName, out TypeResolution result)
        {
            var asm = AssemblyResolution.GetProbableAssembly(assemblyName);
            if (asm != null)
                return asm.TryGetType(typeName, out result);
            result = null;
            return false;
        }
        
        bool DoTryGetAddinType(AddinResolution addin, string typeName, bool fastFailed, out TypeResolution result)
        {
        	result = null;

            // get type from the specified addin
        	foreach (var assembly in addin.Assemblies) 
    		{
    			if (assembly.TryGetType(typeName, out result))
    				return true;
    		}
        	
            // get type from the other addins
        	foreach (var kv in _key2AssemblySets) 
        	{
        		var assembly = kv.Value[0];
        		if (!ReferenceEquals(assembly.DeclaringAddin, addin) 
                    && assembly.TryGetType(typeName, out result))
    				break;
        	}
        	
        	if (fastFailed && result == null) 
        		ThrowWhenTypeNotFound(typeName);

            return result != null;
        }
        
        bool DoTryGetUniqueAddinType(AddinResolution addin, string typeName, bool fastFailed, out TypeResolution result)
        {
        	result = null;

            // get type from the specified addin
        	foreach (var assembly in addin.Assemblies) 
    		{
    			if (assembly.TryGetType(typeName, out result))
    				return true;
    		}

            // get type from the other addins
        	foreach (var kv in _key2AssemblySets) 
        	{
        		var assembly = kv.Value[0];
        		if (ReferenceEquals(assembly.DeclaringAddin, addin))
    				continue;
				
				if (result != null) 
				{
					if (assembly.TryGetType(typeName, out result)) 
						throw new ArgumentException(string.Format("The required type [{0}] has been found in different addins!", typeName));
				}
				else
				{
					assembly.TryGetType(typeName, out result);
				}
        	}

        	if (fastFailed && result == null) 
        		ThrowWhenTypeNotFound(typeName);

            return result != null;
        }
        
        static void ThrowWhenTypeNotFound(string typeName)
        {
        	throw new ArgumentException(string.Format("Can not find a type with name [{0}] from addins!", typeName));
        }
        #endregion

        #region ExtensionPoint
        public void RegisterExtensionPoint(ExtensionPointResolution extensionPoint)
        {
        	_path2ExtensionPoints.Add(extensionPoint.Path, extensionPoint);
        }

        // returns null if sucessful, otherwise return an existing ExtensionPointResolution
        public bool TryRegisterExtensionPoint(ResolutionResult resolutionResult, ExtensionPointResolution newExtensionPoint, 
            out ExtensionPointResolution existingExtensionPoint)
        {
            if (_path2ExtensionPoints.TryGetValue(newExtensionPoint.Path, out existingExtensionPoint))
            {
                resolutionResult.AddError("@@@@@@@@@@@");
                return false;
            }
            _path2ExtensionPoints.Add(newExtensionPoint.Path, newExtensionPoint);
            return true;
        }
        
        public void UnregisterExtensionPoint(ExtensionPointResolution extensionPoint)
        {
            _path2ExtensionPoints.Remove(extensionPoint.Path);
        }
        
        public ExtensionPointResolution GetExtensionPoint(string extensionPointPath)
        {
            return _path2ExtensionPoints[extensionPointPath];
        }
        
        public bool TryGetExtensionPoint(ResolutionResult resolutionResult, string extensionPointPath, out ExtensionPointResolution result)
        {
            return _path2ExtensionPoints.TryGetValue(extensionPointPath, out result);
        }
        #endregion

        #region ExtensionBuilder
        public void RegisterExtensionBuilder(ExtensionBuilderResolution extensionBuilder)
        {
            _path2ExtensionBuilders.Add(extensionBuilder.Path, extensionBuilder);
            if (extensionBuilder.Uid != UidStorage.InvalidExtensionBuilderUid)
                _uid2ExtensionBuilders.Add(extensionBuilder.Uid, extensionBuilder);
        }

        // returns null if sucessful, otherwise return an existing ExtensionPointResolution
        public bool TryRegisterExtensionBuilder(ResolutionResult resolutionResult, ExtensionBuilderResolution newExtensionBuilder, 
            out ExtensionBuilderResolution existingExtensionBuilder)
        {
            if (_path2ExtensionBuilders.TryGetValue(newExtensionBuilder.Path, out existingExtensionBuilder))
            {
                resolutionResult.AddError("&&&&&&&&&&&&&&&&");
                return false;
            }
            _path2ExtensionBuilders.Add(newExtensionBuilder.Path, newExtensionBuilder);
            return true;
        }
        
        public void UnregisterExtensionBuilder(ExtensionBuilderResolution extensionBuilder)
        {
            _path2ExtensionBuilders.Remove(extensionBuilder.Path);
            if (extensionBuilder.Uid != UidStorage.InvalidExtensionBuilderUid)
                _uid2ExtensionBuilders.Remove(extensionBuilder.Uid);
        }

        public ExtensionBuilderResolution GetExtensionBuilder(string extensionBuilderPath)
        {
            return _path2ExtensionBuilders[extensionBuilderPath];
        }
        
        public bool TryGetExtensionBuilder(ResolutionResult resolutionResult, string extensionBuilderPath, out ExtensionBuilderResolution extensionBuilder)
        {
            return _path2ExtensionBuilders.TryGetValue(extensionBuilderPath, out extensionBuilder);
        }

        public bool TryGetExtensionBuilder(ResolutionResult resolutionResult, int extensionBuilderUid, out ExtensionBuilderResolution extensionBuilder)
        {
            return _uid2ExtensionBuilders.TryGetValue(extensionBuilderUid, out extensionBuilder);
        }

        // 因为直接受影响的插件的 Extension.Header 中包含 ExtensionBuilderUid 属性，因此被更新 (updated) 插件需要先注册这个
        public void RegisterExtensionBuilderPath(int extensionBuilderUid, string extensionBuilderPath)
        {
            _uid2UpdatedExtensionBuilderPaths.Add(extensionBuilderUid, extensionBuilderPath);
        }

        public bool TryGetExtensionBuilderPath(ResolutionResult resolutionResult, int extensionBuilderUid, out string extensionBuilderPath)
        {
            return _uid2UpdatedExtensionBuilderPaths.TryGetValue(extensionBuilderUid, out extensionBuilderPath);
        }
        #endregion
        
        #region Extension
        public void RegisterExtension(ExtensionResolution extension)
        {
            _path2Extensions.Add(extension.Head.Path, extension);
        }

        // returns null if sucessful, otherwise return an existing ExtensionPointResolution
        public bool TryRegisterExtension(ResolutionResult resolutionResult, ExtensionResolution newExtension, out ExtensionResolution existingExtension)
        {
            if (_path2Extensions.TryGetValue(newExtension.Head.Path, out existingExtension))
            {
                resolutionResult.AddError("##############");
                return false;
            }
            _path2Extensions.Add(newExtension.Head.Path, newExtension);
            return true;
        }
        
        public void UnregisterExtension(ExtensionResolution extension)
        {
            _path2Extensions.Remove(extension.Head.Path);
        }
        
        public ExtensionResolution GetExtension(string extensionPath)
        {
            return _path2Extensions[extensionPath];
        }
        
        public bool TryGetExtension(ResolutionResult resolutionResult, string extensionPath, out ExtensionResolution extension)
        {
            return _path2Extensions.TryGetValue(extensionPath, out extension);
        }
        #endregion

        public void Dispose()
        {
            AssemblyResolution.DisposeInternal();
        }
    }
}