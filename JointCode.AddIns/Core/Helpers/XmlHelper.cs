//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.Xml;

namespace JointCode.AddIns.Core.Helpers
{
    static class XmlHelper
    {
        internal static string GetMatchingAttribueValue(XmlNode node, string attribName)
        {
            return GetMatchingAttribueValue(node, attribName, true);
        }

        internal static string GetMatchingAttribueValue(XmlNode node, string attribName, bool ignoreCase)
        {
            if (node.Attributes == null || node.Attributes.Count == 0)
                return null;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                var attrib = node.Attributes[i];
                if (string.Equals(attrib.Name, attribName, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                    return attrib.Value;
            }
            //var attrib = node.Attributes[attribName];
            return null;
        }

        internal static string GetAttribueValue(XmlAttribute attrib)
        {
            return attrib.InnerText.Trim();
        }

        internal static string GetAttribueName(XmlAttribute attrib)
        {
            return attrib.Name;
        }

        internal static bool AttribueNameEquals(XmlAttribute attrib, string nodeName)
        {
            return AttribueNameEquals(attrib, nodeName, true);
        }

        internal static bool AttribueNameEquals(XmlAttribute attrib, string nodeName, bool ignoreCase)
        {
            return attrib.Name.Equals(nodeName, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        internal static string GetMatchingNodeValue(XmlNode node, string nodeName)
        {
            return node.LocalName == nodeName ? node.InnerText : null;
        }

        internal static string GetNodeValue(XmlNode node)
        {
            return node.InnerText.Trim();
        }

        internal static string GetNodeName(XmlNode node)
        {
            return node.LocalName;
        }
    }
}
