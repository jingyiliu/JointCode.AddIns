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
    //Make sure the value (0, 1, 2, ...) of each enum is stable, because they have been used to compare to each other. 
    //For example, they have been used in AddinOperand.
    enum AddinCategory : sbyte
    {
        /// <summary>
        /// 1. An application must declare at least one RootAddin.
        /// 2. A RootAddin is an Addin that could not depends on any required Addins.
        /// 3. A RootAddin could not be disabled or uninstalled.
        /// </summary>
        Root = 1,
        /// <summary>
        /// 1. An AppAddin is also an UserAddin, except that it could not be uninstalled and it could not depend on UserAddin.
        /// 2. An AppAddin can be disabled only when all Addins that extend it are disabled.
        /// </summary>
        App = 2,
        /// <summary>
        /// 1. An UserAddin is an Addin provided by user, and it could be disabled and uninstalled.
        /// 2. An UserAddin can be disabled only when all Addins that extend it are disabled.
        /// 3. An UserAddin can be uninstalled only when all Addins that extend it are uninstalled.
        /// </summary>
        User = 3,
    }
}
