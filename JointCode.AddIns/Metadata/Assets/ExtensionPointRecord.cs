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
    class ExtensionPointRecord : ISerializableRecord, IEquatable<ExtensionPointRecord>
    {
        internal static MyFunc<ExtensionPointRecord> Factory = () => new ExtensionPointRecord();
        protected List<ExtensionBuilderRecord> _children;

        // Guid 可以在不同应用程序之间唯一地标识一个 Addin，而由不同 Addin 提供的所有 ExtensionPoint 在一个应用程序中必须唯一。
        /// <summary>
        /// Gets the id of <see cref="IExtensionPoint"/> that uniquely identify an <see cref="IExtensionPoint"/> within an application.
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of an <see cref="ExtensionPointRecord"/>.
        /// </summary>
        internal int Uid { get; set; }

        internal string Description { get; set; }

        // The uid of assembly where the IExtensionPoint/IExtensionBuilder type resides
        internal int AssemblyUid { get; set; }

        // The type name of the IExtensionPoint/IExtensionBuilder
        internal string TypeName { get; set; }

        internal List<ExtensionBuilderRecord> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderRecord item)
        {
            _children = _children ?? new List<ExtensionBuilderRecord>();
            _children.Add(item);
        }

        public virtual void Read(Stream reader)
        {
            DoRead(reader);

            var childCount = reader.ReadInt32();
            if (childCount > 0)
            {
                _children = new List<ExtensionBuilderRecord>(childCount);
                for (int i = 0; i < childCount; i++)
                {
                    var child = ExtensionBuilderRecordHelper.Read(reader, Id, Id);
                    child.Read(reader);
                    _children.Add(child);
                }
            }
        }

        protected void DoRead(Stream reader)
        {
            Id = reader.ReadString();
            Uid = reader.ReadInt32();
            Description = reader.ReadString();
            AssemblyUid = reader.ReadInt32();
            TypeName = reader.ReadString();
        }

        public virtual void Write(Stream writer)
        {
            DoWrite(writer);

            if (_children == null || _children.Count == 0)
            {
                writer.WriteInt32(0);
            }
            else
            {
                writer.WriteInt32(_children.Count);
                for (int i = 0; i < _children.Count; i++)
                    ExtensionBuilderRecordHelper.Write(writer, _children[i]);
            }
        }

        void DoWrite(Stream writer)
        {
            writer.WriteString(Id);
            writer.WriteInt32(Uid);
            writer.WriteString(Description);
            writer.WriteInt32(AssemblyUid);
            writer.WriteString(TypeName);
        }

        #region IEquatable<ExtensionPointRecord> Members

        public bool Equals(ExtensionPointRecord other)
        {
            return Id == other.Id;
        }

        #endregion
    }
}
