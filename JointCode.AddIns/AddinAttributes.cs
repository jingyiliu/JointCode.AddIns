////
//// Authors:
////   刘静谊 (Johnny Liu) <jingeelio@163.com>
////
//// Copyright (c) 2017 刘静谊 (Johnny Liu)
////
//// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
////
//using System;
//using System.Collections.Generic;
//using System.Text;
//using JointCode.AddIns.Core;
//using JointCode.AddIns.Extension;

//namespace JointCode.AddIns
//{
//    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
//    public class AddinAttribute : Attribute
//    {
//        public AddinAttribute(string guid)
//        {
//            Guid = guid;
//        }
//        public string Guid { get; private set; }
//        public string Name { get; set; }
//        public string Version { get; set; }
//        public string CompatibleVersion { get; set; }
//        public bool Enabled { get; set; }
//        public AddinCategory Category { get; set; }
//        public string Description { get; set; }
//        public string[] AssemblyFiles { get; set; } // The extension point and extension builder schema will be searched in these assemblies
//        public string[] DataFiles { get; set; }
//    }

//    /// <summary>
//    /// </summary>
//    /// <seealso cref="System.Attribute" />
//    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
//    public class AddinAssemblyAttribute : Attribute
//    {
//        //internal const string PropertyIsPublic = "IsPublic";
//        //public bool IsPublic { get; set; }
//        public Version CompatibleVersion { get; set; }
//    }

//    /// <summary>
//    /// Applies to the <see cref="IExtensionPoint{TExtension, TExtensionRoot}"/> implementation to define an extension point schema.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
//    public class ExtensionPointAttribute : Attribute
//    {
//        public ExtensionPointAttribute(string name)
//        {
//            Name = name;
//        }
//        public string Name { get; private set; }
//        public string Description { get; set; }
//    }

//    /// <summary>
//    /// Applies to the <see cref="IExtensionBuilder{TExtension}"/> or <see cref="ICompositeExtensionBuilder{TExtension}"/> implementation to define an extension builder schema.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//    public class ExtensionBuilderAttribute : Attribute
//    {
//        public ExtensionBuilderAttribute(string name, string parentPath)
//        {
//            Name = name;
//            ParentPath = parentPath;
//        }

//        public string ParentPath { get; private set; }
//        public string Name { get; private set; }
//        public string Description { get; set; }
//    }
//}
