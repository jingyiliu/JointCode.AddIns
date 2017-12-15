//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class ExtensionDeclarationXml
    {
    	List<ExtensionBuilderXmlGroup> _ebGroups;
    	List<ExtensionPointXml> _eps;
    	
        internal List<ExtensionBuilderXmlGroup> ExtensionBuilderGroups { get { return _ebGroups; } }
        internal List<ExtensionPointXml> ExtensionPoints { get { return _eps; } }

        internal void AddExtensionBuilderGroup(ExtensionBuilderXmlGroup ebGroup)
        {
            _ebGroups = _ebGroups ?? new List<ExtensionBuilderXmlGroup>();
            _ebGroups.Add(ebGroup);
        }

        internal void AddExtensionPoint(ExtensionPointXml ep)
        {
            _eps = _eps ?? new List<ExtensionPointXml>();
            _eps.Add(ep);
        }

		internal bool Introspect(IMessageDialog dialog)
		{
            var result = true;
		    if (_eps != null)
		    {
                foreach (var ep in _eps)
                    result &= ep.Introspect(dialog);
		    }
            if (_ebGroups != null)
            {
                foreach (var ebGroup in _ebGroups)
                    result &= ebGroup.Introspect(dialog);
            }
		    return result;
		}

        internal bool TryParse(IMessageDialog dialog, AddinResolution addin, 
            out List<ExtensionPointResolution> epResolutions, 
            out List<ExtensionBuilderResolutionGroup> ebResolutionGroups)
        {
            epResolutions = null;
            ebResolutionGroups = null;
            var result = true;

            if (_eps != null)
            {
                epResolutions = new List<ExtensionPointResolution>(_eps.Count);
                foreach (var ep in _eps)
                {
                    ExtensionPointResolution epr;
                    result &= ep.TryParse(dialog, addin, out epr);
                    if (!result)
                        break;
                    epResolutions.Add(epr);
                }
            }

            if (_ebGroups != null)
            {
                ebResolutionGroups = new List<ExtensionBuilderResolutionGroup>(_ebGroups.Count);
                foreach (var ebGroup in _ebGroups)
                {
                    ExtensionBuilderResolutionGroup ebrg;
                    result &= ebGroup.TryParse(dialog, addin, out ebrg);
                    if (!result)
                        break;
                    ebResolutionGroups.Add(ebrg);
                }
            }

            return result;
		}
    }
}
