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
using JointCode.Common.IO;

namespace JointCode.AddIns.Core.Serialization
{
    // this class is used to postpone the time of getting the real metadata to be written to the persistence file.
    abstract class SerializableHolder
    {
        protected object _val;
        internal SerializableHolder() { }
        internal SerializableHolder(object val) { _val = val; }
        internal object Value { get { return _val; } }
        internal abstract sbyte KnownTypeCode { get; }
        internal abstract void Read(Stream reader);
        internal abstract void Write(Stream writer);
    }

    class StringHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 1;
        internal StringHolder() { }
        internal StringHolder(object val) : base(val) { }

        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadString(); }
        internal override void Write(Stream writer) { writer.WriteString((String)_val); }
    }

    class VersionHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 2;
        internal VersionHolder() { }
        internal VersionHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadVersion(); }
        internal override void Write(Stream writer) { writer.WriteVersion((Version)_val); }
    }

    class GuidHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 3;
        internal GuidHolder() { }
        internal GuidHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadGuid(); }
        internal override void Write(Stream writer) { writer.WriteGuid((Guid)_val); }
    }

    class DateTimeHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 4;
        internal DateTimeHolder() { }
        internal DateTimeHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadDateTime(); }
        internal override void Write(Stream writer) { writer.WriteDateTime((DateTime)_val); }
    }

    class TimeSpanHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 5;
        internal TimeSpanHolder() { }
        internal TimeSpanHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadTimeSpan(); }
        internal override void Write(Stream writer) { writer.WriteTimeSpan((TimeSpan)_val); }
    }

    class DecimalHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 6;
        internal DecimalHolder() { }
        internal DecimalHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadDecimal(); }
        internal override void Write(Stream writer) { writer.WriteDecimal((Decimal)_val); }
    }

    class BooleanHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 7;
        internal BooleanHolder() { }
        internal BooleanHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadBoolean(); }
        internal override void Write(Stream writer) { writer.WriteBoolean((Boolean)_val); }
    }

    class CharHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 8;
        internal CharHolder() { }
        internal CharHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadChar(); }
        internal override void Write(Stream writer) { writer.WriteChar((Char)_val); }
    }

    class SByteHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 9;
        internal SByteHolder() { }
        internal SByteHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadSByte(); }
        internal override void Write(Stream writer) { writer.WriteSByte((SByte)_val); }
    }

    class ByteHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 10;
        internal ByteHolder() { }
        internal ByteHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadByte(); }
        internal override void Write(Stream writer) { writer.WriteByte((Byte)_val); }
    }

    class Int16Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 11;
        internal Int16Holder() { }
        internal Int16Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadInt16(); }
        internal override void Write(Stream writer) { writer.WriteInt16((Int16)_val); }
    }

    class UInt16Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 12;
        internal UInt16Holder() { }
        internal UInt16Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadUInt16(); }
        internal override void Write(Stream writer) { writer.WriteUInt16((UInt16)_val); }
    }

    class Int32Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 13;
        internal Int32Holder() { }
        internal Int32Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadInt32(); }
        internal override void Write(Stream writer) { writer.WriteInt32((Int32)_val); }
    }

    class UInt32Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 14;
        internal UInt32Holder() { }
        internal UInt32Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadUInt32(); }
        internal override void Write(Stream writer) { writer.WriteUInt32((UInt32)_val); }
    }

    class Int64Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 15;
        internal Int64Holder() { }
        internal Int64Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadInt64(); }
        internal override void Write(Stream writer) { writer.WriteInt64((Int64)_val); }
    }

    class UInt64Holder : SerializableHolder
    {
        internal const sbyte TypeCode = 16;
        internal UInt64Holder() { }
        internal UInt64Holder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadUInt64(); }
        internal override void Write(Stream writer) { writer.WriteUInt64((UInt64)_val); }
    }

    class SingleHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 17;
        internal SingleHolder() { }
        internal SingleHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadSingle(); }
        internal override void Write(Stream writer) { writer.WriteSingle((Single)_val); }
    }

    class DoubleHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 18;
        internal DoubleHolder() { }
        internal DoubleHolder(object val) : base(val) { }
        internal override sbyte KnownTypeCode { get { return TypeCode; } }
        internal override void Read(Stream reader) { _val = reader.ReadDouble(); }
        internal override void Write(Stream writer) { writer.WriteDouble((Double)_val); }
    }

    class TypeIdHolder : SerializableHolder
    {
        internal const sbyte TypeCode = 19;
        internal TypeIdHolder() { }
        internal TypeIdHolder(object val) : base(val) { }

        internal override sbyte KnownTypeCode { get { return TypeCode; } }

        internal override void Read(Stream reader)
        {
            var val = new TypeId();
            val.Read(reader);
            _val = val;
        }

        internal override void Write(Stream writer)
        {
            ((TypeId)_val).Write(writer);
        }
    }
}