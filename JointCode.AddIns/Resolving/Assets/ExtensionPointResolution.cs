//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Resolving.Assets
{
    internal interface IExtensionPointPathInfo
    {
        string Name { get; set; }
        //string Path { get; }
    }

    abstract class BaseExtensionPointResolution : TypeConstrainedResolvable
    {
        List<ExtensionBuilderResolution> _children;

        protected BaseExtensionPointResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        public string Name { get; set; }
        internal string Description { get; set; }
        internal string TypeName { get; set; }

        internal List<ExtensionBuilderResolution> Children { get { return _children; } }
        
        #region Dependences
        // the extension type of the extension point/builder
        internal TypeResolution ExtensionType { get; set; }
        //// the extension point/builder type
        //internal TypeResolution Type { get; set; }
        #endregion

        internal void AddChild(ExtensionBuilderResolution item)
        {
            _children = _children ?? new List<ExtensionBuilderResolution>();
            item.Parent = this;
            _children.Add(item);
        }
    }

    abstract class ExtensionPointResolution : BaseExtensionPointResolution, IExtensionPointPathInfo
    {
        internal static ExtensionPointResolution Empty = new NewOrUpdatedExtensionPointResolution(null);

        internal ExtensionPointResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        internal string Path { get { return ExtensionHelper.GetExtensionPointPath(this); } }

        internal abstract int Uid { get; }

        internal TypeResolution ExtensionRootType { get; set; }

        internal abstract ExtensionPointRecord ToRecord();

        // todo: extensionRootType：将会由 IExtensionPoint<TExtension, TExtensionRoot> 的实现类所在的程序集去引用 ExtensionType 和 ExtensionRootType 所在的程序集，最后的结果会添加到 ReferencedAssemblies，此处不必解析
        // apply some rules to extension point.
        protected bool ApplyRules(ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType, out TypeResolution extensionRootType)
        {
            var result = true;
            if (!Type.IsClass || Type.IsAbstract)
            {
                resolutionResult.AddError(string.Format("The specified extension point type [{0}] is not a concrete class!", Type.TypeName));
                result = false;
            }
            //// An extension point can be declared in an addin (extension schema), yet defined in another addin, thus we don't need the following check.
            //if (!this.DeclaresInSameAddin())
            //{
            //    resolutionResult.AddError(string.Format(
            //        "The extension point type [{0}] is expected to be defined and declared in a same addin, while its defining addin is [{1}], and its declaring addin is [{2}], which is not the same as the former!", 
            //        Type.TypeName, Type.Assembly.DeclaringAddin.AddinId.Guid, DeclaringAddin.AddinId.Guid));
            //    result = false;
            //}


            extensionRootType = null;
            if (!this.InheritFromExtensionPointInterface(resolutionResult, ctx, out extensionType))
            {
                resolutionResult.AddError(string.Format("The specified extension point type [{0}] does not implement the required interface (IExtensionPoint<TExtension, TRoot>)!", Type.TypeName));
                result = false;
            }
            if (!this.Type.HasPublicParameterLessConstructor())
            {
                resolutionResult.AddError(string.Format("The specified extension point type [{0}] do not have a public parameter-less constructor!", Type.TypeName));
                result = false;
            }
            return result;
        }
    }

    abstract class ResolvableExtensionPointResolution : ExtensionPointResolution
    {
        internal ResolvableExtensionPointResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        // The resolution of an extension point depends on the existence of its implementation type (IExtensionPoint<TExtension, TRoot>), 
        // extension type (TExtension) and root type (TRoot), and it needs to obey some rules.
        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                {
                    resolutionResult.AddError(string.Format("Can not find the specified extension point type [{0}]!", TypeName));
                    return ResolutionStatus.Failed;
                }

                TypeResolution extensionType, extensionRootType;
                if (!ApplyRules(resolutionResult, ctx, out extensionType, out extensionRootType))
                    return ResolutionStatus.Failed;
                ExtensionType = extensionType;
                ExtensionRootType = extensionRootType;

                if (Type.Assembly.DeclaringAddin != null &&
                    !ReferenceEquals(Type.Assembly.DeclaringAddin, DeclaringAddin))
                {
                    AssemblyResolutionSet assemblySet;
                    if (!ctx.TryGetAssemblySet(Type.Assembly.AssemblyKey, out assemblySet))
                        throw new Exception();
                    DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                }
            }

            return ResolveType(Type);
        }
    }

    // New or updated or affected extension point
    class NewOrUpdatedExtensionPointResolution : ResolvableExtensionPointResolution
    {
        int _uid;

        internal NewOrUpdatedExtensionPointResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override int Uid { get { return _uid; } }

        internal override ExtensionPointRecord ToRecord()
        {
            _uid = UidStorage.GetNextExtensionPointUid();
            var result = new ExtensionPointRecord
            {
                Name = Name,
                Description = Description,
                TypeName = TypeName,
                Uid = _uid,
                AssemblyUid = Type.Assembly.Uid
            };
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    var childItem = child.ToRecord();
                    result.AddChild(childItem);
                }
            }
            return result;
        }
    }

    class DirectlyAffectedExtensionPointResolution : ResolvableExtensionPointResolution
    {
        readonly ExtensionPointRecord _old;

        internal DirectlyAffectedExtensionPointResolution(AddinResolution declaringAddin, ExtensionPointRecord old)
            : base(declaringAddin) { _old = old; }

        internal override int Uid { get { return _old.Uid; } }

        internal override ExtensionPointRecord ToRecord()
        {
            var result = new ExtensionPointRecord
            {
                Name = Name,
                Description = Description,
                TypeName = TypeName,
                Uid = _old.Uid,
                AssemblyUid = Type.Assembly.Uid
            };
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    var childItem = child.ToRecord();
                    result.AddChild(childItem);
                }
            }
            return result;
        }

        // The extension type and root type of an extension point might be gone or changed, but that will be a problem of the 
        // resolution of assembly references.
    }

    class IndirectlyAffectedExtensionPointResolution : ExtensionPointResolution
    {
        readonly ExtensionPointRecord _old;

        internal IndirectlyAffectedExtensionPointResolution(AddinResolution declaringAddin, ExtensionPointRecord old)
            : base(declaringAddin) { _old = old; }

        internal override int Uid { get { return _old.Uid; } }

        internal override ExtensionPointRecord ToRecord()
        {
            return _old;
        }

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                    return ResolutionStatus.Failed;

                TypeResolution extensionType, extensionRootType;
                if (!ApplyRules(resolutionResult, ctx, out extensionType, out extensionRootType))
                    return ResolutionStatus.Failed;
                ExtensionType = extensionType;
                ExtensionRootType = extensionRootType;
            }

            return ResolveType(Type);
        }
    }

    class UnaffectedExtensionPointResolution : IndirectlyAffectedExtensionPointResolution
    {
        //readonly ExtensionPointRecord _old;

        internal UnaffectedExtensionPointResolution(AddinResolution declaringAddin, ExtensionPointRecord old)
            : base(declaringAddin, old) { /*_old = old;*/ }

        //internal override int Uid { get { return _old.Uid; } }

        //internal override ExtensionPointRecord ToRecord()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        //{
        //    return ResolutionStatus.Success;
        //}
    }
}