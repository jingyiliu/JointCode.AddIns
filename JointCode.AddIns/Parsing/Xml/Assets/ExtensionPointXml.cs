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
    class ExtensionPointXml
    {
        protected List<ExtensionBuilderXml> _children;

        internal string Id { get; set; }
        internal string Description { get; set; }
        // The type name of the IExtensionPoint
        internal string TypeName { get; set; }
        
        internal List<ExtensionBuilderXml> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderXml item)
        {
            _children = _children ?? new List<ExtensionBuilderXml>();
            _children.Add(item);
        }

        internal virtual bool Introspect(IMessageDialog dialog)
        {
            var result = true;
            if (TypeName.IsNullOrWhiteSpace())
            {
                dialog.AddError("An extension point must at least provide a type name!");
                result = false;
            }
            return result | DoIntrospect(dialog, "extension point");
        }
        
        protected bool DoIntrospect(IMessageDialog dialog, string name)
		{
        	var result = true;
            if (Id.IsNullOrWhiteSpace())
            {
                dialog.AddError("An " + name + " must at least have an id to be identified!");
                result = false;
            }
        	if (Id.Contains(SysConstants.PathSeparator))
            {
                dialog.AddError("An " + name + " id can not contain [/]!");
                result = false;
            }

            if (_children != null)
            {
                foreach (var child in _children)
                    result &= child.Introspect(dialog);
            }
            return result;
		}
		
		internal bool TryParse(IMessageDialog dialog, AddinResolution addin, out ExtensionPointResolution result)
		{
            result = new NewExtensionPointResolution(addin)
		    {
		        Id = Id,
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
                    eb.ParentPath = Id;
                    eb.ParentIsExtensionPoint = true;
                    result.AddChild(eb);
                }
		    }
		    return true;
		}
    }
}
