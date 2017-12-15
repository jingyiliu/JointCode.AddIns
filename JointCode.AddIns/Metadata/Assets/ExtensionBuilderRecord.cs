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
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Helpers;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
	// a list of extensions that extends a same extension point or parent extension
	class ExtensionBuilderRecordGroup : ISerializableRecord
    {
        internal static MyFunc<ExtensionBuilderRecordGroup> Factory = () => new ExtensionBuilderRecordGroup();
        List<ExtensionBuilderRecord> _children;

        internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension
        internal List<ExtensionBuilderRecord> Children { get { return _children; } }

        internal void AddChild(ExtensionBuilderRecord item)
        {
            _children = _children ?? new List<ExtensionBuilderRecord>();
            _children.Add(item);
        }
        
        public void Read(Stream reader)
        {
        	ParentPath = reader.ReadString();
            var extensionPointId = StringHelper.GetExtensionPointId(ParentPath);
        	var childCount = reader.ReadInt32();
            _children = new List<ExtensionBuilderRecord>(childCount);
        	for (int i = 0; i < childCount; i++) 
        	{
                var child = ExtensionBuilderRecordHelper.Read(reader, extensionPointId, ParentPath);
        		child.Read(reader);
        		Children.Add(child);
        	}
        }

        public void Write(Stream writer)
        {
        	writer.WriteString(ParentPath);
            writer.WriteInt32(_children.Count);
            for (int i = 0; i < _children.Count; i++)
                ExtensionBuilderRecordHelper.Write(writer, _children[i]);
        }
    }

    static class ExtensionBuilderRecordHelper
    {
        internal static ExtensionBuilderRecord Read(Stream reader, string extensionPointId, string parentPath)
        {
            var ebKind = (ExtensionBuilderKind)reader.ReadSByte();
            if (ebKind == ExtensionBuilderKind.Declared)
                return new DeclaredExtensionBuilderRecord { ExtensionPointId = extensionPointId, ParentPath = parentPath };
            else
                return new ReferencedExtensionBuilderRecord { ExtensionPointId = extensionPointId, ParentPath = parentPath };
        }

        internal static void Write(Stream writer, ExtensionBuilderRecord obj)
        {
            writer.WriteSByte((sbyte)obj.ExtensionBuilderKind);
            obj.Write(writer);
        }
    }
    
    abstract class ExtensionBuilderRecord : ExtensionPointRecord, IEquatable<ExtensionBuilderRecord>
    {
        /// <summary>
        /// Gets the path to the parent of this extension builder (can be an <see cref="IExtensionPoint"/> or another <see cref="IExtensionBuilder"/>s).
        /// </summary>
        internal string ParentPath { get; set; }

        internal string ExtensionPointId { get; set; }

        internal string GetPath()
        {
            return ExtensionPointId + SysConstants.PathSeparator + Id;
        }

        public override void Read(Stream reader)
        {
            DoRead(reader);
            var thisPath = GetPath();

            var childCount = reader.ReadInt32();
            if (childCount > 0)
            {
                _children = new List<ExtensionBuilderRecord>(childCount);
                for (int i = 0; i < childCount; i++)
                {
                    var child = ExtensionBuilderRecordHelper.Read(reader, ExtensionPointId, thisPath);
                    child.Read(reader);
                    _children.Add(child);
                }
            }
        }

        internal abstract ExtensionBuilderKind ExtensionBuilderKind { get; }

        public bool Equals(ExtensionBuilderRecord other)
        {
            return Id == other.Id && ParentPath == other.ParentPath;
        }
    }

    class DeclaredExtensionBuilderRecord : ExtensionBuilderRecord
    {
        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Declared; } }
    }

    class ReferencedExtensionBuilderRecord : ExtensionBuilderRecord
    {
        internal override ExtensionBuilderKind ExtensionBuilderKind { get { return ExtensionBuilderKind.Referenced; } }
    }
}