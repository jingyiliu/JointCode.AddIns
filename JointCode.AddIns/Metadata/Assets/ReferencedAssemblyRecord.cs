//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    //class ReferencedAssemblyRecordSet : List<ReferencedAssemblyRecord>, IPersistentRecord
    //{
    //    internal static MyFunc<ReferencedAssemblyRecordSet> Factory = () => new ReferencedAssemblyRecordSet();

    //    public void Read(Stream reader)
    //    {
    //        var count = reader.ReadInt32();
    //        if (count <= 0)
    //            return;
    //        for (int i = 0; i < count; i++)
    //        {
    //            var item = ReferencedAssemblyRecord.Factory();
    //            Add(item);
    //        }
    //    }

    //    public void Write(Stream writer)
    //    {
    //        if (Count == 0)
    //        {
    //            writer.WriteInt32(0);
    //            return;
    //        }
    //        writer.WriteInt32(Count);
    //        for (int i = 0; i < Count; i++)
    //            this[i].Write(writer);
    //    }
    //}

    class ReferencedAssemblyRecord : ISerializableRecord, IEquatable<ReferencedAssemblyRecord>
    {
        internal static MyFunc<ReferencedAssemblyRecord> Factory = () => new ReferencedAssemblyRecord();
		
        /// <summary>
        /// Gets the unique identifier (uid) of the assembly that this addin references to.
        /// </summary>
        internal int Uid { get; set; }

        /// <summary>
        /// Gets or sets the version of the referenced assembly.
        /// If the referenced assembly has been updated with a higher version, the reference compatibility will 
        /// be determined by the Api.
        /// </summary>
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

        #region IEquatable<DependedAddinRecord> Members

        public bool Equals(ReferencedAssemblyRecord other)
        {
            return Uid == other.Uid;
        }

        #endregion
    }
}
