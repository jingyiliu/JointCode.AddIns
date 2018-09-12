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
using JointCode.AddIns.Core.Runtime;
using JointCode.AddIns.Extension.Loaders;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Extension
{
	#region ExtensionBuilder    
    class ReflectionExtensionBuilderFactory : IExtensionBuilderFactory
    {
        public IExtensionBuilder CreateInstance(Type extensionBuilderType, ExtensionData extensionData)
        {
            var result = Activator.CreateInstance(extensionBuilderType);

            if (extensionData == null)
                return result as IExtensionBuilder;

            var props = extensionBuilderType.GetProperties();
            if (props.Length == 0)
                return result as IExtensionBuilder;

            // set property values
            foreach (var prop in props)
            {
                if (!prop.CanWrite)
                    continue;
                object propValue;
                if (extensionData.TryGet(prop.Name, out propValue))
                    prop.SetValue(result, propValue, null);
            }

            return result as IExtensionBuilder;
        }
    }

    abstract class ExtensionLoaderFactory
    {
        protected readonly Type _extensionBuilderType;
    	protected readonly IExtensionBuilderFactory _ebFactory;

        protected ExtensionLoaderFactory(IExtensionBuilderFactory ebFactory, Type extensionBuilderType)
    	{
    		_ebFactory = ebFactory;
            _extensionBuilderType = extensionBuilderType;
    	}
    	
        internal abstract ExtensionLoader CreateExtensionLoader(ExtensionRecord exRecord);
    }

    class ExtensionLoaderFactory<TExtension> : ExtensionLoaderFactory
    {
        public ExtensionLoaderFactory(IExtensionBuilderFactory ebFactory, Type extensionBuilderType)
            : base(ebFactory, extensionBuilderType)
    	{ }
    	
        internal override ExtensionLoader CreateExtensionLoader(ExtensionRecord exRecord)
        {
            var extensionBuilder = _ebFactory.CreateInstance(_extensionBuilderType, exRecord.Data);
            if (exRecord.Children != null) 
            {
            	var result = new CompositeExtensionLoader<TExtension>(exRecord, extensionBuilder as ICompositeExtensionBuilder<TExtension>);
            	return result;
            }
            else
            {
            	var result = new ExtensionLoader<TExtension>(exRecord, extensionBuilder as IExtensionBuilder<TExtension>);
            	return result;
            }
        }
    }
    #endregion
    
    #region ExtensionPoint    
    class ReflectionExtensionPointFactory : IExtensionPointFactory
    {
        public IExtensionPoint CreateInstance(Type extensionPointType)
        {
            return Activator.CreateInstance(extensionPointType) as IExtensionPoint;
        }
    }
    
    abstract class ExtensionPointLoaderFactory
    {
        protected readonly Type _extensionPointType;
    	protected readonly IExtensionPointFactory _epFactory;

        protected ExtensionPointLoaderFactory(IExtensionPointFactory epFactory, Type extensionPointType)
    	{
    		_epFactory = epFactory;
            _extensionPointType = extensionPointType;
    	}
    	
        internal abstract ExtensionPointLoader CreateExtensionPointLoader(ExtensionPointRecord epRecord);
    }

    class ExtensionPointLoaderFactory<TExtension, TRoot> : ExtensionPointLoaderFactory 
        where TRoot : class 
    {
        public ExtensionPointLoaderFactory(IExtensionPointFactory epFactory, Type extensionPointType)
            : base(epFactory, extensionPointType)
    	{ }
    	
        internal override ExtensionPointLoader CreateExtensionPointLoader(ExtensionPointRecord epRecord)
        {
            var extensionPoint = _epFactory.CreateInstance(_extensionPointType);
            var result = new ExtensionPointLoader<TExtension, TRoot>(epRecord, extensionPoint as IExtensionPoint<TExtension, TRoot>);
        	return result;
        }
    }
    #endregion
    
    class LoaderFactory
    {
        static readonly Type ExtensionBuilderType = typeof(IExtensionBuilder<>);
        static readonly Type CompositeExtensionBuilderType = typeof(ICompositeExtensionBuilder<>);
        static readonly Type ExtensionPointType = typeof(IExtensionPoint<,>);
        
    	readonly RuntimeAssemblyResolver _asmResolver;
        readonly IExtensionPointFactory _extensionPointFactory;
        readonly IExtensionBuilderFactory _extensionBuilderFactory;
        Dictionary<int, ExtensionLoaderFactory> _uid2ExLoaderFactories;
    	Dictionary<int, ExtensionPointLoaderFactory> _uid2EpLoaderFactories;
    	
    	internal LoaderFactory(RuntimeAssemblyResolver asmResolver, IExtensionPointFactory extensionPointFactory, IExtensionBuilderFactory extensionBuilderFactory)
    	{
    		_asmResolver = asmResolver;
	        _extensionPointFactory = extensionPointFactory;
	        _extensionBuilderFactory = extensionBuilderFactory;
            _uid2ExLoaderFactories = new Dictionary<int, ExtensionLoaderFactory>();
    		_uid2EpLoaderFactories = new Dictionary<int, ExtensionPointLoaderFactory>();
    	}

        internal void Reset()
        {
            _uid2ExLoaderFactories = new Dictionary<int, ExtensionLoaderFactory>();
            _uid2EpLoaderFactories = new Dictionary<int, ExtensionPointLoaderFactory>();
        }

        #region ExtensionPoint

        internal bool ExtensionPointRegistered(ExtensionPointRecord epRecord)
        {
            return _uid2EpLoaderFactories.ContainsKey(epRecord.Uid);
        }

        internal void RegisterExtensionPoint(ExtensionPointRecord epRecord, Type extensionRootType)
        {
            ExtensionPointLoaderFactory factory;
            if (_uid2EpLoaderFactories.TryGetValue(epRecord.Uid, out factory))
                return;

            var assembly = _asmResolver.GetOrLoadAssemblyByUid(epRecord.AssemblyUid);
            var epType = assembly.GetType(epRecord.TypeName);

            Type extensionType, rootType;
            GetExtensionAndRootType(epType, out extensionType, out rootType);
            if (!rootType.IsAssignableFrom(extensionRootType))
                throw new ArgumentException(string.Format(
                    "The provided extension root object is invalid for ExtensionPoint [{0}]! The reason is: extension root type not matched! The required type is [{1}], while the provided extension root is of type [{2}]!",
                    epRecord.Path, rootType.FullName, extensionRootType.FullName));

            var factoryType = typeof(ExtensionPointLoaderFactory<,>).MakeGenericType(extensionType, rootType);
            //IExtensionPointFactory _extensionPointFactory = new ReflectionExtensionPointFactory();
            factory = Activator.CreateInstance(factoryType, _extensionPointFactory, epType) as ExtensionPointLoaderFactory;

            _uid2EpLoaderFactories.Add(epRecord.Uid, factory);
        }

        static void GetExtensionAndRootType(Type extensionPointType, out Type extensionType, out Type extensionRootType)
        {
            extensionType = extensionRootType = null;
            var interfaceTypes = extensionPointType.GetInterfaces();
            if (interfaceTypes == null && interfaceTypes.Length == 0)
                throw new InvalidOperationException();

            foreach (Type interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsInterface || !interfaceType.IsGenericType)
                    continue;

                Type genType = interfaceType.GetGenericTypeDefinition();
                if (genType == ExtensionPointType)
                {
                    Type[] genParams = interfaceType.GetGenericArguments();
                    if (genParams != null && genParams.Length == 2)
                    {
                        extensionType = genParams[0];
                        extensionRootType = genParams[1];
                        return;
                    }
                }
            }
        }

        internal void UnregisterExtensionPoint(ExtensionPointRecord epRecord)
        {
            _uid2EpLoaderFactories.Remove(epRecord.Uid);
        } 
        #endregion

        #region ExtensionBuilder
        internal void RegisterExtensionBuilder(ExtensionBuilderRecord ebRecord)
        {
            ExtensionLoaderFactory factory;
            if (_uid2ExLoaderFactories.TryGetValue(ebRecord.Uid, out factory))
                return;

            var assembly = _asmResolver.GetOrLoadAssemblyByUid(ebRecord.AssemblyUid);
            var ebType = assembly.GetType(ebRecord.TypeName);

            var extensionType = GetExtensionType(ebType);
            var factoryType = typeof(ExtensionLoaderFactory<>).MakeGenericType(extensionType);

            //var _extensionBuilderFactory = new ReflectionExtensionBuilderFactory();
            factory = Activator.CreateInstance(factoryType, _extensionBuilderFactory, ebType) as ExtensionLoaderFactory;

            _uid2ExLoaderFactories.Add(ebRecord.Uid, factory);
        }

        static Type GetExtensionType(Type extensionBuilderType)
        {
            var interfaceTypes = extensionBuilderType.GetInterfaces();
            if (interfaceTypes == null && interfaceTypes.Length == 0)
                throw new InvalidOperationException();

            Type result = null;
            foreach (var interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsInterface || !interfaceType.IsGenericType)
                    continue;

                var genType = interfaceType.GetGenericTypeDefinition();
                if (genType == ExtensionBuilderType)
                {
                    var genParams = interfaceType.GetGenericArguments();
                    if (genParams != null && genParams.Length == 1)
                    {
                        //                        _interfaceType = interfaceType;
                        result = genParams[0];
                    }
                    break;
                }
                else if (genType == CompositeExtensionBuilderType)
                {
                    var genParams = interfaceType.GetGenericArguments();
                    if (genParams != null && genParams.Length == 1)
                    {
                        //                        _interfaceType = interfaceType;
                        result = genParams[0];
                    }
                    break;
                }
            }

            return result;
        }

        internal void UnregisterExtensionBuilder(ExtensionBuilderRecord ebRecord)
        {
            _uid2ExLoaderFactories.Remove(ebRecord.Uid);
        }

        //internal void RegisterExtensionBuilderFactory(IExtensionBuilderFactory ebFactory)
        //{
        //} 
        #endregion
        
        internal ExtensionPointLoader CreateExtensionPointLoader(ExtensionPointRecord epRecord)
    	{
        	ExtensionPointLoaderFactory factory;
        	if (!_uid2EpLoaderFactories.TryGetValue(epRecord.Uid, out factory)) 
        		throw new InvalidOperationException(string.Format("The extension point [{0}] has not been registered to the extension mannager yet!", epRecord.Path));
        	return factory.CreateExtensionPointLoader(epRecord);
    	}

        internal bool TryCreateExtensionPointLoader(ExtensionPointRecord epRecord, out ExtensionPointLoader result)
        {
            ExtensionPointLoaderFactory factory;
            if (!_uid2EpLoaderFactories.TryGetValue(epRecord.Uid, out factory))
            {
                result = null;
                return false;
            }
            result = factory.CreateExtensionPointLoader(epRecord);
            return true;
        }

        internal ExtensionLoader CreateExtensionLoader(ExtensionRecord exRecord)
    	{
        	ExtensionLoaderFactory factory;
        	if (!_uid2ExLoaderFactories.TryGetValue(exRecord.Head.ExtensionBuilderUid, out factory)) 
        		throw new InvalidOperationException();
        	return factory.CreateExtensionLoader(exRecord);
    	}
    }
}
