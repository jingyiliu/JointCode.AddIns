using System.IO;
using JointCode.AddIns.Core.Storage;
using JointCode.Common.IO;

namespace JointCode.AddIns.Core
{
    public class AddinId : ObjectId
    {
        readonly object _ownerTag;

        public AddinId()
        {
            _ownerTag = new object();
            Uid = UidStorage.InvalidAddinUid;
        }

        public override object Tag
        {
            get { return _ownerTag; }
        }

        internal void Read(Stream reader)
        {
            Guid = reader.ReadGuid();
            Uid = reader.ReadInt32();
        }

        internal void Write(Stream writer)
        {
            writer.WriteGuid(Guid);
            writer.WriteInt32(Uid);
        }
    }
}