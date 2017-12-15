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
using JointCode.Common.Helpers;
using JointCode.Common.IO;

namespace JointCode.AddIns.Core.Serialization
{
    static class SerializationHelper
    {
        internal static SerializableHolder Read(Stream reader)
        {
            var c = reader.ReadSByte();
            switch (c)
            {
                case StringHolder.TypeCode:
                    var str = new StringHolder();
                    str.Read(reader);
                    return str;

                case Int32Holder.TypeCode:
                    var i32 = new Int32Holder();
                    i32.Read(reader);
                    return i32;

                case TypeIdHolder.TypeCode:
                    var tid = new TypeIdHolder();
                    tid.Read(reader);
                    return tid;

                case BooleanHolder.TypeCode:
                    var boo = new BooleanHolder();
                    boo.Read(reader);
                    return boo;

                case VersionHolder.TypeCode:
                    var ver = new VersionHolder();
                    ver.Read(reader);
                    return ver;

                case DateTimeHolder.TypeCode:
                    var dt = new DateTimeHolder();
                    dt.Read(reader);
                    return dt;

                case GuidHolder.TypeCode:
                    var guid = new GuidHolder();
                    guid.Read(reader);
                    return guid;

                case TimeSpanHolder.TypeCode:
                    var ts = new TimeSpanHolder();
                    ts.Read(reader);
                    return ts;

                case Int64Holder.TypeCode:
                    var i64 = new Int64Holder();
                    i64.Read(reader);
                    return i64;

                case UInt64Holder.TypeCode:
                    var ui64 = new UInt64Holder();
                    ui64.Read(reader);
                    return ui64;

                case UInt32Holder.TypeCode:
                    var ui32 = new UInt32Holder();
                    ui32.Read(reader);
                    return ui32;

                case Int16Holder.TypeCode:
                    var i16 = new Int16Holder();
                    i16.Read(reader);
                    return i16;

                case UInt16Holder.TypeCode:
                    var ui16 = new UInt16Holder();
                    ui16.Read(reader);
                    return ui16;

                case ByteHolder.TypeCode:
                    var bt = new ByteHolder();
                    bt.Read(reader);
                    return bt;

                case SByteHolder.TypeCode:
                    var sbt = new SByteHolder();
                    sbt.Read(reader);
                    return sbt;

                case CharHolder.TypeCode:
                    var ch = new CharHolder();
                    ch.Read(reader);
                    return ch;

                case DecimalHolder.TypeCode:
                    var dc = new DecimalHolder();
                    dc.Read(reader);
                    return dc;

                case DoubleHolder.TypeCode:
                    var dbl = new DoubleHolder();
                    dbl.Read(reader);
                    return dbl;

                case SingleHolder.TypeCode:
                    var sgl = new SingleHolder();
                    sgl.Read(reader);
                    return sgl;

                default:
                    ExceptionHelper.Handle(new InvalidOperationException("Unexpected value type!"));
                    return null;
            }
        }

        internal static void Write(Stream writer, SerializableHolder obj)
        {
            writer.WriteSByte(obj.KnownTypeCode);
            obj.Write(writer);
        }
    }
}