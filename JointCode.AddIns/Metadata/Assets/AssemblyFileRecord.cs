//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class AssemblyFileRecord : AssemblyFileResolution, ISerializableRecord
    {
        internal static MyFunc<AssemblyFileRecord> Factory = () => new AssemblyFileRecord();

        string _fullPath;

        internal string FullPath { get { return _fullPath; } }

        internal void SetDirectory(string addinDirectory)
        {
            if (_fullPath == null)
                _fullPath = Path.Combine(addinDirectory, FilePath);
        }

        public void Read(Stream reader)
        {
            FilePath = reader.ReadString();
            //IsPublic = reader.ReadBoolean();
            Uid = reader.ReadInt32();
            LastWriteTime = reader.ReadDateTime();
        }

        public void Write(Stream writer)
        {
            writer.WriteString(FilePath);
            //writer.WriteBoolean(IsPublic);
            writer.WriteInt32(Uid);
            writer.WriteDateTime(LastWriteTime);
        }
    }

    static class AssemblyFileRecordExtensions
    {
        public static bool Exists(this AssemblyFileRecord item)
        {
            return File.Exists(item.FilePath);
        }

        public static bool UnChanged(this AssemblyFileRecord item)
        {
            return item.LastWriteTime == File.GetLastWriteTime(item.FilePath);
        }
    }
}