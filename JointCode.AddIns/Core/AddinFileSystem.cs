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
using System.IO;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Core
{
    public class AddinFileSystem
    {
        readonly Dictionary<Guid, Addin> _guid2Addins;
        readonly AddinIndexRecord _addinIndex;

        internal AddinFileSystem(Dictionary<Guid, Addin> guid2Addins, AddinIndexRecord addinIndex)
        {
            _guid2Addins = guid2Addins;
            _addinIndex = addinIndex;
        }

        internal AddinIndexRecord AddinIndexRecord { get { return _addinIndex; } }

        /// <summary>
        /// Gets the full file path of current addin.
        /// </summary>
        /// <param name="filePath">A file path relative to the current addin directory.</param>
        /// <returns></returns>
        public string GetFullFilePath(string filePath)
        {
            return Path.Combine(_addinIndex.AddinDirectory, filePath);
        }

        /// <summary>
        /// Gets the full file path of the specified addin.
        /// </summary>
        /// <param name="addinId">The addin identifier.</param>
        /// <param name="filePath">A file path relative to the specified addin directory.</param>
        /// <returns></returns>
        public string GetFullFilePath(AddinId addinId, string filePath)
        {
            if (ReferenceEquals(addinId, _addinIndex.AddinId))
                return Path.Combine(_addinIndex.AddinDirectory, filePath);
            Addin addin;
            return _guid2Addins.TryGetValue(addinId.Guid, out addin)
                ? Path.Combine(addin.AddinIndexRecord.AddinDirectory, filePath)
                : null;
        }
    }
}
