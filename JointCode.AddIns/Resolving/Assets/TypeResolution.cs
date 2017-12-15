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
using Mono.Cecil;

namespace JointCode.AddIns.Resolving.Assets
{
    partial class TypeResolution
    {
        internal static TypeResolution Empty = new TypeResolution(null, null);
        static readonly TypeDefinition ExtensionPointInterface;
        static readonly TypeDefinition ExtensionBuilderInterface;
        static readonly TypeDefinition CompositeExtensionBuilderInterface;
        static readonly TypeDefinition ExtensionDataAttributeType;

        static TypeResolution()
        {
            var epMetadataToken = typeof(IExtensionPoint<,>).MetadataToken;
            var ebMetadataToken = typeof(IExtensionBuilder<>).MetadataToken;
            var cebMetadataToken = typeof(ICompositeExtensionBuilder<>).MetadataToken;
            var edaMetadataToken = typeof(ExtensionPropertyAttribute).MetadataToken;
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
                    if (ExtensionDataAttributeType == null && metadataToken == edaMetadataToken)
                        ExtensionDataAttributeType = type;
                }
            }
        }

        public static implicit operator TypeId(TypeResolution d)
        {
            return new TypeId(d._assembly.Uid, d.MetadataToken);
        }
    }

    /// <summary>
    /// Represent a type during resolution.
    /// Notes that to determine whether two <see cref="TypeResolution"/>s is the same, you can not use <see cref="object.ReferenceEquals"/>,
    /// try to use the <see cref="TypeResolution.Equals"/> instead.
    /// </summary>
    partial class TypeResolution : IEquatable<TypeResolution>
    {
        readonly AssemblyResolution _assembly;
        readonly TypeDefinition _typeDef;
        Type _type;
        List<PropertyInfo> _properties;

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

        internal bool IsAssignableFrom(TypeResolution subType)
        {
            return _typeDef.IsAssignableFrom(subType._typeDef);
        }

        #region IEquatable implementation
        public bool Equals(TypeResolution other)
        {
            return other != null && _assembly.Equals(other._assembly) && MetadataToken == other.MetadataToken;
        }
        #endregion

        internal Type GetRuntimeType()
        {
            if (_type != null)
                return _type;
            var assembly = _assembly.GetRuntimeAssembly();
            foreach (var module in assembly.GetModules())
            {
                _type = module.ResolveType(MetadataToken);
                if (_type != null)
                    break;
            }
            return _type;
        }

        internal List<PropertyInfo> GetSettableRuntimeProperties()
        {
            if (_properties != null)
                return _properties;
            var type = GetRuntimeType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;
                _properties = _properties ?? new List<PropertyInfo>();
                _properties.Add(property);
            }
            return _properties;
        }
    }

    partial class TypeResolution
    {
        internal bool ImplementsExtensionPointInterface(IMessageDialog dialog, ResolutionContext ctx, out TypeResolution type)
        {
            return TryGetGenericInterfaceImplementation(ctx, _typeDef, ExtensionPointInterface, 2, out type) != null;
        }

        internal bool ImplementsExtensionBuilderInterface(IMessageDialog dialog, ResolutionContext ctx, out TypeResolution type)
        {
            return TryGetGenericInterfaceImplementation(ctx, _typeDef, ExtensionBuilderInterface, 1, out type) != null;
        }

        internal bool ImplementsCompositeExtensionBuilderInterface(IMessageDialog dialog, ResolutionContext ctx, out TypeResolution type)
        {
            return TryGetGenericInterfaceImplementation(ctx, _typeDef, CompositeExtensionBuilderInterface, 1, out type) != null;
        }

        TypeReference TryGetGenericInterfaceImplementation(ResolutionContext ctx, 
            TypeDefinition classType, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeResolution extensionType)
        {
            extensionType = null;

            TypeReference trExtensionType;
            var result = TryGetGenericInterfaceImplementation(classType, baseGenericInterface, genericParamCount, out trExtensionType);
            if (result == null)
                return null;

            if (ctx.TryGetAddinType(Assembly.DeclaringAddin, trExtensionType.FullName, out extensionType))
                return result;

            var tdExtensionType = trExtensionType.Resolve();
            if (ctx.TryGetProbableType(tdExtensionType.Module.Assembly.FullName, trExtensionType.FullName, out extensionType))
                return result;

            return null;
        }

        // @classType is an implementation type of the IExtensionPoint<TExtension, TRoot>, IExtensionBuilder<TExtension>, or 
        // ICompositeExtensionBuilder<TExtension>
        static TypeReference TryGetGenericInterfaceImplementation(TypeDefinition classType, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeReference extensionType)
        {
            var subType = classType;
            var interfaces = subType.Interfaces;
            if (interfaces != null)
            {
                foreach (var @interface in interfaces)
                {
                    var result = DoTryGetGenericInterfaceImplementation(@interface, baseGenericInterface, genericParamCount, out extensionType);
                    if (result != null)
                        return result;
                }
            }

            var baseTypeRef = subType.BaseType;
            if (baseTypeRef == null)
            {
                extensionType = null;
                return null;
            }

            subType = baseTypeRef.SafeResolve();
            // recursively call this method.
            return TryGetGenericInterfaceImplementation(subType, baseGenericInterface, genericParamCount, out extensionType);
        }

        // @baseGenericDefinition: IExtensionPoint<TExtension, TRoot>, IExtensionBuilder<TExtension>, or ICompositeExtensionBuilder<TExtension>
        static TypeReference DoTryGetGenericInterfaceImplementation(TypeReference subInterface, TypeDefinition baseGenericInterface,
            int genericParamCount, out TypeReference extensionType)
        {
            extensionType = null;
            if (subInterface.IsGenericInstance) // a closed generic type
            {
                var genericDef = subInterface.GetGenericTypeDefinition();
                var matchingGenericInterface = genericDef.SafeResolve();
                if (ReferenceEquals(matchingGenericInterface, baseGenericInterface))
                {
                    var genericInstanceType = (GenericInstanceType)subInterface;
                    var genArgs = genericInstanceType.GenericArguments;
                    if (genArgs.Count == genericParamCount)
                    {
                        extensionType = genArgs[0];
                        return subInterface;
                    }
                }
            }
            else
            {
                var subInterfaceDef = subInterface.SafeResolve();
                if (subInterfaceDef.Interfaces == null)
                    return null;
                foreach (var iface in subInterfaceDef.Interfaces)
                {
                    var result = DoTryGetGenericInterfaceImplementation(iface, baseGenericInterface, genericParamCount, out extensionType);
                    if (result != null)
                        return result;
                }
            }
            
            return null;
        }
    }
}