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

namespace JointCode.AddIns
{
    public delegate void ExtensionEventHandler(object sender, ExtensionEventArgs args);

    public class ExtensionEventArgs : EventArgs
    {
        string _path;
        ExtensionChange _change;

        public ExtensionEventArgs(string path, ExtensionChange change)
        {
            _path = path;
            _change = change;
        }

        /// <summary>
        /// Path of the extension that changed.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Type of change.
        /// </summary>
        public ExtensionChange Change
        {
            get { return _change; }
        }
    }

    /// <summary>
    /// Type of change in an extension change event.
    /// </summary>
    public enum ExtensionChange : sbyte
    {
        /// <summary>
        /// An extension has been loaded.
        /// </summary>
        Load,
        /// <summary>
        /// An extension is going to unload.
        /// </summary>
        Unload
    }
}
