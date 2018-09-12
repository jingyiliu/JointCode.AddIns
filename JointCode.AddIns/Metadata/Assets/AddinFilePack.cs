//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using System.IO;
using JointCode.Common;

namespace JointCode.AddIns.Metadata.Assets
{
    class AddinFilePack : ISerializableRecord
    {
        internal static MyFunc<AddinFilePack> Factory = () => new AddinFilePack();

        List<DataFileRecord> _dataFiles;
        List<AssemblyFileRecord> _assemblyFiles;

        /// <summary>
        /// The direcoty where the manifest file resides is the base directory of the addin.
        /// </summary>
        internal string BaseDirectory { get { return ManifestFile.Directory; } }

        #region Files
        internal ManifestFileRecord ManifestFile { get; set; }
        internal List<DataFileRecord> DataFiles { get { return _dataFiles; } }
        internal List<AssemblyFileRecord> AssemblyFiles { get { return _assemblyFiles; } }
        #endregion

        internal void AddDataFile(DataFileRecord item)
        {
            _dataFiles = _dataFiles ?? new List<DataFileRecord>();
            _dataFiles.Add(item);
        }

        internal void AddAssemblyFile(AssemblyFileRecord item)
        {
            _assemblyFiles = _assemblyFiles ?? new List<AssemblyFileRecord>();
            _assemblyFiles.Add(item);
        }

        public void Read(Stream reader)
        {
            ManifestFile = new ManifestFileRecord();
            ManifestFile.Read(reader);
            _dataFiles = RecordHelpers.Read(reader, ref DataFileRecord.Factory);
            _assemblyFiles = RecordHelpers.Read(reader, ref AssemblyFileRecord.Factory);
        }

        public void Write(Stream writer)
        {
            ManifestFile.Write(writer);
            RecordHelpers.Write(writer, _dataFiles);
            RecordHelpers.Write(writer, _assemblyFiles);
        }
    }
}