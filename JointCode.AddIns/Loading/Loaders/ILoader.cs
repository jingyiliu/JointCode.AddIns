//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns.Loading.Loaders
{
	interface ILoader : IDisposable
    {
        bool Loaded { get; set; }
        void Load(IAddinContext context);
        void Unload(IAddinContext context);
    }

    abstract class Loader : ILoader
    {
        public bool Loaded { get; set; }
        /// <summary>
        /// Loads this instance and all of it children.
        /// </summary>
        public abstract void Load(IAddinContext context);
        /// <summary>
        /// Unloads this instance and all of it children.
        /// </summary>
        public abstract void Unload(IAddinContext context);
        
        public abstract void Dispose();
    }
}
