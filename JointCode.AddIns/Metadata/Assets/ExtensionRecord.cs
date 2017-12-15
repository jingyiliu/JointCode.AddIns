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
using JointCode.AddIns.Core.Serialization;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
	// a list of extensions that extends a same extension point or parent extension
    class ExtensionRecordGroup : ISerializableRecord
    {
        internal static MyFunc<ExtensionRecordGroup> Factory = () => new ExtensionRecordGroup();
        List<ExtensionRecord> _children;

        internal bool RootIsExtensionPoint { get; set; }
        internal string ParentPath { get; set; } // can be the id of extension point, or the path to another extension
        internal List<ExtensionRecord> Children { get { return _children; } }

        internal void AddChild(ExtensionRecord item)
        {
            _children = _children ?? new List<ExtensionRecord>();
            _children.Add(item);
        }
        
        public void Read(Stream reader)
        {
        	ParentPath = reader.ReadString();
            RootIsExtensionPoint = reader.ReadBoolean();

        	var childCount = reader.ReadInt32();
            if (childCount > 0)
            {
                _children = new List<ExtensionRecord>(childCount);
                for (int i = 0; i < childCount; i++)
                {
                    var child = new ExtensionRecord();
                    child.Read(reader, ParentPath);
                    _children.Add(child);
                }
            }
        }

        public void Write(Stream writer)
        {
        	writer.WriteString(ParentPath);
            writer.WriteBoolean(RootIsExtensionPoint);

            if (_children == null || _children.Count == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(_children.Count);
            if (_children.Count > 0)
            {
                for (int i = 0; i < _children.Count; i++)
                    _children[i].Write(writer);
            }
        }
    }
    
    class ExtensionRecord
    {
        List<ExtensionRecord> _children;

        internal ExtensionHeadRecord Head { get; set; }
        internal ExtensionDataRecord Data { get; set; }

        internal List<ExtensionRecord> Children { get { return _children; } }

        internal void AddChild(ExtensionRecord item)
        {
            _children = _children ?? new List<ExtensionRecord>();
            _children.Add(item);
        }

        internal void Read(Stream reader, string parentPath)
        {
        	Head = new ExtensionHeadRecord();
        	Head.Read(reader);
            Head.ParentPath = parentPath; // assign the parent path first!!!!
        	Data = new ExtensionDataRecord();
        	Data.Read(reader);
        	
        	var childCount = reader.ReadInt32();
        	if (childCount > 0) 
        	{
                _children = new List<ExtensionRecord>(childCount);
        		for (int i = 0; i < childCount; i++) 
	        	{
	        		var child = new ExtensionRecord();
                    child.Read(reader, Head.Path);
                    _children.Add(child);
	        	}
        	}
        }

        internal void Write(Stream writer)
        {
        	Head.Write(writer);
        	Data.Write(writer);

            if (_children == null || _children.Count == 0) 
        	{
        		writer.WriteInt32(0);
        	}
        	else
        	{
                writer.WriteInt32(_children.Count);
                for (int i = 0; i < _children.Count; i++)
                    _children[i].Write(writer);
        	}
        }
    }
    
    /// <summary>
    /// This class is mainly used to:
    /// 1. link the <see cref="ExtensionDataRecord"/> to a concrete <see cref="IExtensionBuilder"/> implementation type.
    /// 2. provide the insert position of this extension in its parent extension or extension point.
    /// </summary>
    class ExtensionHeadRecord : BaseExtensionHeadResolution
    {
        internal RelativePosition RelativePosition { get; set; }
        
        internal void Read(Stream reader)
        {
            Id = reader.ReadString();
            SiblingId = reader.ReadString();
            RelativePosition = (RelativePosition)reader.ReadSByte();
            ExtensionBuilderUid = reader.ReadInt32();
        }

        internal void Write(Stream writer)
        {
            writer.WriteString(Id);
            writer.WriteString(SiblingId);
            writer.WriteSByte((sbyte)RelativePosition);
            writer.WriteInt32(ExtensionBuilderUid);
        }
    }

    class ExtensionDataRecord : ExtensionData
    {
        internal ExtensionDataRecord() { }
        internal ExtensionDataRecord(Dictionary<string, SerializableHolder> items)
            : base(items) { }

        internal void Add(string key, SerializableHolder value)
        {
            _items = _items ?? new Dictionary<string, SerializableHolder>();
            _items[key] = value;
        }

        internal void Read(Stream reader)
        {
            _items = ReadDictionary(reader);
        }

        internal void Write(Stream writer)
        {
            WriteDictionary(writer, _items);
        }

        static Dictionary<string, SerializableHolder> ReadDictionary(Stream reader)
        {
            var count = reader.ReadInt32();
            if (count <= 0)
                return null;

            var result = new Dictionary<string, SerializableHolder>(count);
            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = SerializationHelper.Read(reader);
                result.Add(key, value);
            }
            return result;
        }

        static void WriteDictionary(Stream writer, Dictionary<string, SerializableHolder> value)
        {
            if (value == null || value.Count == 0)
            {
                writer.WriteInt32(0);
                return;
            }
            writer.WriteInt32(value.Count);
            foreach (var kv in value)
            {
                writer.WriteString(kv.Key);
                SerializationHelper.Write(writer, kv.Value);
            }
        }
    }
}
