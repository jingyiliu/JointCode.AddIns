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
    /// Applies to an <see cref="IExtensionBuilder"/> implementation to indicate that a property 
    /// is required for an extension.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExtensionPropertyAttribute : Attribute
    {
        //public ExtensionDataAttribute() : this(true) { }
        //public ExtensionDataAttribute(bool required /*, string name*/)
        //{
        //    //Name = name;
        //    Required = required;
        //}

        //public string Name { get; set; }

        public bool Required { get; set; }
    }
}
