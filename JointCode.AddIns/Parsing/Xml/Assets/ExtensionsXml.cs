//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Resolving;
using JointCode.AddIns.Resolving.Assets;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class ExtensionsXml
    {
        List<ExtensionXmlGroup> _exGroups;

        internal List<ExtensionXmlGroup> ExtensionGroups { get { return _exGroups; } }

        internal void AddExtensionGroup(ExtensionXmlGroup exGroup)
        {
            _exGroups = _exGroups ?? new List<ExtensionXmlGroup>();
            _exGroups.Add(exGroup);
        }
        
        internal bool Introspect(ResolutionResult resolutionResult)
        {
            if (_exGroups == null)
                return false;
            var result = true;
            foreach (var exGroup in _exGroups)
                result &= exGroup.Introspect(resolutionResult);
            return result;
		}

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out List<ExtensionResolutionGroup> exResolutionGroups)
		{
            var result = true;
            exResolutionGroups = new List<ExtensionResolutionGroup>(_exGroups.Count);
            foreach (var ebGroup in _exGroups)
            {
                ExtensionResolutionGroup exrg;
                result &= ebGroup.TryParse(resolutionResult, addin, out exrg);
                if (!result)
                    break;
                exResolutionGroups.Add(exrg);
            }
            return result;
		}
    }
}
