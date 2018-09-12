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
        NewOrUpdated = 1,
        //// The addin has a new version, the updated files have been downloaded to local disk, and it needs 
        //// to update its files at the next time the application starts up.
        //Updated = 2,
        DirectlyAffected = 4,
        IndirectlyAffected = 8,

        //// The addin is marked to be deleted at the next time the application starts up, but none or only
        //// part of its files has been deleted (dynamic assembly left).
        //Uninstalled = 16, // 不会有此状态，因为插件卸载 (Uninstalled) 时是直接卸载的
        //// The addin has been successfully built, but now it is invalid because one of its depended addin 
        //// has been un-installed or updated to an incompatible version.
        //Invalid = 32 // 不会有此状态，因为插件解析失败时是不会添加到 AddinStorage 的
    }
}