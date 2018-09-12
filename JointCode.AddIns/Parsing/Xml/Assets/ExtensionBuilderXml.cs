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
using JointCode.AddIns.Resolving;
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
        
        internal bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult)
		{
            if (ParentPath == null)
            {
                resolutionResult.AddError("");
                return false;
            }
            if (_children == null)
            {
                resolutionResult.AddError("");
                return false;
            }
		    foreach (var child in _children)
		    {
		        if (!child.Introspect(nameConvention, resolutionResult))
		            return false;
		    }
            return true;
        }

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out ExtensionBuilderResolutionGroup result)
		{
            result = new ExtensionBuilderResolutionGroup { ParentPath = ParentPath };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionBuilderResolution eb;
                    if (!child.TryParse(resolutionResult, addin, null, out eb))
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
    abstract class ExtensionBuilderXml : BaseExtensionPointXml
    {
    	string _path;
    	
    	// because the id of extension builder must be unique within an extension point, we can simplely 
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

        internal string ExtensionPointName { get; set; }
    	
		internal string ParentPath { get; set; }

        internal abstract bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult);

        internal abstract bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, BaseExtensionPointResolution parent,
            out ExtensionBuilderResolution result);
    }

    class DeclaredExtensionBuilderXml : ExtensionBuilderXml
    {
        internal override bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult)
        {
            var result = true;
            if (ExtensionPointName == null)
            {
                resolutionResult.AddError("An id of extension point must have an parent!");
                result = false;
            }
            if (ParentPath == null)
            {
                resolutionResult.AddError("An extension builder must have an parent!");
                result = false;
            }
            if (TypeName.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError("An extension builder must at least provide a type name!");
                result = false;
            }
            if (Name != nameConvention.GetExtensionBuilderName(TypeName))
            {
                resolutionResult.AddError("The extension builder name and its type name does not comply with the name convention!");
                result = false;
            }
            return result | DoIntrospect(nameConvention, resolutionResult, "extension builder");
        }

        internal override bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, BaseExtensionPointResolution parent,
            out ExtensionBuilderResolution result)
        {
            result = new NewOrUpdatedDeclaredExtensionBuilderResolution(addin)
            {
                Name = Name,
                ExtensionPointName = ExtensionPointName,
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
                    if (!child.TryParse(resolutionResult, addin, result, out eb))
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
        internal override bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult)
        {
            var result = true;
            if (ExtensionPointName == null)
            {
                resolutionResult.AddError("An id of extension point must have an parent!");
                result = false;
            }
            if (ParentPath == null)
            {
                resolutionResult.AddError("An extension builder must have an parent!");
                result = false;
            }
            return result | DoIntrospect(nameConvention, resolutionResult, "extension builder");
        }

        internal override bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, BaseExtensionPointResolution parent, 
            out ExtensionBuilderResolution result)
        {
            result = new NewOrUpdatedReferencedExtensionBuilderResolution(addin)
            {
                Name = Name,
                ExtensionPointName = ExtensionPointName,
                ParentPath = ParentPath,
                Parent = parent
            };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionBuilderResolution eb;
                    if (!child.TryParse(resolutionResult, addin, result, out eb))
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
