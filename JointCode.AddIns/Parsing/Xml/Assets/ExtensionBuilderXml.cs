//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Core;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
	class ExtensionBuilderXmlGroup
	{
        List<ExtensionBuilderXml> _children;

		internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension
        internal List<ExtensionBuilderXml> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderXml item)
        {
            _children = _children ?? new List<ExtensionBuilderXml>();
            _children.Add(item);
        }
        
        internal bool Introspect(IMessageDialog dialog)
		{
            if (ParentPath == null)
            {
                dialog.AddError("");
                return false;
            }
            if (_children == null)
            {
                dialog.AddError("");
                return false;
            }
            return true;
		}

        internal bool TryParse(IMessageDialog dialog, AddinResolution addin, out ExtensionBuilderResolutionGroup result)
		{
            result = new ExtensionBuilderResolutionGroup { ParentPath = ParentPath };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionBuilderResolution eb;
                    if (!child.TryParse(dialog, addin, null, out eb))
                        return false;
                    eb.ParentPath = ParentPath; // The parent path of an extension builder is always ExtensionPoint.Id
                    eb.ParentIsExtensionPoint = false;
                    result.AddChild(eb);
                }
            }
            return true;
		}
	}
	
    // ExtensionBuilder 也是有层次关系的，例如一个 MenuStripItem 可以作为 parent，但一个 MenuStripSeparator 不能
    abstract class ExtensionBuilderXml : ExtensionPointXml
    {
    	string _path;
    	
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

        internal string ExtensionPointId { get; set; }
    	
		internal string ParentPath { get; set; }

        internal abstract bool TryParse(IMessageDialog dialog, AddinResolution addin, BaseExtensionPointResolution parent,
            out ExtensionBuilderResolution result);
    }

    class DeclaredExtensionBuilderXml : ExtensionBuilderXml
    {
        internal override bool Introspect(IMessageDialog dialog)
        {
            var result = true;
            if (ExtensionPointId == null)
            {
                dialog.AddError("An id of extension point must have an parent!");
                result = false;
            }
            if (ParentPath == null)
            {
                dialog.AddError("An extension builder must have an parent!");
                result = false;
            }
            if (TypeName.IsNullOrWhiteSpace())
            {
                dialog.AddError("An extension builder must at least provide a type name!");
                result = false;
            }
            return result | DoIntrospect(dialog, "extension builder");
        }

        internal override bool TryParse(IMessageDialog dialog, AddinResolution addin, BaseExtensionPointResolution parent,
            out ExtensionBuilderResolution result)
        {
            result = new NewDeclaredExtensionBuilderResolution(addin)
            {
                Id = Id,
                ExtensionPointId = ExtensionPointId,
                ParentPath = ParentPath,
                Parent = parent,
                TypeName = TypeName,
                Description = Description
            };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionBuilderResolution eb;
                    if (!child.TryParse(dialog, addin, result, out eb))
                        return false;
                    eb.ParentPath = Path; // The parent path of an extension builder
                    eb.ParentIsExtensionPoint = false;
                    result.AddChild(eb);
                }
            }
            return true;
        }
    }

    class ReferencedExtensionBuilderXml : ExtensionBuilderXml
    {
        internal override bool Introspect(IMessageDialog dialog)
        {
            var result = true;
            if (ExtensionPointId == null)
            {
                dialog.AddError("An id of extension point must have an parent!");
                result = false;
            }
            if (ParentPath == null)
            {
                dialog.AddError("An extension builder must have an parent!");
                result = false;
            }
            return result | DoIntrospect(dialog, "extension builder");
        }

        internal override bool TryParse(IMessageDialog dialog, AddinResolution addin, BaseExtensionPointResolution parent, 
            out ExtensionBuilderResolution result)
        {
            result = new NewReferencedExtensionBuilderResolution(addin)
            {
                Id = Id,
                ExtensionPointId = ExtensionPointId,
                ParentPath = ParentPath,
                Parent = parent
            };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionBuilderResolution eb;
                    if (!child.TryParse(dialog, addin, result, out eb))
                        return false;
                    eb.ParentPath = Path; // The parent path of an extension builder
                    eb.ParentIsExtensionPoint = false;
                    result.AddChild(eb);
                }
            }
            return true;
        }
    }
}
