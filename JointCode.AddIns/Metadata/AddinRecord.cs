//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;
using JointCode.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace JointCode.AddIns.Metadata
{
    // AddinRecord 的存储结构依次如下：
    // 1. AddinFilePack
    // 2. 可持久化属性
    partial class AddinRecord : ISerializableRecord
    {
        //internal static MyFunc<AddinRecord> Factory = () => new AddinRecord();
        readonly AddinMetadata _addinMetadata;
        readonly AddinFilePack _addinFilePack;
        readonly AddinHeaderRecord _addinHeader;
        AddinActivatorRecord _addinActivator;

        internal AddinRecord(AddinMetadata metadata)
        {
            _addinMetadata = metadata;
            _addinHeader = new AddinHeaderRecord();
            _addinFilePack = new AddinFilePack();
            //OperationStatus = AddinOperationStatus.Unaffected;
        }

        internal AddinRecord(AddinHeaderRecord addinHeader, AddinFilePack addinFilePack, AddinActivatorRecord addinActivator)
        {
            _addinFilePack = addinFilePack;
            _addinHeader = addinHeader;
            _addinActivator = addinActivator;
            //OperationStatus = AddinOperationStatus.Unaffected;
            _addinMetadata = new AddinMetadata();
        }

        //// 该属性仅用于在解析现有插件时，判断该插件是受到更新直接影响、间接影响或无影响的插件，可以设法去除（见 AddinResolver.RegisterExistingAssets 方法）
        //internal AddinOperationStatus OperationStatus { get; set; } 

        #region 非持久化属性
        internal bool AssembliesRegistered { get; set; }
        internal AddinMetadata AddinMetadata { get { return _addinMetadata; } }
        //internal AddinStatus Status
        //{
        //    get { return _addinMetadata.Status; }
        //    set { _addinMetadata.Status = value; }
        //}
        internal bool Enabled
        {
            get { return _addinMetadata.Enabled; }
            set { _addinMetadata.Enabled = value; }
        }
        #endregion

        // 以下为可持久化属性
        internal AddinHeaderRecord AddinHeader { get { return _addinHeader; } }
        internal AddinId AddinId { get { return _addinHeader.AddinId; } }
        internal Guid Guid { get { return AddinId.Guid; } }
        internal int Uid { get { return AddinId.Uid; } }

        #region Files
        internal AddinFilePack AddinFilePack { get { return _addinFilePack; } }
        internal string BaseDirectory { get { return _addinFilePack.BaseDirectory; } }
        internal ManifestFileRecord ManifestFile
        {
            get { return _addinFilePack.ManifestFile; }
            set { _addinFilePack.ManifestFile = value; }
        }
        internal List<DataFileRecord> DataFiles { get { return _addinFilePack.DataFiles; } }
        internal List<AssemblyFileRecord> AssemblyFiles { get { return _addinFilePack.AssemblyFiles; } }
        #endregion

        internal AddinActivatorRecord AddinActivator { get { return _addinActivator; } }

        public void Read(Stream reader)
        {
            //OperationStatus = (AddinOperationStatus)reader.ReadSByte();

            _addinHeader.Read(reader);
            _addinFilePack.Read(reader);

            var activatorIsNull = reader.ReadBoolean();
            if (!activatorIsNull)
            {
                _addinActivator = new AddinActivatorRecord();
                _addinActivator.Read(reader);
            }

            //_referencedApplicationAssemblies = RecordHelpers.Read(reader, ref ReferencedApplicationAssemblyRecord.Factory);
            _referencedAssemblies = RecordHelpers.Read(reader, ref ReferencedAssemblyRecord.Factory);
            _extendedAddins = RecordHelpers.Read(reader, ref ExtendedAddinRecord.Factory);
            _extendedExtensionPoints = RecordHelpers.Read(reader);

            _extensionPoints = RecordHelpers.Read(reader, ref ExtensionPointRecord.Factory);
            _ebRecordGroups = RecordHelpers.Read(reader, ref ExtensionBuilderRecordGroup.Factory);
            _exRecordGroups = RecordHelpers.Read(reader, ref ExtensionRecordGroup.Factory);
        }

        public void Write(Stream writer)
        {
            //writer.WriteSByte((sbyte)OperationStatus);

            _addinHeader.Write(writer);
            _addinFilePack.Write(writer);

            if (_addinActivator == null)
            {
                writer.WriteBoolean(true);
            }
            else
            {
                writer.WriteBoolean(false);
                _addinActivator.Write(writer);
            }

            //RecordHelpers.Write(writer, _referencedApplicationAssemblies);
            RecordHelpers.Write(writer, _referencedAssemblies);
            RecordHelpers.Write(writer, _extendedAddins);
            RecordHelpers.Write(writer, _extendedExtensionPoints);

            RecordHelpers.Write(writer, _extensionPoints);
            RecordHelpers.Write(writer, _ebRecordGroups);
            RecordHelpers.Write(writer, _exRecordGroups);
        }

        // 将 addinRecord 插入到 list 中，按其 Uid 排序，Uid 越小越靠前。
        internal static void InsetAddinByUid(List<AddinRecord> list, AddinRecord addinRecord)
        {
            if (list.Count == 0)
            {
                list.Add(addinRecord);
                return;
            }

            if (list.Contains(addinRecord))
                return;

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                // 插件 Uid 表示插件的依赖关系以及添加到应用程序的时间。依赖项越少、添加到应用程序的时间越早，Uid 越小。
                if (addinRecord.Uid < item.Uid)
                {
                    list.Insert(i, addinRecord);
                    return;
                }
            }

            // 比所有现存插件的 Uid 都大，所以添加到末尾。
            list.Add(addinRecord);
        }

        // todo:
        //internal abstract AddinResolution ToResolution();
    }
}
