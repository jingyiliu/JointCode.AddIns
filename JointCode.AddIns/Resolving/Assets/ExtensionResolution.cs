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
using JointCode.AddIns.Core.Serialization;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving.Assets
{
    class ExtensionResolutionGroup
    {
        List<ExtensionResolution> _children;

        internal bool RootIsExtensionPoint { get; set; }
        internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension
        internal List<ExtensionResolution> Children { get { return _children; } }

        internal void AddChild(ExtensionResolution item)
        {
            _children = _children ?? new List<ExtensionResolution>();
            _children.Add(item);
        }

        internal ExtensionRecordGroup ToRecord()
        {
            var exGroup = new ExtensionRecordGroup
            {
                ParentPath = ParentPath,
                RootIsExtensionPoint = RootIsExtensionPoint
            };
            foreach (var child in Children)
                exGroup.AddChild(child.ToRecord());
            return exGroup;
        }
    }

    #region ExtensionResolution
    abstract class ExtensionResolution : Resolvable
    {
        internal static ExtensionResolution Empty = new DirectlyAffectedExtensionResolution(null);
        List<ExtensionResolution> _children;

        internal ExtensionResolution(AddinResolution declaringAddin) : base(declaringAddin) { }

        internal ExtensionHeadResolution Head { get; set; }
        internal ExtensionDataResolution Data { get; set; }

        internal List<ExtensionResolution> Children { get { return _children; } }

        #region Dependences
        internal ExtensionBuilderResolution ExtensionBuilder { get; set; }
        internal Resolvable Parent { get; set; } // can be another ExtensionResolution, or ExtensionPointResolution
        internal ExtensionResolution Sibling { get; set; }
        #endregion
        
        internal void AddChild(ExtensionResolution item)
        {
            _children = _children ?? new List<ExtensionResolution>();
            item.Parent = this;
            _children.Add(item);
        }

        internal abstract ExtensionRecord ToRecord();
    }

    abstract class ResolvableExtensionResolution : ExtensionResolution
    {
        internal ResolvableExtensionResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        // might not get the extension point here!!!!!!!!!!!!!!!
        protected static ExtensionPointResolution GetExtensionPointFor(ExtensionResolution exResolution)
        {
            var ep = exResolution.Parent as ExtensionPointResolution;
            if (ep != null)
                return ep;
            var ex = exResolution.Parent as ExtensionResolution;
            if (ex == null)
                return null;
            return GetExtensionPointFor(ex);
        }

        // apply some rules.
        protected bool ApplyRules(IMessageDialog dialog, ResolutionContext ctx, ConvertionManager convertionManager)
        {
            var result = this.ExtensionBuildersMatchParent(dialog);
            if (!this.ExtensionDataMatchesExtensionBuilder(dialog, ctx, convertionManager))
                result = false;
            return result;
        }

        internal override ExtensionRecord ToRecord()
        {
            var head = new ExtensionHeadRecord
            {
                ExtensionBuilderUid = ExtensionBuilder.Uid,
                Id = Head.Id,
                RelativePosition = Head.RelativePosition,
                SiblingId = Head.SiblingId,
                ParentPath = Head.ParentPath
            };

            var data = new ExtensionDataRecord(Data.Items);
            var result = new ExtensionRecord { Head = head, Data = data };

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

    // new or updated extension
    class NewExtensionResolution : ResolvableExtensionResolution
    {
        internal NewExtensionResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Parent != null && ExtensionBuilder != null)
            {
                // all dependencies that might be declared in other addins has been retrieved.
                if (Head.SiblingId != null && Sibling != null)
                    return ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent) | ResolveAddin(Sibling);
                if (Head.SiblingId == null)
                    return ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent);
            }

            if (Parent == null)
            {
                ExtensionPointResolution epResolution;
                if (!ctx.TryGetExtensionPoint(dialog, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(dialog, Head.ParentPath, out exResolution))
                    {
                        dialog.AddError(string.Format("Can not find the parent of the specified extension [{0}] with path [{1}]!", Head.Path, Head.ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    {
                        var ep = GetExtensionPointFor(exResolution);
                        if (ep == null)
                            return ResolutionStatus.Pending; // the extension point is probably not available right now.
                        DeclaringAddin.AddExtendedExtensionPoint(ep);
                    }
                    Parent = exResolution;
                }
                else
                {
                    if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                        DeclaringAddin.AddExtendedExtensionPoint(epResolution);
                    Parent = epResolution;
                }
                // The metadata of the parent (extension point or another extension) must be loaded before this extension, 
                // so we needs to add its declaring addin as a dependency.
                if (!ReferenceEquals(Parent.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedAddin(Parent.DeclaringAddin);
            }

            if (ExtensionBuilder == null)
            {
                ExtensionBuilderResolution eb;
                if (!ctx.TryGetExtensionBuilder(dialog, Head.ExtensionBuilderPath, out eb))
                {
                    dialog.AddError(string.Format("Can not find the extension builder of the specified extension [{0}] with path [{1}]!", Head.Path, Head.ExtensionBuilderPath));
                    return ResolutionStatus.Failed;
                }
                // The type of extension builder must be loaded before this extension, and it might not defined in the same 
                // addin as current extension (ex), so we needs to add its declaring addin as a reference.
                // !!!Note that the extension point type must loaded before this extension too, but we'll let the extension 
                // builder to refer to it.
                if (!ReferenceEquals(eb.DeclaringAddin, DeclaringAddin))
                {
                    if (eb.Type == null)
                        return ResolutionStatus.Pending;
                    if (!ReferenceEquals(eb.Type.Assembly.DeclaringAddin, DeclaringAddin))
                    {
                        AssemblyResolutionSet assemblySet;
                        if (!ctx.TryGetAssemblySet(eb.Type.Assembly.AssemblyKey, out assemblySet))
                            throw new Exception();
                        DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                    }
                }
                ExtensionBuilder = eb;
            }

            if (Head.SiblingId != null && Sibling == null)
            {
                ExtensionResolution sibling;
                if (!ctx.TryGetExtension(dialog, Head.SiblingPath, out sibling))
                {
                    dialog.AddError(string.Format("Can not find the sibling extension of the specified extension [{0}] with path [{1}]!", Head.Path, Head.SiblingPath));
                    return ResolutionStatus.Failed;
                }
                Sibling = sibling;
                // The metadata of the sibling extension must be loaded before this extension, so we needs to add its declaring 
                // addin as a dependency. 
                if (!ReferenceEquals(sibling.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedAddin(sibling.DeclaringAddin);
            }

            if (!ApplyRules(dialog, ctx, convertionManager))
                return ResolutionStatus.Failed;

            return Sibling != null
                ? ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent) | ResolveAddin(Sibling)
                : ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent);
        }
    }

    // directly affected extension
    class DirectlyAffectedExtensionResolution : ResolvableExtensionResolution
    {
        internal DirectlyAffectedExtensionResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (Parent != null && ExtensionBuilder != null)
            {
                // all dependencies that might be declared in other addins has been retrieved.
                if (Head.SiblingId != null && Sibling != null)
                    return ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent) | ResolveAddin(Sibling);
                if (Head.SiblingId == null)
                    return ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent);
            }

            if (Parent == null)
            {
                ExtensionPointResolution epResolution;
                if (!ctx.TryGetExtensionPoint(dialog, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(dialog, Head.ParentPath, out exResolution))
                        return ResolutionStatus.Failed;
                    if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    {
                        var ep = GetExtensionPointFor(exResolution);
                        if (ep == null)
                            return ResolutionStatus.Pending; // the extension point is probably not available right now.
                        DeclaringAddin.AddExtendedExtensionPoint(ep);
                    }
                    Parent = exResolution;
                }
                else
                {
                    if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                        DeclaringAddin.AddExtendedExtensionPoint(epResolution);
                    Parent = epResolution;
                }
                // The metadata of the parent (extension point or another extension) must be loaded before this extension, 
                // so we needs to add its declaring addin as a dependency.
                if (!ReferenceEquals(Parent.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedAddin(Parent.DeclaringAddin);
            }

            if (ExtensionBuilder == null)
            {
                ExtensionBuilderResolution eb;
                // the extension builder might be declared in:
                // 1. the same addin where this extension is declared or other affected addins (use the uid to get the extension builder directly)
                // 2. an updated addin (use the uid to get the extension builder path, and then use that path to get the extension builder)
                if (!ctx.TryGetExtensionBuilder(dialog, Head.ExtensionBuilderUid, out eb)) 
                {
                    string extensionBuilderPath;
                    if (!ctx.TryGetExtensionBuilderPath(dialog, Head.ExtensionBuilderUid, out extensionBuilderPath))
                        return ResolutionStatus.Failed;
                    if (!ctx.TryGetExtensionBuilder(dialog, extensionBuilderPath, out eb))
                        return ResolutionStatus.Failed;
                }
                // The type of extension builder must be loaded before this extension, and it might not defined in the same 
                // addin as current extension (ex), so we needs to add its declaring addin as a reference.
                // !!!Note that the extension point type must loaded before this extension too, but we'll let the extension 
                // builder to refer to it.
                if (!ReferenceEquals(eb.DeclaringAddin, DeclaringAddin))
                {
                    if (eb.Type == null)
                        return ResolutionStatus.Pending;
                    if (!ReferenceEquals(eb.Type.Assembly.DeclaringAddin, DeclaringAddin))
                    {
                        AssemblyResolutionSet assemblySet;
                        if (!ctx.TryGetAssemblySet(eb.Type.Assembly.AssemblyKey, out assemblySet))
                            throw new Exception();
                        DeclaringAddin.AddReferencedAssemblySet(assemblySet);
                    }
                }
                ExtensionBuilder = eb;
            }

            if (Head.SiblingId != null && Sibling == null)
            {
                ExtensionResolution sibling;
                if (!ctx.TryGetExtension(dialog, Head.SiblingPath, out sibling))
                    return ResolutionStatus.Failed;
                Sibling = sibling;
                // The metadata of the sibling extension must be loaded before this extension, so we needs to add its declaring 
                // addin as a dependency. 
                if (!ReferenceEquals(sibling.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedAddin(sibling.DeclaringAddin);
            }

            if (!ApplyRules(dialog, ctx, convertionManager))
                return ResolutionStatus.Failed;

            return Sibling != null
                ? ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent) | ResolveAddin(Sibling)
                : ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent);
        }
    }

    class IndirectlyAffectedExtensionResolution : ExtensionResolution
    {
        readonly ExtensionRecord _old;

        internal IndirectlyAffectedExtensionResolution(AddinResolution declaringAddin, ExtensionRecord old)
            : base(declaringAddin)
        {
            _old = old;
        }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager,
            ResolutionContext ctx)
        {
            if (Parent == null)
            {
                ExtensionPointResolution epResolution;
                if (!ctx.TryGetExtensionPoint(dialog, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(dialog, Head.ParentPath, out exResolution))
                        return ResolutionStatus.Failed;
                    Parent = exResolution;
                }
                else
                {
                    Parent = epResolution;
                }
            }

            if (Head.SiblingId != null && Sibling == null)
            {
                ExtensionResolution sibling;
                if (!ctx.TryGetExtension(dialog, Head.SiblingPath, out sibling))
                    return ResolutionStatus.Failed;
                Sibling = sibling;
            }

            if (ExtensionBuilder == null)
            {
                // the extension builder can only be declared in:
                // the same addin where this extension is declared or other affected addins (use the uid to get the extension builder directly)
                // otherwise, it would not be an indirectly affected addin.
                ExtensionBuilderResolution eb;
                if (!ctx.TryGetExtensionBuilder(dialog, Head.ExtensionBuilderUid, out eb))
                    return ResolutionStatus.Failed;
                ExtensionBuilder = eb;
            }

            return Sibling != null
                ? ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent) | ResolveAddin(Sibling)
                : ResolveAddin(ExtensionBuilder) | ResolveAddin(Parent);
        }

        internal override ExtensionRecord ToRecord()
        {
            return _old;
        }
    }

    class UnaffectedExtensionResolution : ExtensionResolution
    {
        internal UnaffectedExtensionResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager,
            ResolutionContext ctx)
        {
            throw new NotImplementedException();
        }

        internal override ExtensionRecord ToRecord()
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    class BaseExtensionHeadResolution
    {
        #region Non-Persistent
        string _path;
        // 运行时通过计算得出
        internal string Path
        {
            get
            {
                if (_path != null)
                    return _path;
                if (ParentPath == null)
                    throw new InvalidOperationException("Error getting the extension path: the parent path is empty!");
                _path = ParentPath + SysConstants.PathSeparator + Id;
                return _path;
            }
        }
        // 运行时通过计算得出
        internal string SiblingPath
        {
            get
            {
                if (SiblingId == null)
                    return null;
                if (ParentPath == null)
                    throw new InvalidOperationException("Error getting the sibling path: the parent path is empty!");
                return ParentPath + SysConstants.PathSeparator + SiblingId;
            }
        }

        // 运行时赋值
        internal string ParentPath { get; set; }
        #endregion

        #region Persistent
        internal int ExtensionBuilderUid { get; set; }
        internal string Id { get; set; }
        internal string SiblingId { get; set; }
        #endregion
    }

    class ExtensionHeadResolution : BaseExtensionHeadResolution
    {
        internal RelativePosition RelativePosition { get; set; }
        internal string ExtensionBuilderPath { get; set; }
    }

    class ExtensionDataResolution
    {
        readonly Dictionary<string, string> _rawItems;
        Dictionary<string, SerializableHolder> _items;

        internal ExtensionDataResolution() { }
        internal ExtensionDataResolution(Dictionary<string, string> rawItems)
        {
            _rawItems = rawItems;
        }

        //internal Dictionary<string, string> RawItems { get { return _rawItems; } }
        internal Dictionary<string, SerializableHolder> Items { get { return _items; } }

        internal void AddSerializableHolder(string key, SerializableHolder value)
        {
            _items = _items ?? new Dictionary<string, SerializableHolder>();
            _items[key] = value;
        }

        public bool TryGetString(string key, out string value)
        {
            if (_rawItems != null)
                return _rawItems.TryGetValue(key, out value);
            value = null;
            return false;
        }
    }
}