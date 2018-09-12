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
using System.Reflection;
using JointCode.AddIns.Core;
using JointCode.AddIns.Extension;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace JointCode.AddIns.Resolving.Assets
{
    partial class TypeResolution
    {
        internal static TypeResolution Empty = new TypeResolution(null, null);
        static readonly TypeDefinition ExtensionPointInterface;
        static readonly TypeDefinition ExtensionBuilderInterface;
        static readonly TypeDefinition CompositeExtensionBuilderInterface;
        internal static readonly TypeDefinition ExtensionPropertyAttributeType;
        static readonly TypeDefinition AddinActivatorInterface;

        static TypeResolution()
        {
            var epMetadataToken = typeof(IExtensionPoint<,>).MetadataToken;
            var ebMetadataToken = typeof(IExtensionBuilder<>).MetadataToken;
            var cebMetadataToken = typeof(ICompositeExtensionBuilder<>).MetadataToken;
            var epaMetadataToken = typeof(ExtensionPropertyAttribute).MetadataToken;
            var aaMetadataToken = typeof(IAddinActivator).MetadataToken;
            var thisAssembly = AssemblyResolution.ThisAssembly;
            foreach (var module in thisAssembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    var metadataToken = type.MetadataToken.ToInt32();
                    if (ExtensionPointInterface == null && metadataToken == epMetadataToken)
                        ExtensionPointInterface = type;
                    if (ExtensionBuilderInterface == null && metadataToken == ebMetadataToken)
                        ExtensionBuilderInterface = type;
                    if (CompositeExtensionBuilderInterface == null && metadataToken == cebMetadataToken)
                        CompositeExtensionBuilderInterface = type;
                    if (ExtensionPropertyAttributeType == null && metadataToken == epaMetadataToken)
                        ExtensionPropertyAttributeType = type;
                    if (AddinActivatorInterface == null && metadataToken == aaMetadataToken)
                        AddinActivatorInterface = type;
                }
            }
        }

        public static implicit operator AddinTypeHandle(TypeResolution tr)
        {
            return new AddinTypeHandle(tr._assembly.Uid, tr.MetadataToken);
        }
    }

    /// <summary>
    /// Represent a type during resolution.
    /// Notes that to determine whether two <see cref="TypeResolution"/>s is the same, you can not use <see cref="object.ReferenceEquals"/>,
    /// try to use the <see cref="TypeResolution.Equals(TypeResolution)"/> instead.
    /// </summary>
    partial class TypeResolution : IEquatable<TypeResolution>
    {
        readonly AssemblyResolution _assembly;
        readonly TypeDefinition _typeDef;
        //Type _type;
        //List<PropertyInfo> _properties;

        internal TypeResolution(AssemblyResolution assembly, TypeDefinition typeDef)
        {
            _assembly = assembly;
            _typeDef = typeDef; 
        }

        ///// <summary>
        ///// Gets a value indicating whether this type is defined in a probable assembly (an assembly that is provided
        ///// by runtime [.net or mono] or application) or not.
        ///// </summary>
        //internal bool IsProbableType { get { return _isProbableType; } }
        //internal bool IsOpenGeneric { get { return _typeDef.IsSealed; } }

        internal AssemblyResolution Assembly { get { return _assembly; } }
        internal string TypeName { get { return _typeDef.FullName; } }
        internal bool IsInterface { get { return _typeDef.IsInterface; } }
        internal bool IsAbstract { get { return _typeDef.IsAbstract; } }
        internal bool IsClass { get { return _typeDef.IsClass; } }
        internal bool IsValueType { get { return _typeDef.IsValueType; } }
        internal bool IsSealed { get { return _typeDef.IsSealed; } }
        internal int MetadataToken { get { return _typeDef.MetadataToken.ToInt32(); } }

        internal List<ConstructorResolution> GetConstructors()
        {
            var result = new List<ConstructorResolution>();
            foreach (var method in _typeDef.Methods)
            {
                if (!method.IsConstructor)
                    continue;
                result.Add(new ConstructorResolution(this, method));
            }
            return result;
        }

        //internal bool IsAssignableFrom(TypeResolution subType)
        //{
        //    return _typeDef.IsAssignableFrom(subType._typeDef);
        //}

        #region IEquatable implementation
        public bool Equals(TypeResolution other)
        {
            return other != null && _assembly.Equals(other._assembly) && MetadataToken == other.MetadataToken;
        }
        #endregion

        internal bool HasProperties { get { return _typeDef.HasProperties; } }
        internal Collection<PropertyDefinition> Properties { get { return _typeDef.Properties; } }

        //internal Type GetRuntimeType()
        //{
        //    if (_type != null)
        //        return _type;
        //    var assembly = _assembly.GetAssembly();
        //    foreach (var module in assembly.GetModules())
        //    {
        //        _type = module.ResolveType(MetadataToken);
        //        if (_type != null)
        //            break;
        //    }
        //    return _type;
        //}

        //internal List<PropertyInfo> GetSettableRuntimeProperties()
        //{
        //    if (_properties != null)
        //        return _properties;
        //    var type = GetRuntimeType();
        //    var properties = type.GetProperties();
        //    foreach (var property in properties)
        //    {
        //        if (!property.CanWrite)
        //            continue;
        //        _properties = _properties ?? new List<PropertyInfo>();
        //        _properties.Add(property);
        //    }
        //    return _properties;
        //}
    }

    partial class TypeResolution
    {
        // whether this type implements the extension point interface
        internal bool ImplementsAddinActivatorInterface(ResolutionResult resolutionResult, ResolutionContext ctx)
        {
            return _typeDef.ImplementsInterface(AddinActivatorInterface);
        }

        // whether this type implements the extension point interface
        internal bool ImplementsExtensionPointInterface(ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return TryResolveExtensionType(ctx, _typeDef, ExtensionPointInterface, 2, out extensionType);
        }

        // whether this type implements the extension builder interface
        internal bool ImplementsExtensionBuilderInterface(ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return TryResolveExtensionType(ctx, _typeDef, ExtensionBuilderInterface, 1, out extensionType);
        }

        // whether this type implements the composite extension builder interface
        internal bool ImplementsCompositeExtensionBuilderInterface(ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return TryResolveExtensionType(ctx, _typeDef, CompositeExtensionBuilderInterface, 1, out extensionType);
        }

        bool TryResolveExtensionType(ResolutionContext ctx, 
            TypeDefinition classType, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeResolution extensionType)
        {
            extensionType = null;

            // tries to get the extension type
            TypeReference trExtensionType;
            var result = TryResolveExtensionType(classType, null, baseGenericInterface, genericParamCount, out trExtensionType);
            if (!result)
                return false;

            // tries to retrieve the extension type from the current addin.
            if (ctx.TryGetAddinType(Assembly.DeclaringAddin, trExtensionType.FullName, out extensionType))
                return true;

			// tries to find the extension type from the probable location (runtime or application).
			var tdExtensionType = trExtensionType.Resolve();
            if (ctx.TryGetProbableType(tdExtensionType.Module.Assembly.FullName, trExtensionType.FullName, out extensionType))
                return true;

            return false;
        }

		// @tdType is an implementation type of the IExtensionPoint<TExtension, TRoot>, IExtensionBuilder<TExtension>, or ICompositeExtensionBuilder<TExtension>
		// @trType is the same to the @tdType, only it's a TypeReference, and can be null.
		// @trExtensionType: the return value, notes that this value can not be of type GenericParameter.
		static bool TryResolveExtensionType(TypeDefinition tdType, TypeReference trType, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeReference trExtensionType)
        {
            var baseTypeDef = tdType;
            var interfaces = baseTypeDef.Interfaces;
            if (interfaces != null)
            {
                foreach (var @interface in interfaces)
                {
                    var result = CanResolveExtensionType(trType, @interface, baseGenericInterface, genericParamCount, out trExtensionType);
                    if (result)
                        return true;
                }
            }

            var baseTypeRef = baseTypeDef.BaseType;
            if (baseTypeRef == null)
            {
                trExtensionType = null;
                return false;
            }

            // 如果 baseTypeRef 是一个闭合泛型类型，则 mono.cecil 无法将其正确解析为对应的 TypeDefinition（会丢失 baseTypeRef 的泛型参数类型），
            // 因此这里手动将其泛型参数加入 baseTypeDef.GenericParameters 中，以便在获取其 interface 时，能够将泛型参数传给 interface。
            // 但这样获取的 interface 的泛型参数会变成 GenericParameter 类型，是不正确的，因此我们在 CanResolveExtensionType 方法再传入原来的 baseTypeRef，
            // 以便可以将 interface 的泛型参数与 baseTypeRef 比较，并直接从 baseTypeRef 中提取泛型参数。
            baseTypeDef = baseTypeRef.SafeResolve();

            if (baseTypeRef.IsGenericInstance)
            {
                var baseType = (GenericInstanceType)baseTypeRef;
                var genArgs = baseType.GenericArguments;
                for (int i = 0; i < genArgs.Count; i++)
                {
                    var genArg = genArgs[i];
                    baseTypeDef.GenericParameters[i] = new GenericParameter(genArg.FullName, genArg);
                }
            }

            // recursively call this method.
            return TryResolveExtensionType(baseTypeDef, baseTypeRef, baseGenericInterface, genericParamCount, out trExtensionType);
        }

		// @trType: the implementation type of the @subInterface.
		// @subInterface: a closed generic interfance of the @baseGenericDefinition.
		// @baseGenericDefinition: the open generic interface: IExtensionPoint<TExtension, TRoot>, IExtensionBuilder<TExtension>, or ICompositeExtensionBuilder<TExtension>
		static bool CanResolveExtensionType(TypeReference trType, TypeReference subInterface, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeReference trExtensionType)
        {
            trExtensionType = null;
            if (subInterface.IsGenericInstance) // a closed generic type
            {
                var genericDef = subInterface.GetGenericTypeDefinition();
                var matchingGenericInterface = genericDef.SafeResolve();
                if (matchingGenericInterface.EqualsTo(baseGenericInterface))
                    return CanResolveExtensionType(trType, subInterface, genericParamCount, out trExtensionType);
            }
            else
            {
                var subInterfaceDef = subInterface.SafeResolve();
                if (subInterfaceDef.Interfaces == null)
                    return false;
                foreach (var iface in subInterfaceDef.Interfaces)
                {
                    var result = CanResolveExtensionType(trType, iface, baseGenericInterface, genericParamCount, out trExtensionType);
                    if (result)
                        return true;
                }
            }
            
            return false;
        }

	    static bool CanResolveExtensionType(TypeReference trType, TypeReference subInterface, int genericParamCount, out TypeReference trExtensionType)
	    {
			var genericInstanceType = (GenericInstanceType)subInterface;
		    var genArgs = genericInstanceType.GenericArguments;
		    if (genArgs.Count != genericParamCount)
		    {
			    trExtensionType = null;
			    return false;
			}

	        var genArg = genArgs[0]; // TExtension 位

            if (trType == null || !trType.IsGenericInstance)
	        {
	            trExtensionType = genArg;
	            return true;
            }

	        var baseType = (GenericInstanceType)trType;
	        var genArgs2 = baseType.GenericArguments;
	        for (int i = 0; i < genArgs2.Count; i++)
	        {
	            var genArg2 = genArgs2[i];
	            if (genArg2.FullName == genArg.FullName)
	            {
	                trExtensionType = genArg2;
	                return true;
                }
	        }

	        trExtensionType = null;
            return false;
	    }
    }
}