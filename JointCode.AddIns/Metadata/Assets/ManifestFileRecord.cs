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
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    // if the addin manifest changed, it means that the addin has been updated, and it will be rescaned.
    // if it's not changed, even if the assemblies of addin changed, no rescan will happen.
    class ManifestFileRecord : ManifestFileResolution
    {
        internal void Read(Stream reader)
        {
            FilePath = reader.ReadString();
            Directory = reader.ReadString();
            LastWriteTime = reader.ReadDateTime();
            FileLength = reader.ReadInt64();
            FileHash = reader.ReadString();
        }

        internal void Write(Stream writer)
        {
            writer.WriteString(FilePath);
            writer.WriteString(Directory);
            writer.WriteDateTime(LastWriteTime);
            writer.WriteInt64(FileLength);
            writer.WriteString(FileHash);
        }
    }
}