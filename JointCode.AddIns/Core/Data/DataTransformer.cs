using JointCode.AddIns.Resolving.Assets;
using JointCode.Common.Conversion;
using JointCode.Common.IO;
using System;
using System.IO;
using JointCode.AddIns.Resolving;

namespace JointCode.AddIns.Core.Data
{
    abstract class DataTransformer
    {
        readonly Type _extensionDataType;

        protected DataTransformer(Type extensionDataType)
        {
            _extensionDataType = extensionDataType;
        }

        internal Type ExtensionDataType { get { return _extensionDataType; } }

        internal abstract void Intialize(ResolutionContext ctx, ConvertionManager cm);
        internal abstract bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex);

        internal bool CanTransform(DataHolder dataHolder)
        {
            return ExtensionDataType.IsInstanceOfType(dataHolder.Value);
        }

        protected void RegisterTo(ResolutionContext ctx)
        {
            ctx.RegisterDataTransformer(_extensionDataType.Assembly.FullName, _extensionDataType.MetadataToken, this);
        }
    }


    class StringDataTransformer : DataTransformer
    {
        internal class StringHolder : DataHolder
        {
            internal const sbyte TypeCode = 1;
            internal StringHolder() { }
            internal StringHolder(object val) : base(val) { }

            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadString(); }
            internal override void Write(Stream writer) { writer.WriteString((String)_val); }
        }

        internal StringDataTransformer() : base(typeof(string)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            var holder = new StringHolder(propData);
            ex.Data.AddDataHolder(propName, holder);
            return true;
        }
    }

    class TypeHandleDataTransformer : DataTransformer
    {
        //class StringToTypeHandleConverter : ObjectConverter<string, TypeId>
        //{
        //    public override TypeId Convert(string input) { return TypeId.Parse(input); }
        //    public override bool TryConvert(string input, out TypeId output) { return TypeId.TryParse(input, out output); }
        //}

        class LazyTypeHandleHolder : TypeHandleHolder
        {
            //internal LazyTypeHandleHolder() { }
            internal LazyTypeHandleHolder(TypeResolution val) : base(val) { }

            internal override void Write(Stream writer)
            {
                var type = _val as TypeResolution;
                var typeHandle = new AddinTypeHandle(type.Assembly.Uid, type.MetadataToken);
                typeHandle.Write(writer);
            }
        }

        internal class TypeHandleHolder : DataHolder
        {
            internal const sbyte TypeCode = 19;
            internal TypeHandleHolder() { }
            internal TypeHandleHolder(object val) : base(val) { }

            internal override sbyte KnownTypeCode { get { return TypeCode; } }

            internal override void Read(Stream reader)
            {
                var val = new AddinTypeHandle();
                val.Read(reader);
                _val = val;
            }

            internal override void Write(Stream writer)
            {
                ((AddinTypeHandle)_val).Write(writer);
            }
        }

        internal TypeHandleDataTransformer() : base(typeof(AddinTypeHandle)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            TypeResolution type;
            // a type dependency is introduced here. 
            // should it be added to the current addin's reference set?
            if (!ctx.TryGetAddinType(ex.DeclaringAddin, propData, out type))
                return false;
            var holder = new LazyTypeHandleHolder(type);
            ex.Data.AddDataHolder(propName, holder);
            return true;
        }
    }


    class GuidDataTransformer : DataTransformer
    {
        class StringToGuidConverter : ObjectConverter<string, Guid>
        {
            public override Guid Convert(string input) { return new Guid(input); }

            public override bool TryConvert(string input, out Guid output)
            {
                try
                {
                    output = new Guid(input);
                    return true;
                }
                catch
                {
                    output = Guid.Empty;
                    return false;
                }
            }
        }

        internal class GuidHolder : DataHolder
        {
            internal const sbyte TypeCode = 3;
            internal GuidHolder() { }
            internal GuidHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadGuid(); }
            internal override void Write(Stream writer) { writer.WriteGuid((Guid)_val); }
        }

        internal GuidDataTransformer() : base(typeof(Guid)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToGuidConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new GuidHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class VersionDataTransformer : DataTransformer
    {
        class StringToVersionConverter : ObjectConverter<string, Version>
        {
            public override Version Convert(string input) { return new Version(input); }

            public override bool TryConvert(string input, out Version output)
            {
                try
                {
                    output = new Version(input);
                    return true;
                }
                catch
                {
                    output = null;
                    return false;
                }
            }
        }

        internal class VersionHolder : DataHolder
        {
            internal const sbyte TypeCode = 2;
            internal VersionHolder() { }
            internal VersionHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadVersion(); }
            internal override void Write(Stream writer) { writer.WriteVersion((Version)_val); }
        }

        internal VersionDataTransformer() : base(typeof(Version)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToVersionConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new VersionHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }


    class DateTimeDataTransformer : DataTransformer
    {
        class StringToDateTimeConverter : ObjectConverter<string, DateTime>
        {
            public override DateTime Convert(string input) { return DateTime.Parse(input); }
            public override bool TryConvert(string input, out DateTime output) { return DateTime.TryParse(input, out output); }
        }

        internal class DateTimeHolder : DataHolder
        {
            internal const sbyte TypeCode = 4;
            internal DateTimeHolder() { }
            internal DateTimeHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadDateTime(); }
            internal override void Write(Stream writer) { writer.WriteDateTime((DateTime)_val); }
        }

        internal DateTimeDataTransformer() : base(typeof(DateTime)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToDateTimeConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new DateTimeHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class TimeSpanDataTransformer : DataTransformer
    {
        class StringToTimeSpanConverter : ObjectConverter<string, TimeSpan>
        {
            public override TimeSpan Convert(string input) { return TimeSpan.Parse(input); }
            public override bool TryConvert(string input, out TimeSpan output) { return TimeSpan.TryParse(input, out output); }
        }

        internal class TimeSpanHolder : DataHolder
        {
            internal const sbyte TypeCode = 5;
            internal TimeSpanHolder() { }
            internal TimeSpanHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadTimeSpan(); }
            internal override void Write(Stream writer) { writer.WriteTimeSpan((TimeSpan)_val); }
        }

        internal TimeSpanDataTransformer() : base(typeof(TimeSpan)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToTimeSpanConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new TimeSpanHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }


    class BooleanDataTransformer : DataTransformer
    {
        class StringToBooleanConverter : ObjectConverter<string, bool>
        {
            public override bool Convert(string input) { return bool.Parse(input); }
            public override bool TryConvert(string input, out bool output) { return bool.TryParse(input, out output); }
        }

        internal class BooleanHolder : DataHolder
        {
            internal const sbyte TypeCode = 7;
            internal BooleanHolder() { }
            internal BooleanHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadBoolean(); }
            internal override void Write(Stream writer) { writer.WriteBoolean((Boolean)_val); }
        }

        internal BooleanDataTransformer() : base(typeof(Boolean)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToBooleanConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new BooleanHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class CharDataTransformer : DataTransformer
    {
        class StringToCharConverter : ObjectConverter<string, Char>
        {
            public override Char Convert(string input) { return Char.Parse(input); }
            public override bool TryConvert(string input, out Char output) { return Char.TryParse(input, out output); }
        }

        internal class CharHolder : DataHolder
        {
            internal const sbyte TypeCode = 8;
            internal CharHolder() { }
            internal CharHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadChar(); }
            internal override void Write(Stream writer) { writer.WriteChar((Char)_val); }
        }

        internal CharDataTransformer() : base(typeof(Char)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToCharConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new CharHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }


    class ByteDataTransformer : DataTransformer
    {
        class StringToByteConverter : ObjectConverter<string, Byte>
        {
            public override Byte Convert(string input) { return Byte.Parse(input); }
            public override bool TryConvert(string input, out Byte output) { return Byte.TryParse(input, out output); }
        }

        internal class ByteHolder : DataHolder
        {
            internal const sbyte TypeCode = 10;
            internal ByteHolder() { }
            internal ByteHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadByte(); }
            internal override void Write(Stream writer) { writer.WriteByte((Byte)_val); }
        }

        internal ByteDataTransformer() : base(typeof(Byte)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToByteConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new ByteHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class SByteDataTransformer : DataTransformer
    {
        class StringToSByteConverter : ObjectConverter<string, SByte>
        {
            public override SByte Convert(string input) { return SByte.Parse(input); }
            public override bool TryConvert(string input, out SByte output) { return SByte.TryParse(input, out output); }
        }

        internal class SByteHolder : DataHolder
        {
            internal const sbyte TypeCode = 9;
            internal SByteHolder() { }
            internal SByteHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadSByte(); }
            internal override void Write(Stream writer) { writer.WriteSByte((SByte)_val); }
        }

        internal SByteDataTransformer() : base(typeof(SByte)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToSByteConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new SByteHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class Int16DataTransformer : DataTransformer
    {
        class StringToInt16Converter : ObjectConverter<string, Int16>
        {
            public override Int16 Convert(string input) { return Int16.Parse(input); }
            public override bool TryConvert(string input, out Int16 output) { return Int16.TryParse(input, out output); }
        }

        internal class Int16Holder : DataHolder
        {
            internal const sbyte TypeCode = 11;
            internal Int16Holder() { }
            internal Int16Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadInt16(); }
            internal override void Write(Stream writer) { writer.WriteInt16((Int16)_val); }
        }

        internal Int16DataTransformer() : base(typeof(Int16)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToInt16Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new Int16Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class UInt16DataTransformer : DataTransformer
    {
        class StringToUInt16Converter : ObjectConverter<string, UInt16>
        {
            public override UInt16 Convert(string input) { return UInt16.Parse(input); }
            public override bool TryConvert(string input, out UInt16 output) { return UInt16.TryParse(input, out output); }
        }

        internal class UInt16Holder : DataHolder
        {
            internal const sbyte TypeCode = 12;
            internal UInt16Holder() { }
            internal UInt16Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadUInt16(); }
            internal override void Write(Stream writer) { writer.WriteUInt16((UInt16)_val); }
        }

        internal UInt16DataTransformer() : base(typeof(UInt16)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToUInt16Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new UInt16Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class Int32DataTransformer : DataTransformer
    {
        class StringToInt32Converter : ObjectConverter<string, Int32>
        {
            public override Int32 Convert(string input) { return Int32.Parse(input); }
            public override bool TryConvert(string input, out Int32 output) { return Int32.TryParse(input, out output); }
        }

        internal class Int32Holder : DataHolder
        {
            internal const sbyte TypeCode = 13;
            internal Int32Holder() { }
            internal Int32Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadInt32(); }
            internal override void Write(Stream writer) { writer.WriteInt32((Int32)_val); }
        }

        internal Int32DataTransformer() : base(typeof(Int32)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToInt32Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new Int32Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class UInt32DataTransformer : DataTransformer
    {
        class StringToUInt32Converter : ObjectConverter<string, UInt32>
        {
            public override UInt32 Convert(string input) { return UInt32.Parse(input); }
            public override bool TryConvert(string input, out UInt32 output) { return UInt32.TryParse(input, out output); }
        }

        internal class UInt32Holder : DataHolder
        {
            internal const sbyte TypeCode = 14;
            internal UInt32Holder() { }
            internal UInt32Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadUInt32(); }
            internal override void Write(Stream writer) { writer.WriteUInt32((UInt32)_val); }
        }

        internal UInt32DataTransformer() : base(typeof(UInt32)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToUInt32Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new UInt32Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class Int64DataTransformer : DataTransformer
    {
        class StringToInt64Converter : ObjectConverter<string, Int64>
        {
            public override Int64 Convert(string input) { return Int64.Parse(input); }
            public override bool TryConvert(string input, out Int64 output) { return Int64.TryParse(input, out output); }
        }

        internal class Int64Holder : DataHolder
        {
            internal const sbyte TypeCode = 15;
            internal Int64Holder() { }
            internal Int64Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadInt64(); }
            internal override void Write(Stream writer) { writer.WriteInt64((Int64)_val); }
        }

        internal Int64DataTransformer() : base(typeof(Int64)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToInt64Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new Int64Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class UInt64DataTransformer : DataTransformer
    {
        class StringToUInt64Converter : ObjectConverter<string, UInt64>
        {
            public override UInt64 Convert(string input) { return UInt64.Parse(input); }
            public override bool TryConvert(string input, out UInt64 output) { return UInt64.TryParse(input, out output); }
        }

        internal class UInt64Holder : DataHolder
        {
            internal const sbyte TypeCode = 16;
            internal UInt64Holder() { }
            internal UInt64Holder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadUInt64(); }
            internal override void Write(Stream writer) { writer.WriteUInt64((UInt64)_val); }
        }

        internal UInt64DataTransformer() : base(typeof(UInt64)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToUInt64Converter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new UInt64Holder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class SingleDataTransformer : DataTransformer
    {
        class StringToSingleConverter : ObjectConverter<string, Single>
        {
            public override Single Convert(string input) { return Single.Parse(input); }
            public override bool TryConvert(string input, out Single output) { return Single.TryParse(input, out output); }
        }

        internal class SingleHolder : DataHolder
        {
            internal const sbyte TypeCode = 17;
            internal SingleHolder() { }
            internal SingleHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadSingle(); }
            internal override void Write(Stream writer) { writer.WriteSingle((Single)_val); }
        }

        internal SingleDataTransformer() : base(typeof(Single)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToSingleConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new SingleHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class DoubleDataTransformer : DataTransformer
    {
        class StringToDoubleConverter : ObjectConverter<string, Double>
        {
            public override Double Convert(string input) { return Double.Parse(input); }
            public override bool TryConvert(string input, out Double output) { return Double.TryParse(input, out output); }
        }

        internal class DoubleHolder : DataHolder
        {
            internal const sbyte TypeCode = 18;
            internal DoubleHolder() { }
            internal DoubleHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadDouble(); }
            internal override void Write(Stream writer) { writer.WriteDouble((Double)_val); }
        }

        internal DoubleDataTransformer() : base(typeof(Double)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToDoubleConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new DoubleHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }

    class DecimalDataTransformer : DataTransformer
    {
        class StringToDecimalConverter : ObjectConverter<string, Decimal>
        {
            public override Decimal Convert(string input) { return Decimal.Parse(input); }
            public override bool TryConvert(string input, out Decimal output) { return Decimal.TryParse(input, out output); }
        }

        internal class DecimalHolder : DataHolder
        {
            internal const sbyte TypeCode = 6;
            internal DecimalHolder() { }
            internal DecimalHolder(object val) : base(val) { }
            internal override sbyte KnownTypeCode { get { return TypeCode; } }
            internal override void Read(Stream reader) { _val = reader.ReadDecimal(); }
            internal override void Write(Stream writer) { writer.WriteDecimal((Decimal)_val); }
        }

        internal DecimalDataTransformer() : base(typeof(Decimal)) { }

        internal override void Intialize(ResolutionContext ctx, ConvertionManager cm)
        {
            RegisterTo(ctx);
            cm.Register(new StringToDecimalConverter());
        }

        internal override bool Transform(string propName, string propData, ResolutionContext ctx, ConvertionManager cm, ExtensionResolution ex)
        {
            // convert to custom type (with an ObjectConverter registered in ConvertionManager).
            var objectConverter = cm.TryGet(typeof(string), ExtensionDataType);
            if (objectConverter == null)
                return false;

            // if an property value is provided for the property name, try to convert it.
            object propValue;
            if (!objectConverter.TryConvert(propData, out propValue))
                return false;

            var holder = new DecimalHolder(propValue);
            ex.Data.AddDataHolder(propName, holder);

            return true;
        }
    }
}