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
using System.Text.RegularExpressions;
using JointCode.AddIns.Core;
using JointCode.AddIns.Resolving;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class AddinHeaderXml
    {
        static readonly Regex _regex = new Regex(@"^[a-zA-Z0-9_]*$"); //匹配所有字符都是字母、数字或下划线;
        static readonly Version _defaultVersion = new Version(1, 0);
        Dictionary<string, string> _properties;

        internal string Guid { get; set; }
        internal string Enabled { get; set; }
        internal string Name { get; set; }
        internal string Description { get; set; }
        internal string Version { get; set; }
        internal string CompatVersion { get; set; }
        internal string AddinCategory { get; set; }
        //internal string Url { get; set; }

        internal Dictionary<string, string> Properties { get { return _properties; } }

        internal void AddProperty(string key, string value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, string>();
            _properties[key] = value;
        }
        
        internal bool Introspect(string addinLocation, ResolutionResult resolutionResult)
        {
            if (Guid.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError(string.Format("The addin located at [{0}] does not provide a valid guid to be identified!", addinLocation));
                return false;
            }
            if (Name.IsNullOrWhiteSpace())
            {
                resolutionResult.AddError(string.Format("The addin located at [{0}] does not provide an addin name!", addinLocation));
                return false;
            }
            if (!_regex.IsMatch(Name))
            {
                resolutionResult.AddError(string.Format("The addin located at [{0}] does not provide a valid name! An addin name can only contains numbers, letters or underscores!", Name));
                return false;
            }
            return true;
        }

        internal bool TryParse(ResolutionResult resolutionResult, AddinResolution addin, out bool enabled, out AddinHeaderResolution result)
        {
            result = new AddinHeaderResolution { AddinId = new AddinId() };

            try
            {
                enabled = Enabled == null ? true : bool.Parse(Enabled);
                result.AddinId.Guid = new Guid(Guid);
                result.AddinCategory = AddinCategory == null ? Core.AddinCategory.User : (AddinCategory)Enum.Parse(typeof(AddinCategory), AddinCategory);
                result.Version = Version == null ? _defaultVersion : new Version(Version);
                result.CompatVersion = CompatVersion == null ? _defaultVersion : new Version(CompatVersion);
            }
            catch (Exception e)
            {
                enabled = false;
                resolutionResult.AddError(string.Format("An exception is thrown while parsing the addin [{0}]! Error: [{1}]", Name, e.Message));
                return false;
                //throw new AccessViolationException(e.Message);
            }

            result.Name = Name;
            result.Description = Description;
            //result.Url = Url;
            result.InnerProperties = Properties;
            return true;
        }
    }
}
