//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Data;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.Conversion;
using System;
using System.Collections.Generic;
using JointCode.AddIns.Extension;

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
        protected bool ApplyRules(ResolutionResult resolutionResult, ResolutionContext ctx, ConvertionManager convertionManager)
        {
            return this.ExtensionBuildersMatchParent(resolutionResult) 
                && this.ExtensionDataMatchesExtensionBuilder(resolutionResult, ctx, convertionManager);
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

            var result = Data != null && Data.Items != null
                ? new ExtensionRecord { Head = head, Data = new ExtensionDataRecord(Data.Items) }
                : new ExtensionRecord { Head = head };

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
    class NewOrUpdatedExtensionResolution : ResolvableExtensionResolution
    {
        internal NewOrUpdatedExtensionResolution(AddinResolution declaringAddin)
            : base(declaringAddin) { }

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
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
                if (!ctx.TryGetExtensionPoint(resolutionResult, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(resolutionResult, Head.ParentPath, out exResolution))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension [{0}] with path [{1}]!", Head.Path, Head.ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    {
                        if (exResolution.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return exResolution.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        DeclaringAddin.AddExtendedAddin(exResolution.DeclaringAddin);
                    }
                    //if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    //{
                    var ep = GetExtensionPointFor(exResolution);
                    if (ep == null)
                        return ResolutionStatus.Pending; // the extension point is probably not available right now.
                    DeclaringAddin.AddExtendedExtensionPoint(ep);
                    //}
                    Parent = exResolution;
                }
                else
                {
                    if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                    {
                        if (epResolution.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return epResolution.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        DeclaringAddin.AddExtendedAddin(epResolution.DeclaringAddin);
                    }
                    //if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedExtensionPoint(epResolution);
                    Parent = epResolution;
                }
            }

            if (ExtensionBuilder == null)
            {
                ExtensionBuilderResolution eb;
                if (!ctx.TryGetExtensionBuilder(resolutionResult, Head.ExtensionBuilderPath, out eb))
                {
                    resolutionResult.AddError(string.Format("Can not find the extension builder of the specified extension [{0}] with path [{1}]!", Head.Path, Head.ExtensionBuilderPath));
                    return ResolutionStatus.Failed;
                }
                // The type of extension builder must be loaded before this extension, and it might not defined in the same 
                // addin as current extension (ex), so we needs to add its declaring addin as a reference.
                // !!!Note that the extension point type, as well as extension type (T of IExtensionBuilder<T> implmentation)
                // must be loaded before this extension too, but we'll let the extension builder to reference them.
                if (!ReferenceEquals(eb.DeclaringAddin, DeclaringAddin))
                {
                    //if (eb.Type == null)
                    if (eb.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                        return eb.DeclaringAddin.ResolutionStatus;
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
                if (!ctx.TryGetExtension(resolutionResult, Head.SiblingPath, out sibling))
                {
                    resolutionResult.AddError(string.Format("Can not find the sibling extension of the specified extension [{0}] with path [{1}]!", Head.Path, Head.SiblingPath));
                    return ResolutionStatus.Failed;
                }
                // The metadata of the sibling extension must be loaded before this extension, so we needs to add its declaring 
                // addin as a dependency. 
                if (!ReferenceEquals(sibling.DeclaringAddin, DeclaringAddin))
                {
                    if (sibling.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                        return sibling.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                    DeclaringAddin.AddExtendedAddin(sibling.DeclaringAddin);
                }
                Sibling = sibling;
            }

            if (!ApplyRules(resolutionResult, ctx, convertionManager))
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

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
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
                if (!ctx.TryGetExtensionPoint(resolutionResult, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(resolutionResult, Head.ParentPath, out exResolution))
                    {
                        resolutionResult.AddError(string.Format("Can not find the parent of the specified extension [{0}] with path [{1}]!", Head.Path, Head.ParentPath));
                        return ResolutionStatus.Failed;
                    }
                    if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    {
                        if (exResolution.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return exResolution.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        DeclaringAddin.AddExtendedAddin(exResolution.DeclaringAddin);
                    }
                    //if (!ReferenceEquals(exResolution.DeclaringAddin, DeclaringAddin))
                    //{
                    var ep = GetExtensionPointFor(exResolution);
                    if (ep == null)
                        return ResolutionStatus.Pending; // the extension point is probably not available right now.
                    DeclaringAddin.AddExtendedExtensionPoint(ep);
                    //}
                    Parent = exResolution;
                }
                else
                {
                    if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                    {
                        if (epResolution.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                            return epResolution.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                        DeclaringAddin.AddExtendedAddin(epResolution.DeclaringAddin);
                    }
                    //if (!ReferenceEquals(epResolution.DeclaringAddin, DeclaringAddin))
                    DeclaringAddin.AddExtendedExtensionPoint(epResolution);
                    Parent = epResolution;
                }
            }

            if (ExtensionBuilder == null)
            {
                ExtensionBuilderResolution eb;
                // the extension builder might be declared in:
                // 1. the same addin where this extension is declared or other affected addins (use the uid to get the extension builder directly)
                // 2. an updated addin (use the uid to get the extension builder path, and then use that path to get the extension builder)
                if (!ctx.TryGetExtensionBuilder(resolutionResult, Head.ExtensionBuilderUid, out eb))
                {
                    string extensionBuilderPath;
                    if (!ctx.TryGetExtensionBuilderPath(resolutionResult, Head.ExtensionBuilderUid, out extensionBuilderPath))
                        return ResolutionStatus.Failed;
                    if (!ctx.TryGetExtensionBuilder(resolutionResult, extensionBuilderPath, out eb))
                        return ResolutionStatus.Failed;
                }
                // The type of extension builder must be loaded before this extension, and it might not defined in the same 
                // addin as current extension (ex), so we needs to add its declaring addin as a reference.
                // !!!Note that the extension point type, as well as extension type (T of IExtensionBuilder<T> implmentation)
                // must be loaded before this extension too, but we'll let the extension builder to reference them.
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
                if (!ctx.TryGetExtension(resolutionResult, Head.SiblingPath, out sibling))
                {
                    resolutionResult.AddError(string.Format("Can not find the sibling extension of the specified extension [{0}] with path [{1}]!", Head.Path, Head.SiblingPath));
                    return ResolutionStatus.Failed;
                }
                // The metadata of the sibling extension must be loaded before this extension, so we needs to add its declaring 
                // addin as a dependency. 
                if (!ReferenceEquals(sibling.DeclaringAddin, DeclaringAddin))
                {
                    if (sibling.DeclaringAddin.ResolutionStatus != ResolutionStatus.Success)
                        return sibling.DeclaringAddin.ResolutionStatus; // 上级对象解析 failed，子对象解析就 failed。上级 pending，此处也是 pending。
                    DeclaringAddin.AddExtendedAddin(sibling.DeclaringAddin);
                }
                Sibling = sibling;
            }

            if (!ApplyRules(resolutionResult, ctx, convertionManager))
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

        protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager,
            ResolutionContext ctx)
        {
            if (Parent == null)
            {
                ExtensionPointResolution epResolution;
                if (!ctx.TryGetExtensionPoint(resolutionResult, Head.ParentPath, out epResolution))
                {
                    ExtensionResolution exResolution;
                    if (!ctx.TryGetExtension(resolutionResult, Head.ParentPath, out exResolution))
                    {
                        _resolutionStatus = ResolutionStatus.Failed;
                        return ResolutionStatus.Failed;
                    }
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
                if (!ctx.TryGetExtension(resolutionResult, Head.SiblingPath, out sibling))
                    return ResolutionStatus.Failed;
                Sibling = sibling;
            }

            if (ExtensionBuilder == null)
            {
                // the extension builder can only be declared in:
                // the same addin where this extension is declared or other affected addins (use the uid to get the extension builder directly)
                // otherwise, it would not be an indirectly affected addin.
                ExtensionBuilderResolution eb;
                if (!ctx.TryGetExtensionBuilder(resolutionResult, Head.ExtensionBuilderUid, out eb))
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

    class UnaffectedExtensionResolution : IndirectlyAffectedExtensionResolution
    {
        internal UnaffectedExtensionResolution(AddinResolution declaringAddin, ExtensionRecord old)
            : base(declaringAddin, old)
        {
            //_resolutionStatus = ResolutionStatus.Success;
        }

        //protected override ResolutionStatus DoResolve(ResolutionResult resolutionResult, ConvertionManager convertionManager, ResolutionContext ctx)
        //{
        //    return ResolutionStatus.Success;
        //}

        //internal override ExtensionRecord ToRecord()
        //{
        //    throw new NotImplementedException();
        //}
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
        Dictionary<string, DataHolder> _items;

        internal ExtensionDataResolution() { }
        internal ExtensionDataResolution(Dictionary<string, string> rawItems)
        {
            _rawItems = rawItems;
        }

        //internal Dictionary<string, string> RawItems { get { return _rawItems; } }
        internal Dictionary<string, DataHolder> Items { get { return _items; } }

        internal void AddDataHolder(string key, DataHolder value)
        {
            _items = _items ?? new Dictionary<string, DataHolder>();
            _items[key] = value;
        }

        internal bool TryGetDataHolder(string key, out DataHolder value)
        {
            if (_items != null)
                return _items.TryGetValue(key, out value);
            value = null;
            return false;
        }

        internal bool TryGetString(string key, out string value)
        {
            if (_rawItems != null)
                return _rawItems.TryGetValue(key, out value);
            value = null;
            return false;
        }
    }
}