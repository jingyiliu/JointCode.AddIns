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
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving.Assets
{
    abstract class BaseExtensionPointResolution : Resolvable
    {
        List<ExtensionBuilderResolution> _children;

        protected BaseExtensionPointResolution(AddinResolution declaringAddin) : base(declaringAddin) { }
    	
        internal string Id { get; set; }
        internal string Description { get; set; }
        internal string TypeName { get; set; }

        internal List<ExtensionBuilderResolution> Children { get { return _children; } }
        
        #region Dependences
        // the extension point/builder type
        internal TypeResolution Type { get; set; }
        // the extension type of the extension point/builder
        internal TypeResolution ExtensionType { get; set; }
        #endregion

        internal void AddChild(ExtensionBuilderResolution item)
        {
            _children = _children ?? new List<ExtensionBuilderResolution>();
            item.Parent = this;
            _children.Add(item);
        }
    }

    abstract class ExtensionPointResolution : BaseExtensionPointResolution
    {
        internal static ExtensionPointResolution Empty = new NewExtensionPointResolution(null);

        internal ExtensionPointResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        internal abstract int Uid { get; }

        internal abstract ExtensionPointRecord ToRecord();

        // apply some rules to extension point.
        protected bool ApplyRules(IMessageDialog dialog, ResolutionContext ctx, out TypeResolution extensionType)
        {
            var result = true;
            if (!Type.IsClass || Type.IsAbstract)
            {
                dialog.AddError(string.Format("The specified extension point type [{0}] is not a concrete class!", Type.TypeName));
                result = false;
            }
            //if (!this.DeclaresInSameAddin())
            //{
            //    dialog.AddError(string.Format("The extension point type [{0}] is expected to be defined and declared in a same addin, while its defining addin is [{1}], and its declaring addin is [{2}], which is not the same as the former!", Type.TypeName, Type.Assembly.DeclaringAddin.AddinId.Guid, DeclaringAddin.AddinId.Guid));
            //    result = false;
            //}
            if (!this.InheritFromExtensionPointInterface(dialog, ctx, out extensionType))
            {
                dialog.AddError(string.Format("The specified extension point type [{0}] does not implement the required interface (IExtensionPoint<TExtension, TRoot>)!", Type.TypeName));
                result = false;
            }
            if (!this.HasPublicParameterLessConstructor())
            {
                dialog.AddError(string.Format("The specified extension point type [{0}] do not have a public parameter-less constructor!", Type.TypeName));
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
        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                {
                    dialog.AddError(string.Format("Can not find the specified extension point type [{0}]!", TypeName));
                    return ResolutionStatus.Failed;
                }

                TypeResolution extensionType;
                if (!ApplyRules(dialog, ctx, out extensionType))
                    return ResolutionStatus.Failed;
                ExtensionType = extensionType;

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
    class NewExtensionPointResolution : ResolvableExtensionPointResolution
    {
        int _uid;

        internal NewExtensionPointResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override int Uid { get { return _uid; } }

        internal override ExtensionPointRecord ToRecord()
        {
            _uid = IndexManager.GetNextExtensionPointUid();
            var result = new ExtensionPointRecord
            {
                Id = Id,
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
                Id = Id,
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
            throw new NotImplementedException();
        }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                    return ResolutionStatus.Failed;

                TypeResolution extensionType;
                if (!ApplyRules(dialog, ctx, out extensionType))
                    return ResolutionStatus.Failed;
                ExtensionType = extensionType;
            }

            return ResolveType(Type);
        }
    }

    class UnaffectedExtensionPointResolution : ExtensionPointResolution
    {
        readonly ExtensionPointRecord _old;

        internal UnaffectedExtensionPointResolution(AddinResolution declaringAddin, ExtensionPointRecord old)
            : base(declaringAddin) { _old = old; }

        internal override int Uid { get { return _old.Uid; } }

        internal override ExtensionPointRecord ToRecord()
        {
            throw new NotImplementedException();
        }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}