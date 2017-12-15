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
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.IO;

namespace JointCode.AddIns.Core
{
    public class TypeId : ISerializableRecord
    {
        const char Separator = ',';

        int _assemblyUid;
        int _metadataToken;

        internal TypeId() { }
        internal TypeId(int assemblyUid, int metadataToken)
        {
            _assemblyUid = assemblyUid;
            _metadataToken = metadataToken;
        }

        internal static TypeId Parse(string input)
        {
            var parts = input.Split(Separator);
            if (parts.Length != 2)
                throw new ArgumentException();
            var assemblyUid = int.Parse(parts[0]);
            var metadataToken = int.Parse(parts[1]);
            return new TypeId(assemblyUid, metadataToken);
        }

        internal static bool TryParse(string input, out TypeId result)
        {
            var parts = input.Split(Separator);
            if (parts.Length != 2)
            {
                result = null;
                return false;
            }
            int assemblyUid, metadataToken;
            if (!int.TryParse(parts[0], out assemblyUid) || !int.TryParse(parts[1], out metadataToken))
            {
                result = null;
                return false;
            }
            result = new TypeId(assemblyUid, metadataToken);
            return true;
        }

        internal int AssemblyUid { get { return _assemblyUid; } }
        internal int MetadataToken { get { return _metadataToken; } }

        public void Read(Stream reader)
        {
            _assemblyUid = reader.ReadInt32();
            _metadataToken = reader.ReadInt32();
        }

        public void Write(Stream writer)
        {
            writer.WriteInt32(_assemblyUid);
            writer.WriteInt32(_metadataToken);
        }

        public override string ToString()
        {
            return _assemblyUid.ToString() + Separator + _metadataToken.ToString();
        }
    }
}