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
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving.Assets
{
    class ExtensionBuilderResolutionGroup
    {
        List<ExtensionBuilderResolution> _children;
        internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension builder
        internal List<ExtensionBuilderResolution> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderResolution item)
        {
            _children = _children ?? new List<ExtensionBuilderResolution>();
            _children.Add(item);
        }

        internal ExtensionBuilderRecordGroup ToRecord()
        {
            var ebGroup = new ExtensionBuilderRecordGroup { ParentPath = ParentPath };
            foreach (var child in Children)
                ebGroup.AddChild(child.ToRecord());
            return ebGroup;
        }
    }

    abstract class ExtensionBuilderResolution : BaseExtensionPointResolution
    {
        internal static ExtensionBuilderResolution Empty = new NewDeclaredExtensionBuilderResolution(null);
        string _path;
    	
        internal ExtensionBuilderResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        internal abstract ExtensionBuilderKind ExtensionBuilderKind { get; }
        internal abstract int Uid { get; }
        internal abstract int AssemblyUid { get; }

        internal string ExtensionPointId { get; set; }

        // because the id of extension builder must be unique with extension point, we can simplely 
        // use the [ExtensionPoint.Id + SysConstants.PathSeparator + ExtensionBuilder.Id] to represent 
        // its path, no matter how deep it is in the extension point.
        internal string Path
        {
            get
            {
                if (_path != null)
                    return _path;
                _path = ExtensionPointId + SysConstants.PathSeparator + Id;
                return _path;
            }
        }
    	
        internal bool ParentIsExtensionPoint { get; set; }
        internal string ParentPath { get; set; }

        #region Dependences
        // the parent of an extension builder can be an extension point or another extension builder
        internal BaseExtensionPointResolution Parent { get; set; }
        #endregion

        internal abstract bool Equals(ExtensionBuilderResolution other);

        internal abstract ExtensionBuilderRecord ToRecord();
    }

    #region Declared
    abstract class DeclaredExtensionBuilderResolution : ExtensionBuilderResolution
    {
        internal DeclaredExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        // The resolution of an extension builder depends on the existence of its implementation type 
        // (IExtensionBuilder<TExtension> / ICompositeExtensionBuilder<TExtension>) and extension type (TExtension),
        // and it obey some rules.
        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                BaseExtensionPointResolution objParent;
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(dialog, ParentPath, out parent))
                    {
                        dialog.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    objParent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(dialog, ParentPath, out parent))
                    {
                        dialog.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    objParent = parent;
                }

                if (objParent.DeclaringAddin != null && !ReferenceEquals(objParent.DeclaringAddin, DeclaringAddin))
                {
                    if (objParent.Type == null)
                        return ResolutionStatus.Pending;
                    // The parent of current extension builder is not defined in the same addin as it is,
                    // so we needs to add its declaring addin as a reference (the type of the parent must 
                    // be loaded before this extension builder at start up).
                    AssemblyResolutionSet assemblySet;
                    if (!ctx.TryGetAssemblySet(objParent.Type.Assembly.AssemblyKey, out assemblySet))
                        throw new Exception();
                    DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                }

                Parent = objParent;
            }

            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                {
                    dialog.AddError(string.Format("Can not find the specified extension builder type [{0}]!", TypeName));
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

            return ResolveAddin(Parent) | ResolveType(Type);
        }

        // apply some rules.
        bool ApplyRules(IMessageDialog dialog, ResolutionContext ctx, out TypeResolution extensionType)
        {
            var result = true;
            if (!Type.IsClass || Type.IsAbstract)
            {
                dialog.AddError(string.Format("The specified extension builder type [{0}] is not a concrete class!", Type.TypeName));
                result = false;
            }
            //if (!this.DeclaresInSameAddin())
            //{
            //    dialog.AddError(string.Format("The extension builder type [{0}] is expected to be defined and declared in a same addin, while its defining addin is [{1}], and its declaring addin is [{2}], which is not the same as the former!", Type.TypeName, Type.Assembly.DeclaringAddin.AddinId.Guid, DeclaringAddin.AddinId.Guid));
            //    result = false;
            //}

            if (Children != null)
            {
                if (!this.InheritFromCompositeExtensionBuilderInterface(dialog, ctx, out extensionType))
                {
                    dialog.AddError(string.Format("The specified extension builder type [{0}] does not implement the required interface (ICompositeExtensionBuilder<TExtension>)!", Type.TypeName));
                    result = false;
                }
            }
            else
            {
                if (!this.InheritFromExtensionBuilderInterface(dialog, ctx, out extensionType))
                {
                    dialog.AddError(string.Format("The specified extension builder type [{0}] does not implement the required interface (IExtensionBuilder<TExtension>)!", Type.TypeName));
                    result = false;
                }
            }

            if (!this.HasPublicParameterLessConstructor())
            {
                dialog.AddError(string.Format("The specified builder point type [{0}] do not have a public parameter-less constructor!", Type.TypeName));
                result = false;
            }

            if (!this.ExtensionTypeMatchesParent(dialog, extensionType))
            {
                result = false;
            }
            return result;
        }
    }

    class NewDeclaredExtensionBuilderResolution : DeclaredExtensionBuilderResolution
    {
        int _uid;

        internal NewDeclaredExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin)
        {
            _uid = UidProvider.InvalidExtensionBuilderUid;
        }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        internal override int Uid { get { return _uid; } }

        internal override int AssemblyUid { get { return Type.Assembly.Uid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            return ReferenceEquals(other, this);
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            _uid = IndexManager.GetNextExtensionBuilderUid();
            var result = new DeclaredExtensionBuilderRecord
            {
                Id = Id,
                ParentPath = ParentPath,
                ExtensionPointId = ExtensionPointId,
                Uid = _uid,
                AssemblyUid = Type.Assembly.Uid,
                Description = Description,
                TypeName = TypeName
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

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExtensionBuilderResolution;
            if (other == null)
                return false;
            if (other.ExtensionBuilderKind == ExtensionBuilderKind.Referenced)
                return other.Equals(this);
            return ReferenceEquals(other, this);
        }
    }

    class DirectlyAffectedDeclaredExtensionBuilderResolution : DeclaredExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal DirectlyAffectedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return Type.Assembly.Uid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new DeclaredExtensionBuilderRecord
            {
                Id = Id,
                ParentPath = ParentPath,
                ExtensionPointId = ExtensionPointId,
                Uid = _old.Uid,
                AssemblyUid = Type.Assembly.Uid,
                Description = Description,
                TypeName = TypeName
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

        // make sure all dependencies (implementation type (IExtensionBuilder<TExtension> / ICompositeExtensionBuilder<TExtension>) and 
        // extension type (TExtension)) exists, and it obey some rules, and refresh the dependency relationship (uids)
    }

    class IndirectlyAffectedDeclaredExtensionBuilderResolution : ExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal IndirectlyAffectedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        // just make sure all dependencies exists
        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(dialog, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    if (parent.Type == null)
                        return ResolutionStatus.Pending;
                    Parent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(dialog, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    if (parent.Type == null)
                        return ResolutionStatus.Pending;
                    Parent = parent;
                }
            }

            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                    return ResolutionStatus.Failed;
            }

            return ResolveAddin(Parent);
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    }

    class UnaffectedDeclaredExtensionBuilderResolution : ExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal UnaffectedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    } 
    #endregion

    #region Referenced
    abstract class ReferencedExtensionBuilderResolution : ExtensionBuilderResolution
    {
        protected ExtensionBuilderResolution _referenced;

        internal ReferencedExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        // if we can find the referenced extension builder, the resolution is done.
        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                BaseExtensionPointResolution objParent;
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(dialog, ParentPath, out parent))
                    {
                        dialog.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    objParent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(dialog, ParentPath, out parent))
                    {
                        dialog.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    objParent = parent;
                }

                if (objParent.DeclaringAddin != null && !ReferenceEquals(objParent.DeclaringAddin, DeclaringAddin))
                {
                    if (objParent.Type == null)
                        return ResolutionStatus.Pending;
                    // The parent of current extension builder is not defined in the same addin as it is,
                    // so we needs to add its declaring addin as a reference (the type of the parent must 
                    // be loaded before this extension builder at start up).
                    AssemblyResolutionSet assemblySet;
                    if (!ctx.TryGetAssemblySet(objParent.Type.Assembly.AssemblyKey, out assemblySet))
                        throw new Exception();
                    DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                }

                Parent = objParent;
            }

            if (_referenced == null)
            {
                var referenced = TryFindReferencedExtensionBuilder(Parent, Id);
                if (referenced == null)
                {
                    if (!ctx.TryGetExtensionBuilder(dialog, Path, out referenced))
                        return ResolutionStatus.Failed;
                }

                if (referenced.DeclaringAddin != null && !ReferenceEquals(referenced.DeclaringAddin, DeclaringAddin))
                {
                    if (referenced.Type == null)
                        return ResolutionStatus.Pending;
                    // The parent of current extension builder is not defined in the same addin as it is,
                    // so we needs to add its declaring addin as a reference (the type of the parent must 
                    // be loaded before this extension builder at start up).
                    AssemblyResolutionSet assemblySet;
                    if (!ctx.TryGetAssemblySet(referenced.Type.Assembly.AssemblyKey, out assemblySet))
                        throw new Exception();
                    DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                }

                _referenced = referenced;
            }

            return ResolveAddin(Parent) | _referenced.Resolve(dialog, convertionManager, ctx);
        }

        // rule: referenced extension builder must be a child of itself.
        protected static ExtensionBuilderResolution TryFindReferencedExtensionBuilder(BaseExtensionPointResolution parent, string id)
        {
            var real = parent as ExtensionBuilderResolution;
            while (real != null)
            {
                if (real.Id == id)
                    break;
                real = real.Parent as ExtensionBuilderResolution;
            }
            return real;
        }
    }
    
    // an extension builder declared at the top level which referenced by the id at a lower level
    class NewReferencedExtensionBuilderResolution : ReferencedExtensionBuilderResolution
    {
        internal NewReferencedExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }

        internal override int Uid { get { return _referenced.Uid; } }

        internal override int AssemblyUid { get { return _referenced.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            return ReferenceEquals(other, _referenced);
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new ReferencedExtensionBuilderRecord
            {
                Id = Id,
                ParentPath = ParentPath,
                ExtensionPointId = ExtensionPointId,
                Uid = _referenced.Uid,
                AssemblyUid = _referenced.AssemblyUid,
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

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExtensionBuilderResolution;
            if (other == null)
                return false;
            if (other.ExtensionBuilderKind == ExtensionBuilderKind.Declared)
                return ReferenceEquals(other, _referenced);
            return ReferenceEquals(other, this);
        }
    }

    class DirectlyAffectedReferencedExtensionBuilderResolution : ReferencedExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal DirectlyAffectedReferencedExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new ReferencedExtensionBuilderRecord
            {
                Id = Id,
                ParentPath = ParentPath,
                ExtensionPointId = ExtensionPointId,
                Uid = _referenced.Uid,
                AssemblyUid = _referenced.AssemblyUid,
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

    class IndirectlyAffectedReferencedExtensionBuilderResolution : ReferencedExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal IndirectlyAffectedReferencedExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(dialog, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    Parent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(dialog, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    Parent = parent;
                }
            }

            if (_referenced == null)
            {
                var referenced = TryFindReferencedExtensionBuilder(Parent, Id);
                if (referenced == null)
                {
                    if (!ctx.TryGetExtensionBuilder(dialog, Path, out referenced))
                        return ResolutionStatus.Failed;
                }
                _referenced = referenced;
            }

            return ResolveAddin(Parent) | _referenced.Resolve(dialog, convertionManager, ctx);
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    }

    class UnaffectedReferencedExtensionBuilderResolution : ExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal UnaffectedReferencedExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        internal override bool Equals(ExtensionBuilderResolution other)
        {
            throw new NotImplementedException();
        }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    } 
    #endregion
}