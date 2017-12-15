//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.AddIns.Core
{
    enum AddinRunningStatus : sbyte
    {
        Default = 0,
        // The addin is in good condition.
        Enabled = 1,
        // The addin is disabled by user or disabled because one or more of its dependencies was disabled.
        Disabled = 2,

        // The addin is marked to be deleted at the next time the application starts up, but none or only
        // part of its files has been deleted (dynamic assembly left).
        Uninstalled = 3,

        Updated = 4,
        
        // The addin has been successfully built, but now it is invalid because one of its depended addin 
        // has been un-installed or updated to an incompatible version.
        Invalid = 5
    }
}
