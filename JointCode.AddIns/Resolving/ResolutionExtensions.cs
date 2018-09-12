//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;
using Mono.Cecil;
using System.Collections.Generic;
using JointCode.AddIns.Core.Data;

namespace JointCode.AddIns.Resolving
{
    static partial class ResolutionExtensions
    {
        /// <summary>
        /// The addin activator type must implement the IAddinActivator interface.
        /// </summary>
        internal static bool InheritFromAddinActivatorInterface(
            this TypeConstrainedResolvable resolvable, ResolutionResult resolutionResult, ResolutionContext ctx)
        {
            return resolvable.Type.ImplementsAddinActivatorInterface(resolutionResult, ctx);
        }

        /// <summary>
        /// The extension point type must implement the IExtensionPoint{TExtension, TRoot} interface (with 2 generic parameters).
        /// </summary>
        internal static bool InheritFromExtensionPointInterface(
            this TypeConstrainedResolvable resolvable, ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return resolvable.Type.ImplementsExtensionPointInterface(resolutionResult, ctx, out extensionType);
        }

        /// <summary>
        /// The extension builder type must implement the IExtensionBuilder{TExtension} interface (with 1 generic parameter).
        /// </summary>
        internal static bool InheritFromExtensionBuilderInterface(
            this TypeConstrainedResolvable resolvable, ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return resolvable.Type.ImplementsExtensionBuilderInterface(resolutionResult, ctx, out extensionType);
        }

        internal static bool InheritFromCompositeExtensionBuilderInterface(
            this TypeConstrainedResolvable eb, ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return eb.Type.ImplementsCompositeExtensionBuilderInterface(resolutionResult, ctx, out extensionType);
        }

        /// <summary>
        /// The extension type of the extension builders must be the same as that of its parent.
        /// </summary>
        internal static bool ExtensionTypeMatchesParent(this ExtensionBuilderResolution eb, ResolutionResult resolutionResult, TypeResolution extensionType)
        {
            var result = extensionType.Equals(eb.Parent.ExtensionType); // can not use ReferenceEquals here!!!!
            var parenteEb = eb.Parent as ExtensionBuilderResolution;
            if (!result)
                resolutionResult.AddError(string.Format(
                    "The extension type of extension builder must match that of its parent, while the extension type of the extension builder [{0}] is [{1}], and that of its parent [{2}] is [{3}], which does not match!", eb.Path, extensionType.TypeName, parenteEb != null ? parenteEb.Path : eb.Parent.Name, eb.Parent.ExtensionType.TypeName));
            return result;
        }

        /// <summary>
        /// The specified type must have a public parameter-less constructor.
        /// </summary>
        internal static bool HasPublicParameterLessConstructor(this TypeResolution type)
        {
            var ctors = type.GetConstructors();
            if (ctors == null)
                return false;
            foreach (var ctor in ctors)
            {
                if (!ctor.IsPublic)
                    continue;
                var parameters = ctor.GetParameters();
                if (parameters == null || parameters.Count == 1)
                    return true;
            }
            return false;
            //var parameters = ctors[0].GetParameters();
            //return parameters == null;
            //return ctors != null && ctors.Count == 1;
        }

        /// <summary>
        /// The extension builder or extension point must be declared in the addin that defined its type.
        /// </summary>
        internal static bool DeclaresInSameAddin(this TypeConstrainedResolvable resolvable)
        {
            return ReferenceEquals(resolvable.DeclaringAddin, resolvable.Type.Assembly.DeclaringAddin);
        }

        /// <summary>
        /// The extension builders used to build the current extension and the sibling extension (if exists) must be a child of that 
        /// of the parent extension or extension point.
        /// </summary>
        internal static bool ExtensionBuildersMatchParent(this ExtensionResolution ex, ResolutionResult resolutionResult)
        {
            return ex.Sibling == null
                ? ExtensionBuilderMatchesParent(ex, resolutionResult)
                : ExtensionBuilderMatchesParent(ex, resolutionResult) && ExtensionBuilderMatchesParent(ex.Sibling, resolutionResult);
        }

        static bool ExtensionBuilderMatchesParent(ExtensionResolution ex, ResolutionResult resolutionResult)
        {
            var extensionBuilder = ex.ExtensionBuilder;

            var parentExtensionPoint = ex.Parent as ExtensionPointResolution;
            bool result;
            if (parentExtensionPoint != null)
            {
                result = parentExtensionPoint.Children != null 
                    && (parentExtensionPoint.Children.Contains(extensionBuilder) 
                        || parentExtensionPoint.Path == extensionBuilder.ParentPath); // 这是一个插件扩展了另外一个插件的 ExtensionSchema
                if (!result)
                    resolutionResult.AddError(string.Format("The extension builder of the extension [{0}] is not a child of that of the extension point [{1}]!", ex.Head.Path, parentExtensionPoint.Name));
                return result;
            }

            var parentExtension = ex.Parent as ExtensionResolution;
            // the child extension builders of the extension builder for parent extension.
            result = parentExtension.ExtensionBuilder.Children.Contains(extensionBuilder) 
                        || parentExtension.ExtensionBuilder.Path == extensionBuilder.ParentPath; // 这是一个插件扩展了另外一个插件的 ExtensionSchema
            if (!result)
                resolutionResult.AddError(string.Format("The extension builder of the extension [{0}] is not a child of that of its parent [{1}]!", ex.Head.Path, parentExtension.Head.Path));

            return result;
        }

