//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Serialization;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Resolving
{
    static class ResolutionExtensions
    {
        /// <summary>
        /// The extension point type must implement the IExtensionPoint{TExtension, TRoot} interface (with 2 generic parameters).
        /// </summary>
        internal static bool InheritFromExtensionPointInterface(
            this ExtensionPointResolution ep, IMessageDialog dialog, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return ep.Type.ImplementsExtensionPointInterface(dialog, ctx, out extensionType);
        }

        /// <summary>
        /// The extension builder type must implement the IExtensionBuilder{TExtension} interface (with 1 generic parameter).
        /// </summary>
        internal static bool InheritFromExtensionBuilderInterface(
            this ExtensionBuilderResolution eb, IMessageDialog dialog, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return eb.Type.ImplementsExtensionBuilderInterface(dialog, ctx, out extensionType);
        }

        internal static bool InheritFromCompositeExtensionBuilderInterface(
            this ExtensionBuilderResolution eb, IMessageDialog dialog, ResolutionContext ctx, out TypeResolution extensionType)
        {
            return eb.Type.ImplementsCompositeExtensionBuilderInterface(dialog, ctx, out extensionType);
        }

        /// <summary>
        /// The extension type of the extension builders must be the same as that of its parent.
        /// </summary>
        internal static bool ExtensionTypeMatchesParent(this ExtensionBuilderResolution eb, IMessageDialog dialog, TypeResolution extensionType)
        {
            var result = extensionType.Equals(eb.Parent.ExtensionType); // can not use ReferenceEquals here!!!!
            var parenteEb = eb.Parent as ExtensionBuilderResolution;
            if (!result)
                dialog.AddError(string.Format(
                    "The extension type of extension builder must match that of its parent, while the extension type of the extension builder [{0}] is [{1}], and that of its parent [{2}] is [{3}], which does not match!", eb.Path, extensionType.TypeName, parenteEb != null ? parenteEb.Path : eb.Parent.Id, eb.Parent.ExtensionType.TypeName));
            return result;
        }

        /// <summary>
        /// The extension point and extension builder type must have a public parameter-less constructor.
        /// </summary>
        internal static bool HasPublicParameterLessConstructor(this BaseExtensionPointResolution epOreb)
        {
            var ctors = epOreb.Type.GetConstructors();
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
        internal static bool DeclaresInSameAddin(this BaseExtensionPointResolution epOreb)
        {
            return ReferenceEquals(epOreb.DeclaringAddin, epOreb.Type.Assembly.DeclaringAddin);
        }

        /// <summary>
        /// The extension builders used to build the current extension and the sibling extension (if exists) must be a child of that 
        /// of the parent extension or extension point.
        /// </summary>
        internal static bool ExtensionBuildersMatchParent(this ExtensionResolution ex, IMessageDialog dialog)
        {
            return ex.Sibling == null
                ? ExtensionBuilderMatchesParent(ex, dialog)
                : ExtensionBuilderMatchesParent(ex, dialog) && ExtensionBuilderMatchesParent(ex.Sibling, dialog);
        }

        static bool ExtensionBuilderMatchesParent(ExtensionResolution ex, IMessageDialog dialog)
        {
            var extensionBuilder = ex.ExtensionBuilder;
            var extensionPoint = ex.Parent as ExtensionPointResolution;
            bool result;
            if (extensionPoint != null)
            {
                result = extensionPoint.Children != null && extensionPoint.Children.Contains(extensionBuilder);
                if (!result)
                    dialog.AddError(string.Format("The extension builder of the extension [{0}] is not a child of that of the extension point [{1}]!", ex.Head.Path, extensionPoint.Id));
                return result;
            }

            var parentObj = ex.Parent as ExtensionResolution;
            // the child extension builders of the extension builder for parent extension.
            result = parentObj.ExtensionBuilder.Children.Contains(extensionBuilder);
            if (!result)
                dialog.AddError(string.Format("The extension builder of the extension [{0}] is not a child of that of its parent [{1}]!", ex.Head.Path, parentObj.Head.Path));
            return result;
        }

        // Notes that this method will load the related assembly which defined the IExtensionBuilder implementation into memory.
        /// <summary>
        /// The required properties of extension builder (marked by ExtensionDataAttribute) must be provided by the extension data.
        /// </summary>
        /// <returns></returns>
        internal static bool ExtensionDataMatchesExtensionBuilder(this ExtensionResolution ex, IMessageDialog dialog,
            ResolutionContext ctx, ConvertionManager convertionManager)
        {
            var ebProperties = ex.ExtensionBuilder.Type.GetSettableRuntimeProperties();
            if (ebProperties == null)
                return true;

            var data = ex.Data;
            foreach (var ebProp in ebProperties)
            {
                var propName = ebProp.Name;
                string propInput;
                if (!data.TryGetString(propName, out propInput))
                {
                    var exPropAttrib = ebProp.GetCustomAttribute<ExtensionPropertyAttribute>(false, false);
                    if (exPropAttrib != null && exPropAttrib.Required)
                    {
                        dialog.AddError("a required property is missing!");
                        return false;
                    }
                    continue;
                }

                #region specific types
                if (ebProp.PropertyType == typeof(string))
                {
                    var holder = new StringHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }

                // convert to TypeId
                if (ebProp.PropertyType == typeof(TypeId))
                {
                    TypeResolution type;
                    // a type dependency is introduced here. 
                    // should it be added to the current addin's reference set?
                    if (!ctx.TryGetAddinType(ex.DeclaringAddin, propInput, out type))
                    {
                        dialog.AddError("");
                        return false;
                    }
                    var holder = new LazyTypeIdHolder(type);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                } 
                #endregion

                // convert to custom type (with an ObjectConverter registered in ConvertionManager).
                var objectConverter = convertionManager.TryGet(typeof(string), ebProp.PropertyType);
                if (objectConverter == null)
                {
                    dialog.AddError("No converter is registered for !");
                    return false;
                }

                // if an property value is provided for the property name, try to convert it.
                object propValue;
                if (!objectConverter.TryConvert(propInput, out propValue))
                {
                    dialog.AddError("The string [] can not be converted to !");
                    return false;
                }

                #region common types
                if (ebProp.PropertyType == typeof(Int32))
                {
                    var holder = new Int32Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Boolean))
                {
                    var holder = new BooleanHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Version))
                {
                    var holder = new VersionHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(DateTime))
                {
                    var holder = new DateTimeHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Guid))
                {
                    var holder = new GuidHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(TimeSpan))
                {
                    var holder = new TimeSpanHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Int64))
                {
                    var holder = new Int64Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(UInt64))
                {
                    var holder = new UInt64Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(UInt32))
                {
                    var holder = new UInt32Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Int16))
                {
                    var holder = new Int16Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(UInt16))
                {
                    var holder = new UInt16Holder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Byte))
                {
                    var holder = new ByteHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(SByte))
                {
                    var holder = new SByteHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Char))
                {
                    var holder = new CharHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Decimal))
                {
                    var holder = new DecimalHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Double))
                {
                    var holder = new DoubleHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                if (ebProp.PropertyType == typeof(Single))
                {
                    var holder = new SingleHolder(propInput);
                    data.AddSerializableHolder(propName, holder);
                    continue;
                }
                #endregion
            }

            return true;
        }
    }

    class LazyTypeIdHolder : TypeIdHolder
    {
        internal LazyTypeIdHolder() { }
        internal LazyTypeIdHolder(TypeResolution val) : base(val) { }

        internal override void Write(Stream writer)
        {
            var type = _val as TypeResolution;
            var typeId = new TypeId(type.Assembly.Uid, type.MetadataToken);
            typeId.Write(writer);
        }
    }
}
