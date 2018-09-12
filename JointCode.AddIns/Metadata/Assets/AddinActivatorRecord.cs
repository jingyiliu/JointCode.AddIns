//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.Common.IO;
using System.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class AddinActivatorRecord : ISerializableRecord
    {
        internal AddinActivatorRecord() { }
        internal AddinActivatorRecord(int assemblyUid, string typeName)
        {
            AssemblyUid = assemblyUid;
            TypeName = typeName;
        }

        // The uid of assembly where the IAddinActivator type resides
        internal int AssemblyUid { get; set; }

        // The type name of the IAddinActivator
        internal string TypeName { get; set; }

        public void Read(Stream reader)
        {
            AssemblyUid = reader.ReadInt32();
            TypeName = reader.ReadString();
        }

        public void Write(Stream writer)
        {
            writer.WriteInt32(AssemblyUid);
            writer.WriteString(TypeName);
        }
    }
}