//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Metadata;
using System.Collections.Generic;
using System.IO;

namespace JointCode.AddIns.Core
{
    /// <summary>
    /// Provides the location information for the addin files.
    /// </summary>
    public class AddinFile
    {
        string[] _files;
        readonly AddinRecord _addinRecord;

        internal AddinFile(AddinRecord addinRecord)
        {
            _addinRecord = addinRecord;
        }

        internal AddinRecord AddinRecord { get { return _addinRecord; } }

        /// <summary>
        /// Gets the absolute path for addin directory.
        /// </summary>
        public string BaseDirectory { get { return _addinRecord.BaseDirectory; } }

        /// <summary>
        /// Gets the relative path of manifest file.
        /// </summary>
        public string ManifestFile { get { return _addinRecord.ManifestFile.FilePath; } }

        /// <summary>
        /// Gets the relative path of all addin files.
        /// </summary>
        public string[] Files
        {
            get
            {
                if (_files != null)
                    return _files;

                var filePack = _addinRecord.AddinFilePack;
                var files = new List<string>();
                files.Add(filePack.ManifestFile.FilePath);
                if (filePack.AssemblyFiles != null)
                {
                    foreach (var assemblyFile in filePack.AssemblyFiles)
                        files.Add(assemblyFile.FilePath);
                }
                if (filePack.DataFiles != null)
                {
                    foreach (var dataFile in filePack.DataFiles)
                        files.Add(dataFile.FilePath);
                }

                _files = files.ToArray();
                return _files;
            }
        }

        /// <summary>
        /// Gets the full file path for the specified file.
        /// </summary>
        /// <param name="filePath">A file path relative to the current addin directory.</param>
        /// <returns></returns>
        public string GetFilePath(string filePath)
        {
            return Path.Combine(_addinRecord.BaseDirectory, filePath);
        }

        /// <summary>
        /// Gets the full file path for the specified assembly file.
        /// </summary>
        /// <param name="assemblyFilePath">A file path relative to the current addin directory.</param>
        /// <returns></returns>
        public string GetRuntimeAssemblyPath(string assemblyFilePath)
        {
            return null;
        }

        /// <summary>
        /// Gets the full file path for the specified assembly file.
        /// </summary>
        /// <param name="assemblyFilePath">A file path relative to the current addin directory.</param>
        /// <returns></returns>
        public string GetAssemblyPath(string assemblyFilePath)
        {
            return null;
        }

        ///// <summary>
        ///// Gets the full file path of the specified addin.
        ///// </summary>
        ///// <param name="addinId">The addin identifier.</param>
        ///// <param name="filePath">A file path relative to the specified addin directory.</param>
        ///// <returns></returns>
        //public string MapPath(AddinId addinId, string filePath)
        //{
        //    if (ReferenceEquals(addinId, _addinRecord.AddinId))
        //        return Path.Combine(_addinRecord.AddinDirectory, filePath);
        //    AddinContext addin;
        //    return _guid2Addins.TryGetValue(addinId.Guid, out addin)
        //        ? Path.Combine(addin.AddinRecord.AddinDirectory, filePath)
        //        : null;
        //}
    }
}
