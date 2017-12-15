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
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    static class RecordHelpers
    {
        internal static List<T> Read<T>(Stream reader, ref MyFunc<T> fac)
            where T : ISerializableRecord
        {
            var count = reader.ReadInt32();
            if (count <= 0)
                return null;
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var item = fac();
                item.Read(reader);
                result.Add(item);
            }
            return result;
        }

        internal static void Write<T>(Stream writer, List<T> items)
            where T : ISerializableRecord
        {
            if (items == null || items.Count == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(items.Count);
            for (int i = 0; i < items.Count; i++)
                items[i].Write(writer);
        }

        internal static List<int> Read(Stream reader)
        {
            var count = reader.ReadInt32();
            if (count <= 0)
                return null;
            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                var item = reader.ReadInt32();
                result.Add(item);
            }
            return result;
        }

        internal static void Write(Stream writer, List<int> items)
        {
            if (items == null || items.Count == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(items.Count);
            for (int i = 0; i < items.Count; i++)
                writer.WriteInt32(items[i]);
        }
    }
}