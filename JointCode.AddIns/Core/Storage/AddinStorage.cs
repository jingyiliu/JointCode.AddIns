using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Metadata;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Core.Storage
{
    // 整个持久化文件的存储结构依次如下：
    // 1. AddinRecord Segment：AddinRecord + AddinRecord + ...
    // 2. AddinMetadata Segment (AddinMetadataTable)：AddinMetadata + AddinMetadata + ...
    // 3. InvalidAddinFilePack Segment：AddinFilePack + AddinFilePack + ...
    // 4. AddinRecord 的长度（int64 类型）[起始位置为 0]
    // 5. AddinMetadata 的长度（int64 类型）[起始位置为 AddinRecord 的长度 + 1]
    // 6. InvalidAddinFilePacks 的长度（int64 类型）[起始位置为 AddinRecord 的长度 + AddinMetadata 的长度 + 1]
    // 7. 整个数据区域 (data segment) 的长度
    partial class AddinStorage
    {
        bool _changed;
        readonly string _storageFilePath;
        AddinMetadataTable _metadataTable; // 插件元数据表

        internal AddinStorage(string storageFilePath)
        {
            _changed = false;
            _storageFilePath = storageFilePath;
            _metadataTable = new AddinMetadataTable();
            _addinRecords = new List<AddinRecord>();
        }

        long SegmentLength { get { return 4 * StreamExtensions.Int64Size; } }
        internal bool Changed { get { return _changed; } }

        internal void Reset()
        {
            _changed = false;
            _metadataTable = new AddinMetadataTable();
            _addinRecords = new List<AddinRecord>();
            _invalidAddinFilePacks = null;
        }

        /// <summary>
        /// Reads the addin metadata from the addin storage file
        /// </summary>
        /// <returns></returns>
        internal bool Read()
        {
            if (!File.Exists(_storageFilePath))
                return false;

            try
            {
                // open the addin storage file with read/write share mode.
                var stream = IoHelper.OpenReadWrite(_storageFilePath);
                var result = DoRead(stream);
                if (stream != null)
                    stream.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the addin metadata from the addin storage file
        /// </summary>
        /// <returns></returns>
        internal bool ReadOrReset()
        {
            if (!File.Exists(_storageFilePath))
                return false;

            FileStream stream = null; 
            try
            {
                // open the addin storage file with readonly mode.
                stream = IoHelper.OpenReadWrite(_storageFilePath);
                var result = DoRead(stream);
                if (stream != null)
                    stream.Dispose();
                if (!result)
                    IoHelper.ClearContent(_storageFilePath);
                return result;
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Dispose();
                    IoHelper.ClearContent(_storageFilePath);
                }
                return false;
            }
        }

        bool DoRead(Stream stream)
        {
            if (stream.Length < _metadataTable.MinLength + SegmentLength)
                return false; // 虽然插件数据文件存在，但并未包含有效的数据

            // 1. 读取各个分段的长度
            stream.Position = stream.Length - SegmentLength;
            var addinRecordSegmentLength = stream.ReadInt64();
            var addinMetadataSegmentLength = stream.ReadInt64();
            var invalidAddinFilePacksSegmentLength = stream.ReadInt64();
            var dataSegmentLength = stream.ReadInt64();
            if (dataSegmentLength + SegmentLength != stream.Length)
                return false; // 插件数据文件已被破坏，返回 false 以便让系统重新解析插件

            // 2. 读取 AddinMetadata 分段 (AddinMetadataTable)
            stream.Position = addinRecordSegmentLength; // 指针指向 AddinMetadata 分段 (AddinMetadataTable) 的起始位置
            var hasAddins = _metadataTable.Read(stream);
            if (!hasAddins || _metadataTable.AddinCount == 0)
                return false; // 虽然插件数据文件存在，但并未包含有效的数据

            // 3. 根据 AddinMetadata 读取 AddinRecord 分段，并进行验证
            foreach (var metadata in _metadataTable.Addins)
            {
                stream.Position = metadata.Position;
                var addin = new AddinRecord(metadata);
                addin.Read(stream);
                if (stream.Position - metadata.Position != metadata.Length || addin.Uid != metadata.AddinUid)
                    return false; // 插件数据文件已被破坏，返回 false 以便让系统重新解析插件

                _addinRecords.Add(addin);
            }

            // 4. 读取 InvalidAddinFilePack 分段
            stream.Position = addinRecordSegmentLength + addinMetadataSegmentLength; // 指针指向 InvalidAddinFilePack 分段的起始位置
            _invalidAddinFilePacks = RecordHelpers.Read(stream, ref AddinFilePack.Factory);
            if (stream.Position - (addinRecordSegmentLength + addinMetadataSegmentLength) != invalidAddinFilePacksSegmentLength)
                return false; // 插件数据文件已被破坏，返回 false 以便让系统重新解析插件

            // 5. 读取 Uid 分段
            stream.Position = addinRecordSegmentLength + addinMetadataSegmentLength + invalidAddinFilePacksSegmentLength; // 指针指向 Uid 分段的起始位置
            UidStorage.Read(stream);

            return true;
        }

        /// <summary>
        /// Writes the addin metadata to the addin storage file
        /// </summary>
        internal bool Write()
        {
            if (!_changed)
                return false;

            if (File.Exists(_storageFilePath))
            {
                // 如果元数据文件存在，在写元数据文件之前，先给当前元数据文件创建一个备份，这样在写入失败时，还可以恢复到之前的版本
                var bakFile = _storageFilePath + "." + DateTime.Now.ToString("yyyyMMdd") + ".bak";
                if (File.Exists(bakFile))
                    File.Delete(bakFile);
                File.Copy(_storageFilePath, bakFile);
            }

            FileStream stream = null;
            try
            {
                stream = IoHelper.OpenWrite(_storageFilePath);
                var result = DoWrite(stream);
                _changed = false;
                if (stream != null)
                    stream.Dispose();
                return result;
            }
            catch (Exception ex1)
            {
                if (stream != null)
                    stream.Dispose();

                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(500);
                    try
                    {
                        stream = IoHelper.OpenWrite(_storageFilePath);
                        var result = DoWrite(stream);
                        _changed = false;
                        if (stream != null)
                            stream.Dispose();
                        return result;
                    }
                    catch (Exception ex2)
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                }

                return false;
            }
        }

        bool DoWrite(Stream stream)
        {
            long addinRecordSegmentLength = 0,
                addinMetadataSegmentLength = 0,
                invalidAddinFilePacksSegmentLength = 0,
                dataSegmentLength = 0;

            // 1. 写入 AddinRecord 分段
            long startPosition = 0;
            foreach (var addin in _addinRecords)
            {
                var addinMetadata = addin.AddinMetadata;
                addinMetadata.AddinUid = addin.Uid;
                // 记录 AddinRecord 的起始位置
                addinMetadata.Position = stream.Position;
                // 写入 AddinRecord
                addin.Write(stream);
                // 记录 AddinRecord 的长度
                addinMetadata.Length = stream.Position - addinMetadata.Position;
            }
            addinRecordSegmentLength = stream.Position - startPosition;

            // 2. 写入 AddinMetadata 分段 (AddinMetadataTable)
            startPosition = stream.Position;
            _metadataTable.Write(stream);
            addinMetadataSegmentLength = stream.Position - startPosition;

            // 3. 写入 InvalidAddinFilePack 分段
            startPosition = stream.Position;
            RecordHelpers.Write(stream, _invalidAddinFilePacks);
            invalidAddinFilePacksSegmentLength = stream.Position - startPosition;

            // 4. 写入 Uid 分段
            UidStorage.Write(stream);

            // 5. 获取数据段的长度
            dataSegmentLength = stream.Position;

            // 6. 写入各个分段 (segemtnt) 的长度
            stream.WriteInt64(addinRecordSegmentLength);
            stream.WriteInt64(addinMetadataSegmentLength);
            stream.WriteInt64(invalidAddinFilePacksSegmentLength);
            stream.WriteInt64(dataSegmentLength);

            return true;
        }
    }

    // AddinStorage 类似于 AddinRecord 的数据访问层 (DAL)，该类实现 AddinRecord 的 CRD 功能
    partial class AddinStorage
    {
        List<AddinRecord> _addinRecords;

        internal int AddinRecordCount { get { return _addinRecords == null ? 0 : _addinRecords.Count; } }
        internal IEnumerable<AddinRecord> AddinRecords { get { return _addinRecords; } }

        // 获取所有启用的插件
        internal List<AddinRecord> GetEnabledAddins()
        {
            if (_addinRecords == null)
                throw new InvalidOperationException("");
            var result = new List<AddinRecord>();
            for (int i = 0; i < _addinRecords.Count; i++)
            {
                var addinRecord = _addinRecords[i];
                if (addinRecord.Enabled)
                    result.Add(addinRecord);
            }
            return result;
        }

        internal AddinRecord Get(int index)
        {
            return _addinRecords[index];
        }

        // 此方法由调用者依据插件之间的依赖关系，按先后顺序调用
        internal void Add(AddinRecord addinRecord)
        {
            //if (_metadataTable.AddinCount != AddinRecordCount)
            //    throw GetInconsistentStateException();
            _metadataTable.Add(addinRecord.AddinMetadata);
            AddinRecord.InsetAddinByUid(_addinRecords, addinRecord);
            _changed = true;
        }

        internal void Remove(AddinRecord addinRecord)
        {
            //if (_metadataTable.AddinCount != AddinRecordCount)
            //    throw GetInconsistentStateException();
            _metadataTable.Remove(addinRecord.AddinMetadata);
            _addinRecords.Remove(addinRecord);
            _changed = true;
        }

        //Exception GetInconsistentStateException()
        //{
        //    return new InconsistentStateException("The addin storage file is in inconsistent state!");
        //}

        internal void TryRemove(Guid addinGuid)
        {
            AddinRecord adnRecord = null;
            foreach (var addinRecord in _addinRecords)
            {
                if (addinRecord.Guid != addinGuid)
                    continue;
                adnRecord = addinRecord;
                break;
            }
            if (adnRecord != null)
                Remove(adnRecord);
        }
    }

    partial class AddinStorage
    {
        List<AddinFilePack> _invalidAddinFilePacks;
        //// assemblies managed individually, including assemblis provided by application directory or other locations.
        //List<AssemblyFileRecord> _standaloneAssemblies;  

        internal int InvalidAddinFilePackCount { get { return _invalidAddinFilePacks == null ? 0 : _invalidAddinFilePacks.Count; } }
        internal IEnumerable<AddinFilePack> InvalidAddinFilePacks { get { return _invalidAddinFilePacks; } }

        #region InvalidAddinFilePack CUD

        internal void AddInvalidAddinFilePack(AddinFilePack addinFilePack)
        {
            if (_invalidAddinFilePacks == null)
            {
                _changed = true;
                _invalidAddinFilePacks = _invalidAddinFilePacks ?? new List<AddinFilePack>();
                _invalidAddinFilePacks.Add(addinFilePack);
            }
            else
            {
                var addinDiretory = addinFilePack.BaseDirectory;
                for (int i = 0; i < _invalidAddinFilePacks.Count; i++)
                {
                    var invalidAddinFilePack = _invalidAddinFilePacks[i];
                    if (addinDiretory.Equals(invalidAddinFilePack.BaseDirectory, StringComparison.InvariantCultureIgnoreCase))
                        return;
                }
                _changed = true;
                _invalidAddinFilePacks.Add(addinFilePack);
            }
        }

        internal bool RemoveInvalidAddinFilePack(string addinDirectory)
        {
            if (InvalidAddinFilePackCount == 0)
                return false;
            for (int i = 0; i < _invalidAddinFilePacks.Count; i++)
            {
                var invalidAddinFilePack = _invalidAddinFilePacks[i];
                if (!addinDirectory.Equals(invalidAddinFilePack.BaseDirectory, StringComparison.InvariantCulture)) // 在 *nix 系统中，文件路径是区分大小写的
                    continue;
                _changed = true;
                _invalidAddinFilePacks.RemoveAt(i);
                return true;
            }
            return false;
        }

        #endregion
    }
}