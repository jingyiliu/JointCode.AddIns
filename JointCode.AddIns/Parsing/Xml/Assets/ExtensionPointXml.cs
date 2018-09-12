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
    abstract class BaseExtensionPointXml
    {
        protected List<ExtensionBuilderXml> _children;

        internal string Name { get; set; }
        internal string Description { get; set; }
        // The type name of the IExtensionPoint
        internal string TypeName { get; set; }
        
        internal List<ExtensionBuilderXml> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderXml item)
        {
            _children = _children ?? new List<ExtensionBuilderXml>();
            _children.Add(item);
        }

        protected bool DoIntrospect(INameConvention nameConvention, ResolutionResult resolutionResult, string name)
		{
        	var result = true;
            if (Name.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError("An " + name + " must at least have an name to be identified!");
                result = false;
            }
        	if (Name.Contains(SysConstants.PathSeparatorString))
            {
                resolutionResult.AddError("An " + name + " name can not contain [/]!");
                result = false;
            }

            if (_children != null)
            {
                foreach (var child in _children)
                    result &= child.Introspect(nameConvention, resolutionResult);
            }
            return result;
		}
		
		internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out ExtensionPointResolution result)
		{
            result = new NewOrUpdatedExtensionPointResolution(addin)
		    {
		        Name = Name,
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
                    eb.ParentPath = Name;
                    eb.ParentIsExtensionPoint = true;
                    result.AddChild(eb);
                }
		    }
		    return true;
		}
    }

    class ExtensionPointXml : BaseExtensionPointXml
    {
        internal bool Introspect(INameConvention nameConvention, ResolutionResult resolutionResult)
        {
            var result = true;
            if (TypeName.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError("An extension point must at least provide a type name!");
                result = false;
            }
            return result | DoIntrospect(nameConvention, resolutionResult, "extension point");
        }
    }
}
