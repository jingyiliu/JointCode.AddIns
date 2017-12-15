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
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Extensions;

namespace JointCode.AddIns.Parsing.Xml.Assets
{
    class AddinHeaderXml
    {
        static readonly Version _defaultVersion = new Version(1, 0);
        Dictionary<string, string> _properties;

        internal string Guid { get; set; }
        internal string FriendName { get; set; }
        internal string Description { get; set; }
        internal string Version { get; set; }
        internal string CompatVersion { get; set; }
        internal string Enabled { get; set; }
        internal string AddinCategory { get; set; }
        internal string Url { get; set; }
        
        internal Dictionary<string, string> Properties
        {
            get { return _properties; }
        }

        internal void AddProperty(string key, string value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, string>();
            _properties[key] = value;
        }
        
        internal bool Introspect(IMessageDialog dialog)
        {
            if (!Guid.IsNullOrWhiteSpace())
                return true;
            dialog.AddError("An addin must at least have a valid guid to be identified!");
            return false;
        }

        internal bool TryParse(IMessageDialog dialog, AddinResolution addin, out AddinHeaderResolution result)
        {
            result = new AddinHeaderResolution { AddinId = new AddinId() };
            try
            {
                result.AddinId.Guid = new Guid(Guid);
                result.AddinCategory = AddinCategory == null ? Core.AddinCategory.User : (AddinCategory)Enum.Parse(typeof(AddinCategory), AddinCategory);
                result.Enabled = Enabled == null ? true : bool.Parse(Enabled);
                result.Version = Version == null ? _defaultVersion : new Version(Version);
                result.CompatVersion = CompatVersion == null ? _defaultVersion : new Version(CompatVersion);
                result.FriendName = FriendName;
                result.Url = Url;
                result.Properties = Properties;
                result.Description = Description;
                return true;
            }
            catch (Exception e) 
            {
                dialog.AddError(e.Message);
                return false;
            }
        }
    }
}
