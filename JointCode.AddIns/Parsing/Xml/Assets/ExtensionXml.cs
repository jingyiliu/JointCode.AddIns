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
using JointCode.AddIns.Extension;
using JointCode.AddIns.Resolving;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
	class ExtensionXmlGroup
	{
        List<ExtensionXml> _children;

        internal bool RootIsExtensionPoint { get; set; }
		internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension
        internal List<ExtensionXml> Children { get { return _children; } }

        internal void AddChild(ExtensionXml item)
        {
            _children = _children ?? new List<ExtensionXml>();
            _children.Add(item);
        }

        internal bool Introspect(ResolutionResult resolutionResult)
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
            var result = true;
            foreach (var child in _children)
                result &= child.Introspect(resolutionResult);
            return result;
		}

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out ExtensionResolutionGroup result)
		{
            result = new ExtensionResolutionGroup { ParentPath = ParentPath, RootIsExtensionPoint = RootIsExtensionPoint };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionResolution ex;
                    if (!child.TryParse(resolutionResult, addin, null, out ex))
                        return false;
                    result.AddChild(ex);
                }
            }
            return true;
		}
	}
	
    class ExtensionXml
    {
        List<ExtensionXml> _children;

    	internal ExtensionHeadXml Head { get; set; }
    	internal ExtensionDataXml Data { get; set; }

        internal List<ExtensionXml> Children { get { return _children; } }

        internal void AddChild(ExtensionXml item)
        {
            _children = _children ?? new List<ExtensionXml>();
            _children.Add(item);
        }
    	
    	internal bool Introspect(ResolutionResult resolutionResult)
    	{
            var result = Head.Introspect(resolutionResult);
            if (_children != null)
            {
                foreach (var child in _children)
                    result &= child.Introspect(resolutionResult);
            }
            return result;
		}

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, Resolvable parent, out ExtensionResolution result)
        {
            result = null;
            ExtensionHeadResolution head;
            if (!Head.TryParse(resolutionResult, addin, out head))
                return false;
            result = new NewOrUpdatedExtensionResolution(addin) { Data = (Data != null && Data.Items != null ? new ExtensionDataResolution(Data.Items) : null), Head = head, Parent = parent };
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    ExtensionResolution ex;
                    if (!child.TryParse(resolutionResult, addin, result, out ex))
                        return false;
                    result.AddChild(ex);
                }
            }
            return true;
        }
    }

    class ExtensionHeadXml : BaseExtensionHeadResolution
    {
        internal RelativePosition RelativePosition { get; set; }

        internal string ExtensionBuilderPath { get; set; }

        internal bool Introspect(ResolutionResult resolutionResult)
        {
            var result = true;
            if (!Id.IsNullOrWhiteSpace() && Id.Contains(SysConstants.PathSeparatorString))
            {
                resolutionResult.AddError("The Id property of an extension can not contains slash (/)!");
                result = false;
            }
            if (!SiblingId.IsNullOrWhiteSpace() && SiblingId.Contains(SysConstants.PathSeparatorString))
            {
                resolutionResult.AddError("The SiblingId property of an extension can not contains slash (/)!");
                result = false;
            }
            if (ExtensionBuilderPath.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError("The extension builder path of an extension can not be null or empty!");
                result = false;
            }
            return result;
        }

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out ExtensionHeadResolution result)
        {
            result = new ExtensionHeadResolution
            {
                Id = Id,
                ExtensionBuilderPath = ExtensionBuilderPath,
                RelativePosition = RelativePosition,
                SiblingId = SiblingId, 
                ParentPath = ParentPath
            };
            return true;
        }
    }

    class ExtensionDataXml
    {
        Dictionary<string, string> _items;

        internal Dictionary<string, string> Items { get { return _items; } }

        internal void Add(string key, string value)
        {
            _items = _items ?? new Dictionary<string, string>();
            _items[key] = value;
        }
    }
}