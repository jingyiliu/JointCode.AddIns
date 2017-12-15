//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns
{
    /// <summary>
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AddinAssemblyAttribute : Attribute
    {
        //internal const string PropertyIsPublic = "IsPublic";
        //public bool IsPublic { get; set; }
        public Version CompatibleVersion { get; set; }
    }
}