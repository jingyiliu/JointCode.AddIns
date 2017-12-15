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

namespace JointCode.AddIns.Resolving.Assets
{
    class AddinHeaderResolution
    {
        internal AddinId AddinId { get; set; }
        
        internal string FriendName { get; set; }

        internal string Description { get; set; }

        internal Version Version { get; set; }

        internal Version CompatVersion { get; set; }

        internal string Url { get; set; }

        internal bool Enabled  { get; set; }

        internal AddinCategory AddinCategory { get; set; }

        internal Dictionary<string, string> Properties { get; set; }
    }
}