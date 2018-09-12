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
    public enum AddinStatus : sbyte
    {
        Enabled = 1,
        Disabled = 2,
        Starting = 3,
        Started = 4,
        Stopping = 5,
        Stopped = 6
    }
}
