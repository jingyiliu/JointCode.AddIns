//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns.Resolving.Assets
{
    class ManifestFileResolution : DataFileResolution
    { 
//		string _fullPath;

        #region Cacheable Item
        /// <summary>
        /// An absolute path to the directory where the addin locates
        /// </summary>
        internal string Directory { get; set; } 

        internal DateTime LastWriteTime { get; set; }
        // 文件的 Hash 码，以确定文件内容未改变
        internal int FileHash { get; set; } 
        #endregion

//        #region Non-cacheable Item
//        internal string FullPath
//        {
//            get
//            {
//                _fullPath = _fullPath ?? Path.Combine(Directory, FilePath);
//                return _fullPath;
//            }
//            set
//            {
//                if (_fullPath == null && !string.IsNullOrEmpty(value))
//                {
//                    _fullPath = value;
//                    FilePath = Path.GetFileName(value);
//                    Directory = Path.GetDirectoryName(value);
//                }
//            }
//        }
//        #endregion
    }
}