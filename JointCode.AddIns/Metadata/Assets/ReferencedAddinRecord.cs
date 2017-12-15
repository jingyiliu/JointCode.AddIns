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
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class ReferencedAddinRecordSet : List<ReferencedAddinRecord>, ISerializableRecord
	{
        internal static MyFunc<ReferencedAddinRecordSet> Factory = () => new ReferencedAddinRecordSet();

        public void Read(Stream reader)
        {
            var count = reader.ReadInt32();
            if (count <= 0)
                return;
            for (int i = 0; i < count; i++)
            {
                var item = ReferencedAddinRecord.Factory();
                Add(item);
            }
        }

        public void Write(Stream writer)
        {
            if (Count == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(Count);
            for (int i = 0; i < Count; i++)
                this[i].Write(writer);
        }
	}

    class ReferencedAddinRecord : IEquatable<ReferencedAddinRecord>, ISerializableRecord
	{
        internal static MyFunc<ReferencedAddinRecord> Factory = () => new ReferencedAddinRecord();

        /// <summary>
        /// Gets the unique identifier (<see cref="Uid"/>) of the addin that this addin depends on.
        /// </summary>
        internal int Uid { get; set; }
        internal Version Version { get; set; }

        public void Read(Stream reader)
        {
            Uid = reader.ReadInt32();
            Version = reader.ReadVersion();
        }

        public void Write(Stream writer)
        {
            writer.WriteInt32(Uid);
            writer.WriteVersion(Version);
        }
		
		#region IEquatable<ReferencedAddinRecord> Members

        public bool Equals(ReferencedAddinRecord other)
        {
            return Uid == other.Uid;
        }

        #endregion
	}
}
