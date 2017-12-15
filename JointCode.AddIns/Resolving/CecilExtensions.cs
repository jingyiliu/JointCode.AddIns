//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using Mono.Cecil;

namespace JointCode.AddIns.Resolving
{
    static class CecilExtensions
    {
        // true = typeof (BaseResolution).IsAssignableFrom(typeof (ExtensionBuilderResolution));
        internal static bool IsAssignableFrom(this TypeDefinition self, TypeDefinition subType)
        {
            if (self.IsValueType || self.IsSealed)
                return false;
            
            if (ReferenceEquals(self, subType))
                return true;

            if (self.IsInterface)
                return subType.ImplementsInterface(self);

            if (self.IsClass)
                return IsSubclassOf(self, subType);

            return false;
        }

        /// <summary>
        /// Determines whether the <see cref="subType"/> is a subclass of <see cref="self"/>.
        /// Notes that the <see cref="self"/> must be a class.
        /// </summary>
        internal static bool IsSubclassOf(this TypeDefinition self, TypeDefinition subType)
        {
            if (!self.IsClass)
                throw new InvalidOperationException("");
            if (!subType.IsClass)
                return false;
            var type = subType;
            while (true)
            {
                if (ReferenceEquals(type, self)) // how to determine that 2 TypeDefinition equals?
                    return true;
                var baseTypeRef = type.BaseType;
                if (baseTypeRef == null)
                    break;
                type = baseTypeRef.SafeResolve();
            }
            return false;
        }

        /// <summary>
        /// Determines whether the <see cref="self"/> implements the specified <see cref="@interface"/>.
        /// Notes that the <see cref="@interface"/> must be an interface.
        /// </summary>
        internal static bool ImplementsInterface(this TypeDefinition self, TypeDefinition @interface)
        {
            if (!@interface.IsInterface)
                throw new InvalidOperationException("");

            // @self is an interface
            if (self.IsInterface)
                return DoImplementsInterface(self, @interface);

            // @self is a class
            var type = self;
            while (true)
            {
                var interfaces = type.Interfaces;
                if (interfaces != null)
                {
                    foreach (var iface in interfaces)
                    {
                        var ifaceDef = iface.SafeResolve();
                        if (DoImplementsInterface(ifaceDef, @interface))
                            return true;
                    }
                }
                var baseTypeRef = type.BaseType;
                if (baseTypeRef == null)
                    break;
                type = baseTypeRef.SafeResolve();
            }
            return false;
        }

        static bool DoImplementsInterface(TypeDefinition subInterface, TypeDefinition baseInterface)
        {
            if (ReferenceEquals(subInterface, baseInterface)) // how to determine that 2 TypeDefinition equals?
                return true;
            if (subInterface.Interfaces == null)
                return false;
            foreach (var iface in subInterface.Interfaces)
            {
                var ifaceDef = iface.SafeResolve();
                if (DoImplementsInterface(ifaceDef, baseInterface))
                    return true;
            }
            return false;
        }

        internal static TypeReference GetGenericTypeDefinition(this TypeDefinition self)
        {
            return !self.IsGenericInstance ? null : self.GetElementType();
        }

        internal static TypeReference GetGenericTypeDefinition(this TypeReference self)
        {
            return !self.IsGenericInstance ? null : self.GetElementType();
        }

        // determine whether the given assembly is provided by runtime (.net or mono) or application which can be referenced any way.
        internal static bool IsProbableAssembly(this AssemblyNameReference self, ReaderParameters cecilReaderParameters)
        {
            return cecilReaderParameters.AssemblyResolver.Resolve(self) != null;
        }

        // determine whether the given assembly is provided by runtime (.net or mono) or application which can be referenced any way.
        internal static bool IsProbableAssembly(this AssemblyNameDefinition self, ReaderParameters cecilReaderParameters)
        {
            return cecilReaderParameters.AssemblyResolver.Resolve(self) != null;
        }

        internal static TypeDefinition SafeResolve(this TypeReference self)
        {
            return self.Resolve();
        }
    }
}