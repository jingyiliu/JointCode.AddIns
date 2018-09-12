//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;
using System;
using System.Collections.Generic;
using JointCode.AddIns.Extension;

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

    // ExtensionBuilder 可以分为三种：
    // 1. DeclaredExtensionBuilderResolution：正常定义的 ExtensionBuilder
    // 2. ReferencedExtensionBuilderResolution：指一个 ExtensionBuilder 循环嵌套包含自己
    // 3. 定义在一个插件中，但通过 parentPath 方式扩展定义在另一个插件中的 ExtensionBuilder
    // 例如：
    // <MenuStrip type = "JointCode.AddIns.RootAddin.MenuStripExtensionPoint" friendName="FriendDisplayName" description="This is the extension for MenuStrip">
    //   <ToolStripMenuItem type = "JointCode.AddIns.RootAddin.ToolStripMenuItemExtensionBuilder" >
    //     <ToolStripMenuItem />
    //     <MyToolStripMenuItem type="JointCode.AddIns.RootAddin.MyToolStripMenuItemExtensionBuilder"/>
    //   </ToolStripMenuItem>
    // </MenuStrip>
    // 上面定义了一个 ExtensionPoint (MenuStrip)，其下面有一个 ExtensionBuilder (ToolStripMenuItem)，该 ExtensionBuilder 下面还有一个 ToolStripMenuItem 的 ExtensionBuilder。
    // 第一个 ToolStripMenuItem 为 DeclaredExtensionBuilderResolution，第二个即为 ReferencedExtensionBuilderResolution
    abstract class ExtensionBuilderResolution
        : BaseExtensionPointResolution
        , IEquatable<ExtensionBuilderResolution> // 供 list.Contains 调用
    {
        internal static ExtensionBuilderResolution Empty = new NewOrUpdatedDeclaredExtensionBuilderResolution(null);
        string _path;
    	
        internal ExtensionBuilderResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        internal abstract ExtensionBuilderKind ExtensionBuilderKind { get; }
        internal abstract int Uid { get; }
        internal abstract int AssemblyUid { get; }

        internal string ExtensionPointName { get; set; }

        // because the id of extension builder must be unique with extension point, we can simplely 
        // use the [ExtensionPoint.Id + SysConstants.PathSeparator + ExtensionBuilder.Id] to represent 
        // its path, no matter how deep it is in the extension point.
        internal string Path
        {
            get
            {
                if (_path != null)
                    return _path;
                _path = ExtensionPointName + SysConstants.PathSeparator + Name;
                return _path;
            }
        }
    	
        internal bool ParentIsExtensionPoint { get; set; }
        internal string ParentPath { get; set; }

        #region Dependences
        // the parent of an extension builder can be an extension point or another extension builder
        internal BaseExtensionPointResolution Parent { get; set; }
        #endregion

        internal abstract ExtensionBuilderRecord ToRecord();

        protected static ExtensionPointResolution GetExtensionPointFor(ExtensionBuilderResolution ebResolution)
        {
            var ep = ebResolution.Parent as ExtensionPointResolution;
            if (ep != null)
                return ep;
            var ex = ebResolution.Parent as ExtensionBuilderResolution;
            if (ex == null)
                return null;
            return GetExtensionPointFor(ex);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            var other = obj as ExtensionBuilderResolution;
            if (other == null)
                return false;
            return other.Path == this.Path;
        }

        public bool Equals(ExtensionBuilderResolution other)
        {
            if (ReferenceEquals(other, this))
                return true;
            return other == null
                ? false
                : other.Path == this.Path;  // && other.TypeName == this.TypeName;
        }
    }

    #region Declared：指一个 ExtensionBuilder 定义在另一个 ExtensionBuilder 下，没有循环嵌套
    abstract class DeclaredExtensionBuilderResolution : ExtensionBuilderResolution
    {
        internal DeclaredExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        // The resolution of an extension builder depends on the existence of its implementation type 
        // (IExtensionBuilder<TExtension> / ICompositeExtensionBuilder<TExtension>) and extension type (TExtension),
        // and it obey some rules.
        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                //BaseExtensionPointResolution objParent;
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(resolutionResult, ParentPath, out parent))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    if (!ReferenceEquals(parent.DeclaringAddin, DeclaringAddin))
                    {
                        if (parent.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return parent.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        if (parent.Type == null)
                            return ResolutionStatus.Pending;
                        // The parent of current extension builder is not defined in the same addin as it is,
                        // so we needs to add its declaring addin as a reference (the type of the parent must 
                        // be loaded before this extension builder at start up).
                        AssemblyResolutionSet assemblySet;
                        if (!ctx.TryGetAssemblySet(parent.Type.Assembly.AssemblyKey, out assemblySet))
                            throw new Exception();
                        DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                    }
                    Parent = parent;
                    DeclaringAddin.AddExtendedExtensionPoint(parent);
                    //objParent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, ParentPath, out parent))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    if (!ReferenceEquals(parent.DeclaringAddin, DeclaringAddin))
                    {
                        if (parent.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return parent.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        if (parent.Type == null)
                            return ResolutionStatus.Pending;
                        // The parent of current extension builder is not defined in the same addin as it is,
                        // so we needs to add its declaring addin as a reference (the type of the parent must 
                        // be loaded before this extension builder at start up).
                        AssemblyResolutionSet assemblySet;
                        if (!ctx.TryGetAssemblySet(parent.Type.Assembly.AssemblyKey, out assemblySet))
                            throw new Exception();
                        DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                    }
                    Parent = parent;
                    var ep = GetExtensionPointFor(parent);
                    if (ep == null)
                        return ResolutionStatus.Pending; // the extension point is probably not available right now.
                    DeclaringAddin.AddExtendedExtensionPoint(ep);
                    //objParent = parent;
                }
            }

            if (Type == null)
            {
                Type = ctx.GetUniqueAddinType(DeclaringAddin, TypeName);
                if (Type == null)
                {
                    resolutionResult.AddError(string.Format("Can not find the specified extension builder type [{0}]!", TypeName));
                    return ResolutionStatus.Failed;
                }

                TypeResolution extensionType;
                if (!ApplyRules(resolutionResult, ctx, out extensionType))
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

        // todo: extensionType：将会由 IExtensionBuilder<TExtension> 的实现类所在的程序集去引用 ExtensionType 所在的程序集，最后的结果会添加到 ReferencedAssemblies，此处不必解析
        // apply some rules.
        bool ApplyRules(ResolutionResult resolutionResult, ResolutionContext ctx, out TypeResolution extensionType)
        {
            var result = true;
            if (!Type.IsClass || Type.IsAbstract)
            {
                resolutionResult.AddError(string.Format("The specified extension builder type [{0}] is not a concrete class!", Type.TypeName));
                result = false;
            }

            //// Like extension point, An extension builder can be declared in an addin (extension schema), yet defined in another addin, thus we don't need the following check.
            //if (!this.DeclaresInSameAddin())
            //{
            //    resolutionResult.AddError(string.Format("The extension builder type [{0}] is expected to be defined and declared in a same addin, while its defining addin is [{1}], and its declaring addin is [{2}], which is not the same as the former!", Type.TypeName, Type.Assembly.DeclaringAddin.AddinId.Guid, DeclaringAddin.AddinId.Guid));
            //    result = false;
            //}

            if (Children != null)
            {
                if (!this.InheritFromCompositeExtensionBuilderInterface(resolutionResult, ctx, out extensionType))
                {
                    resolutionResult.AddError(string.Format("The specified extension builder type [{0}] does not implement the required interface (ICompositeExtensionBuilder<TExtension>)!", Type.TypeName));
                    result = false;
                }
            }
            else
            {
                if (!this.InheritFromExtensionBuilderInterface(resolutionResult, ctx, out extensionType))
                {
                    resolutionResult.AddError(string.Format("The specified extension builder type [{0}] does not implement the required interface (IExtensionBuilder<TExtension>)!", Type.TypeName));
                    result = false;
                }
            }

            if (!this.Type.HasPublicParameterLessConstructor())
            {
                resolutionResult.AddError(string.Format("The specified builder point type [{0}] do not have a public parameter-less constructor!", Type.TypeName));
                result = false;
            }

            if (!this.ExtensionTypeMatchesParent(resolutionResult, extensionType))
            {
                result = false;
            }
            return result;
        }
    }

    class NewOrUpdatedDeclaredExtensionBuilderResolution : DeclaredExtensionBuilderResolution
    {
        int _uid;

        internal NewOrUpdatedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin)
        {
            _uid = UidStorage.InvalidExtensionBuilderUid;
        }

        internal override int Uid { get { return _uid; } }

        internal override int AssemblyUid { get { return Type.Assembly.Uid; } }

        //internal override bool Equals(ExtensionBuilderResolution other) { return ReferenceEquals(other, this); }

        internal override ExtensionBuilderRecord ToRecord()
        {
            _uid = UidStorage.GetNextExtensionBuilderUid();
            var result = new DeclaredExtensionBuilderRecord
            {
                Name = Name,
                ParentPath = ParentPath,
                ExtensionPointName = ExtensionPointName,
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

        //public override int GetHashCode()
        //{
        //    return Id.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    var other = obj as ExtensionBuilderResolution;
        //    if (other == null)
        //        return false;
        //    if (other.ExtensionBuilderKind == ExtensionBuilderKind.Referenced)
        //        return other.Equals(this);
        //    return ReferenceEquals(other, this);
        //}
    }

    class DirectlyAffectedDeclaredExtensionBuilderResolution : DeclaredExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal DirectlyAffectedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return Type.Assembly.Uid; } }

        //internal override bool Equals(ExtensionBuilderResolution other) { throw new NotImplementedException(); }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new DeclaredExtensionBuilderRecord
            {
                Name = Name,
                ParentPath = ParentPath,
                ExtensionPointName = ExtensionPointName,
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

        //internal override bool Equals(ExtensionBuilderResolution other) { throw new NotImplementedException(); }

        // just make sure all dependencies exists
        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(resolutionResult, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    if (parent.Type == null)
                        return ResolutionStatus.Pending;
                    Parent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, ParentPath, out parent))
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
            return _old;
        }
    }

    class UnaffectedDeclaredExtensionBuilderResolution : IndirectlyAffectedDeclaredExtensionBuilderResolution
    {
        //readonly ExtensionBuilderRecord _old;

        internal UnaffectedDeclaredExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin, old) { /*_old = old;*/ }

        //protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        //{
        //    return ResolutionStatus.Success;
        //}

        //internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }

        //internal override int Uid { get { return _old.Uid; } }

        //internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        //internal override bool Equals(ExtensionBuilderResolution other)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override ExtensionBuilderRecord ToRecord()
        //{
        //    throw new NotImplementedException();
        //}
    }
    #endregion

    #region Referenced：指一个 ExtensionBuilder 循环嵌套包含自己
    abstract class ReferencedExtensionBuilderResolution : ExtensionBuilderResolution
    {
        protected ExtensionBuilderResolution _referenced;

        internal ReferencedExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }

        // if we can find the referenced extension builder, the resolution is done.
        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                BaseExtensionPointResolution objParent;
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(resolutionResult, ParentPath, out parent))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }

                    Parent = parent;
                    DeclaringAddin.AddExtendedExtensionPoint(parent);
                    objParent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, ParentPath, out parent))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension builder [{0}] with path [{1}]!", TypeName, ParentPath));
                        return ResolutionStatus.Failed;
                    }

                    Parent = parent;
                    var ep = GetExtensionPointFor(parent);
                    if (ep == null)
                        return ResolutionStatus.Pending; // the extension point is probably not available right now.
                    DeclaringAddin.AddExtendedExtensionPoint(ep);
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
            }

            if (_referenced == null)
            {
                var referenced = TryFindReferencedExtensionBuilder(Parent, Name);
                if (referenced == null)
                {
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, Path, out referenced))
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

            return ResolveAddin(Parent) | _referenced.Resolve(resolutionResult, convertionManager, ctx);
        }

        // rule: referenced extension builder must be a child of itself.
        protected static ExtensionBuilderResolution TryFindReferencedExtensionBuilder(BaseExtensionPointResolution parent, string id)
        {
            var real = parent as ExtensionBuilderResolution;
            while (real != null)
            {
                if (real.Name == id)
                    break;
                real = real.Parent as ExtensionBuilderResolution;
            }
            return real;
        }
    }
    
    // an extension builder declared at the top level which referenced by the id at a lower level
    class NewOrUpdatedReferencedExtensionBuilderResolution : ReferencedExtensionBuilderResolution
    {
        internal NewOrUpdatedReferencedExtensionBuilderResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override int Uid { get { return _referenced.Uid; } }

        internal override int AssemblyUid { get { return _referenced.AssemblyUid; } }

        //internal override bool Equals(ExtensionBuilderResolution other) { return ReferenceEquals(other, _referenced); }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new ReferencedExtensionBuilderRecord
            {
                Name = Name,
                ParentPath = ParentPath,
                ExtensionPointName = ExtensionPointName,
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

    class DirectlyAffectedReferencedExtensionBuilderResolution : ReferencedExtensionBuilderResolution
    {
        readonly ExtensionBuilderRecord _old;

        internal DirectlyAffectedReferencedExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin) { _old = old; }

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        //internal override bool Equals(ExtensionBuilderResolution other) { throw new NotImplementedException(); }

        internal override ExtensionBuilderRecord ToRecord()
        {
            var result = new ReferencedExtensionBuilderRecord
            {
                Name = Name,
                ParentPath = ParentPath,
                ExtensionPointName = ExtensionPointName,
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

        internal override int Uid { get { return _old.Uid; } }

        internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        //internal override bool Equals(ExtensionBuilderResolution other) { throw new NotImplementedException(); }

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            // Tries to get the parent
            if (Parent == null)
            {
                if (ParentIsExtensionPoint)
                {
                    ExtensionPointResolution parent;
                    if (!ctx.TryGetExtensionPoint(resolutionResult, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    Parent = parent;
                }
                else
                {
                    ExtensionBuilderResolution parent;
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, ParentPath, out parent))
                        return ResolutionStatus.Failed;
                    Parent = parent;
                }
            }

            if (_referenced == null)
            {
                var referenced = TryFindReferencedExtensionBuilder(Parent, Name);
                if (referenced == null)
                {
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, Path, out referenced))
                        return ResolutionStatus.Failed;
                }
                _referenced = referenced;
            }

            return ResolveAddin(Parent) | _referenced.Resolve(resolutionResult, convertionManager, ctx);
        }

        internal override ExtensionBuilderRecord ToRecord()
        {
            return _old;
        }
    }

    class UnaffectedReferencedExtensionBuilderResolution : IndirectlyAffectedReferencedExtensionBuilderResolution
    {
        //readonly ExtensionBuilderRecord _old;

        internal UnaffectedReferencedExtensionBuilderResolution(AddinResolution declaringAddin, ExtensionBuilderRecord old)
            : base(declaringAddin, old) { /*_old = old;*/ }

        //internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }

        //internal override int Uid { get { return _old.Uid; } }

        //internal override int AssemblyUid { get { return _old.AssemblyUid; } }

        //internal override bool Equals(ExtensionBuilderResolution other)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        //{
        //    return ResolutionStatus.Success;
        //}

        //internal override ExtensionBuilderRecord ToRecord()
        //{
        //    throw new NotImplementedException();
        //}
    } 
    #endregion
}