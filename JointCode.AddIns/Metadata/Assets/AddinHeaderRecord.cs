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
using JointCode.AddIns.Core;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    /// <summary>
    /// The addin properties
    /// </summary>
    class AddinHeaderRecord : AddinHeaderResolution
    {
        internal void Read(Stream reader)
        {
            AddinId = new AddinId();
            AddinId.Read(reader);
            Name = reader.ReadString();
            Description = reader.ReadString();
            Version = reader.ReadVersion();
            CompatVersion = reader.ReadVersion();
            //Url = reader.ReadString();
            //Enabled = reader.ReadBoolean();
            //AddinCategory = (AddinCategory)reader.ReadSByte();

            var count = reader.ReadInt32();
            if (count <= 0)
                return;
            var properties = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                properties.Add(key, value);
            }
            InnerProperties = properties;
        }

        internal void Write(Stream writer)
        {
            AddinId.Write(writer);
            writer.WriteString(Name);
            writer.WriteString(Description);
            writer.WriteVersion(Version);
            writer.WriteVersion(CompatVersion);
            //writer.WriteString(Url);
            //writer.WriteBoolean(Enabled);
            //writer.WriteSByte((sbyte)AddinCategory);

            if (PropertyCount == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(PropertyCount);
            foreach (var kv in InnerProperties)
            {
                writer.WriteString(kv.Key);
                writer.WriteString(kv.Value);
            }
        }
    }
}