//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.AddIns.Core;

namespace JointCode.AddIns
{
    public static class AddinManager
    {
        static AddinEngine _adnEngine;

        public static void Initialize(bool shouldRefresh)
        {
            Initialize(shouldRefresh, new AddinConfiguration());
        }

        public static void Initialize(bool shouldRefresh, IMessageDialog dialog)
        {
            Initialize(shouldRefresh, new AddinConfiguration(dialog));
        }

        public static void Initialize(bool shouldRefresh, IMessageDialog dialog, FileConfiguration fileConfig)
        {
            Initialize(shouldRefresh, new AddinConfiguration(dialog, fileConfig));
        }

        public static void Initialize(bool shouldRefresh, AddinConfiguration addinConfig)
        {
            _adnEngine = new AddinEngine(addinConfig);
            _adnEngine.Initialize(shouldRefresh);
        }

        public static void LoadExtensionPoint(string extensionPointId, object extensionRoot)
        {
            AssertDatabaseInitialized();
            _adnEngine.LoadExtensionPoint(extensionPointId, extensionRoot);
        }

        public static bool TryLoadExtensionPoint(string extensionPointId, object extensionRoot)
        {
            AssertDatabaseInitialized();
            return _adnEngine.TryLoadExtensionPoint(extensionPointId, extensionRoot);
        }

        /// <summary>
        /// Tries to load the extension point identified by the type of <see cref="TExtensionRoot"/>.
        /// If no matching extension point found, or the addin which declared the extension point has been
        /// disabled, nothing will happen.
        /// </summary>
        /// <typeparam name="TExtensionRoot">The type of the extension root.</typeparam>
        /// <param name="extensionRoot">The extension root.</param>
        /// <returns></returns>
        public static bool TryLoadExtensionPoint<TExtensionRoot>(TExtensionRoot extensionRoot)
        {
            AssertDatabaseInitialized();
            return _adnEngine.TryLoadExtensionPoint(extensionRoot);
        }

        static void AssertDatabaseInitialized()
        {
            if (_adnEngine == null)
                throw new Exception("The AddinEngine has not been initialized yet! Please call one of the Initialize method to set it up first!");
        }
    }
}