        static bool ExtensionPropertyIsRequired(PropertyDefinition ebProp)
        {
            if (!ebProp.HasCustomAttributes)
                return false;
            foreach (var customAttrib in ebProp.CustomAttributes)
            {
                var attribType = customAttrib.AttributeType.SafeResolve();
                if (!customAttrib.HasProperties
                    || attribType.MetadataToken != TypeResolution.ExtensionPropertyAttributeType.MetadataToken
                    || attribType.Module.Assembly != AssemblyResolution.ThisAssembly)
                    continue;
                foreach (var customProp in customAttrib.Properties)
                {
                    if (customProp.Name == "Required" && customProp.Argument.Value.Equals(true))
                    {
                        //resolutionResult.AddError("a required property is missing!");
                        return true;
                    }
                }
            }
            return false;
            //var exPropAttrib = ebProp.GetCustomAttribute<ExtensionPropertyAttribute>(false, false);
            //if (exPropAttrib != null && exPropAttrib.Required)
            //{
            //    resolutionResult.AddError("a required property is missing!");
            //    return false;
            //}
        }

        static bool HasRequiredExtensionProperties(ResolutionResult resolutionResult, IEnumerable<PropertyDefinition> ebProps)
        {
            foreach (var ebProp in ebProps)
            {
                if (ExtensionPropertyIsRequired(ebProp))
                {
                    resolutionResult.AddError("a required property is missing!");
                    return false;
                }
            }
            return true;
        }

        static bool ExtensionDataMatchesExtensionProperties(ResolutionContext ctx, ConvertionManager convertionManager, 
            ExtensionResolution ex, ResolutionResult resolutionResult, IEnumerable<PropertyDefinition> ebProps)
        {
            var data = ex.Data;
            if (ex is NewOrUpdatedExtensionResolution)
            {
                foreach (var ebProp in ebProps)
                {
                    var propName = ebProp.Name;
                    string propData; 
                    if (!data.TryGetString(propName, out propData))
                    {
                        if (ExtensionPropertyIsRequired(ebProp))
                        {
                            resolutionResult.AddError(string.Format("A required property [{0}] of extension builder [{1}] is not provided with a data!", ebProp.Name, ex.ExtensionBuilder.Type.TypeName));
                            return false;
                        }
                        continue;
                    }

                    var propType = ebProp.PropertyType.SafeResolve();

                    DataTransformer dataTransformer;
                    if (!ctx.TryGetDataTransformer(propType.Module.Assembly.FullName, propType.MetadataToken.ToInt32(), out dataTransformer))
                    {
                        resolutionResult.AddError(string.Format("No data transformer is registered for type [{0}]!", propType.FullName));
                        return false;
                    }

                    if (!dataTransformer.Transform(propName, propData, ctx, convertionManager, ex))
                    {
                        resolutionResult.AddError(string.Format("Can not transform from value [{1}] to type [{0}]!", propType.FullName, propData));
                        return false;
                    }
                }
            }
            else
            {
                foreach (var ebProp in ebProps)
                {
                    var propName = ebProp.Name;
                    DataHolder dataHolder;
                    if (!data.TryGetDataHolder(propName, out dataHolder))
                    {
                        if (ExtensionPropertyIsRequired(ebProp))
                        {
                            resolutionResult.AddError(string.Format("A required property [{0}] of extension builder [{1}] is not provided with a data!", ebProp.Name, ex.ExtensionBuilder.Type.TypeName));
                            return false;
                        }
                        continue;
                    }

                    var propType = ebProp.PropertyType.SafeResolve();

                    DataTransformer dataTransformer;
                    if (!ctx.TryGetDataTransformer(propType.Module.Assembly.FullName, propType.MetadataToken.ToInt32(), out dataTransformer))
                    {
                        resolutionResult.AddError(string.Format("No data transformer is registered for type [{0}]!", propType.FullName));
                        return false;
                    }

                    if (!dataTransformer.CanTransform(dataHolder))
                    {
                        resolutionResult.AddError(string.Format("Can not transform from value [{1}] to type [{0}]!", propType.FullName, dataHolder.Value));
                        return false;
                    }
                }
            }

            return true;
        }

        // Notes that this method will load the related assembly which defined the IExtensionBuilder implementation into memory.
        /// <summary>
        /// The required properties of extension builder (marked by ExtensionDataAttribute) must be provided by the extension data.
        /// </summary>
        /// <returns></returns>
        internal static bool ExtensionDataMatchesExtensionBuilder(this ExtensionResolution ex, ResolutionResult resolutionResult,
            ResolutionContext ctx, ConvertionManager convertionManager)
        {
            if (!ex.ExtensionBuilder.Type.HasProperties)
                return true;

            var ebProps = ex.ExtensionBuilder.Type.Properties;

            return ex.Data == null 
                ? HasRequiredExtensionProperties(resolutionResult, ebProps) 
                : ExtensionDataMatchesExtensionProperties(ctx, convertionManager, ex, resolutionResult, ebProps);
        }
    }
}
