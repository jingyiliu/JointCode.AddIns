using JointCode.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace JointCode.AddIns.Core.Storage
{
    // 记录插件存储元数据的表
    class AddinMetadataTable
    {
        //long _position; // 该插件元数据表在 stream 中的起始位置
        //int _length; // 该插件元数据表在 stream 
        readonly List<AddinMetadata> _addins = new List<AddinMetadata>();

        //// 获取所有启用的插件
        //internal IEnumerable<AddinMetadata> GetEnabledAddins()
        //{ return null; }

        internal long MinLength { get { return 2 * StreamExtensions.Int64Size; } }
        internal int AddinCount { get { return _addins == null ? 0 : _addins.Count; } }
        internal IEnumerable<AddinMetadata> Addins { get { return _addins; } }

        // 此方法由调用者依据插件之间的依赖关系，按先后顺序调用
        internal void Add(AddinMetadata addinMetadata)
        {
            _addins.Add(addinMetadata);
        }

        internal void Remove(AddinMetadata addinMetadata)
        {
            _addins.Remove(addinMetadata);
        }

        // 从持久化文件中读取插件元数据
        // @返回值：是否存在插件
        internal bool Read(Stream stream)
        {
            // 读取所有元数据
            var count = stream.ReadInt32();
            if (count <= 0)
                return false; // 未包含插件数据

            for (int i = 0; i < count; i++)
            {
                var addin = new AddinMetadata();
                addin.Read(stream);
                _addins.Add(addin);
            }

            // 返回元数据表的长度是否一致
            return true;
        }

        // 将插件元数据的变化写入持久化文件
        internal void Write(Stream stream)
        {
            if (_addins.Count > 0)
            { 
                stream.WriteInt32(_addins.Count);
                for (int i = 0; i < _addins.Count; i++)
                    _addins[i].Write(stream);
            }
            else
            {
                stream.WriteInt32(0);
            }
        }
    }

    // 插件元数据
    class AddinMetadata
    {
        ////internal AddinOperationStatus OperationStatus { get; set; }
        //internal AddinStatus Status { get; set; } // 插件运行状态

        internal bool Enabled { get; set; } // 插件是否启用
        internal int AddinUid { get; set; } // 插件在当前宿主程序中的唯一 Id
        //internal Guid AddinGuid { get; set; } // 插件 Guid（用于唯一地标识插件，也用于在 AddinStorage 中让其他插件引用该插件）

        internal long Position { get; set; } // 插件二进制数据在持久化文件中的起始位置
        internal long Length { get; set; } // 插件二进制数据的长度

        internal void Read(Stream stream)
        {
            //Status = (AddinStatus)stream.ReadSByte();
            Enabled = stream.ReadBoolean();
            AddinUid = stream.ReadInt32();
            Position = stream.ReadInt64();
            Length = stream.ReadInt64();
        }

        internal void Write(Stream stream)
        {
            //stream.WriteSByte((sbyte)Status);
            stream.WriteBoolean(Enabled);
            stream.WriteInt32(AddinUid);
            stream.WriteInt64(Position);
            stream.WriteInt64(Length);
        }

        //// 如果当前插件处于“启用”状态，该函数返回 true；否则返回 false。
        //internal static bool ReadEnabled(Stream stream, out AddinMetadata result)
        //{
        //    return ReadByStatus(stream, AddinStatus.Enabled, out result);
        //}

        //static bool ReadByStatus(Stream stream, AddinStatus status, out AddinMetadata result)
        //{
        //    var rs = (AddinStatus)stream.ReadSByte();
        //    if (rs != status)
        //    {
        //        result = null;
        //        return false;
        //    }

        //    result = new AddinMetadata
        //    {
        //        Status = rs,
        //        AddinUid = stream.ReadInt32(),
        //        Position = stream.ReadInt64(),
        //        Length = stream.ReadInt64()
        //    };

        //    return true;
        //}
    }
}
