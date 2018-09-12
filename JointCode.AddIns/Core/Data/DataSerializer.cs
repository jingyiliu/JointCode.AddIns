//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.Common.Helpers;
using JointCode.Common.IO;
using System;
using System.IO;

namespace JointCode.AddIns.Core.Data
{
    static class DataSerializer
    {
        internal static DataHolder Read(Stream reader)
        {
            var c = reader.ReadSByte();
            switch (c)
            {
                case StringDataTransformer.StringHolder.TypeCode:
                    var str = new StringDataTransformer.StringHolder();
                    str.Read(reader);
                    return str;

                case Int32DataTransformer.Int32Holder.TypeCode:
                    var i32 = new Int32DataTransformer.Int32Holder();
                    i32.Read(reader);
                    return i32;

                case TypeHandleDataTransformer.TypeHandleHolder.TypeCode:
                    var tid = new TypeHandleDataTransformer.TypeHandleHolder();
                    tid.Read(reader);
                    return tid;

                case BooleanDataTransformer.BooleanHolder.TypeCode:
                    var boo = new BooleanDataTransformer.BooleanHolder();
                    boo.Read(reader);
                    return boo;

                case VersionDataTransformer.VersionHolder.TypeCode:
                    var ver = new VersionDataTransformer.VersionHolder();
                    ver.Read(reader);
                    return ver;

                case DateTimeDataTransformer.DateTimeHolder.TypeCode:
                    var dt = new DateTimeDataTransformer.DateTimeHolder();
                    dt.Read(reader);
                    return dt;

                case GuidDataTransformer.GuidHolder.TypeCode:
                    var guid = new GuidDataTransformer.GuidHolder();
                    guid.Read(reader);
                    return guid;

                case TimeSpanDataTransformer.TimeSpanHolder.TypeCode:
                    var ts = new TimeSpanDataTransformer.TimeSpanHolder();
                    ts.Read(reader);
                    return ts;

                case Int64DataTransformer.Int64Holder.TypeCode:
                    var i64 = new Int64DataTransformer.Int64Holder();
                    i64.Read(reader);
                    return i64;

                case UInt64DataTransformer.UInt64Holder.TypeCode:
                    var ui64 = new UInt64DataTransformer.UInt64Holder();
                    ui64.Read(reader);
                    return ui64;

                case UInt32DataTransformer.UInt32Holder.TypeCode:
                    var ui32 = new UInt32DataTransformer.UInt32Holder();
                    ui32.Read(reader);
                    return ui32;

                case Int16DataTransformer.Int16Holder.TypeCode:
                    var i16 = new Int16DataTransformer.Int16Holder();
                    i16.Read(reader);
                    return i16;

                case UInt16DataTransformer.UInt16Holder.TypeCode:
                    var ui16 = new UInt16DataTransformer.UInt16Holder();
                    ui16.Read(reader);
                    return ui16;

                case ByteDataTransformer.ByteHolder.TypeCode:
                    var bt = new ByteDataTransformer.ByteHolder();
                    bt.Read(reader);
                    return bt;

                case SByteDataTransformer.SByteHolder.TypeCode:
                    var sbt = new SByteDataTransformer.SByteHolder();
                    sbt.Read(reader);
                    return sbt;

                case CharDataTransformer.CharHolder.TypeCode:
                    var ch = new CharDataTransformer.CharHolder();
                    ch.Read(reader);
                    return ch;

                case DecimalDataTransformer.DecimalHolder.TypeCode:
                    var dc = new DecimalDataTransformer.DecimalHolder();
                    dc.Read(reader);
                    return dc;

                case DoubleDataTransformer.DoubleHolder.TypeCode:
                    var dbl = new DoubleDataTransformer.DoubleHolder();
                    dbl.Read(reader);
                    return dbl;

                case SingleDataTransformer.SingleHolder.TypeCode:
                    var sgl = new SingleDataTransformer.SingleHolder();
                    sgl.Read(reader);
                    return sgl;

                default:
                    ExceptionHelper.Handle(new InvalidOperationException("Unexpected value type!"));
                    return null;
            }
        }

        internal static void Write(Stream writer, DataHolder obj)
        {
            writer.WriteSByte(obj.KnownTypeCode);
            obj.Write(writer);
        }
    }
}