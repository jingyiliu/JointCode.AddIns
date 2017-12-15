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
    enum AddinOperationStatus : sbyte
    {
        Unaffected = 0,
        New = 1,
        // The addin has a new version, the updated files have been downloaded to local disk, and it needs 
        // to update its files at the next time the application starts up.
        Updated = 2,
        DirectlyAffected = 3,
        IndirectlyAffected = 4,
    }
}