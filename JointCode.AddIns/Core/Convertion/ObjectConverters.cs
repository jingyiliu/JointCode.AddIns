//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Core.Convertion
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

    class StringToDateTimeConverter : ObjectConverter<string, DateTime>
    {
        public override DateTime Convert(string input) { return DateTime.Parse(input); }
        public override bool TryConvert(string input, out DateTime output) { return DateTime.TryParse(input, out output); }
    }

    class StringToTimeSpanConverter : ObjectConverter<string, TimeSpan>
    {
        public override TimeSpan Convert(string input) { return TimeSpan.Parse(input); }
        public override bool TryConvert(string input, out TimeSpan output) { return TimeSpan.TryParse(input, out output); }
    }

    class StringToDecimalConverter : ObjectConverter<string, Decimal>
    {
        public override Decimal Convert(string input) { return Decimal.Parse(input); }
        public override bool TryConvert(string input, out Decimal output) { return Decimal.TryParse(input, out output); }
    }

    class StringToBooleanConverter : ObjectConverter<string, bool>
    {
        public override bool Convert(string input) { return bool.Parse(input); }
        public override bool TryConvert(string input, out bool output) { return bool.TryParse(input, out output); }
    }
    
    class StringToCharConverter : ObjectConverter<string, Char>
    {
        public override Char Convert(string input) { return Char.Parse(input); }
        public override bool TryConvert(string input, out Char output) { return Char.TryParse(input, out output); }
    }

    class StringToSByteConverter : ObjectConverter<string, SByte>
    {
        public override SByte Convert(string input) { return SByte.Parse(input); }
        public override bool TryConvert(string input, out SByte output) { return SByte.TryParse(input, out output); }
    }

    class StringToByteConverter : ObjectConverter<string, Byte>
    {
        public override Byte Convert(string input) { return Byte.Parse(input); }
        public override bool TryConvert(string input, out Byte output) { return Byte.TryParse(input, out output); }
    }

    class StringToInt16Converter : ObjectConverter<string, Int16>
    {
        public override Int16 Convert(string input) { return Int16.Parse(input); }
        public override bool TryConvert(string input, out Int16 output) { return Int16.TryParse(input, out output); }
    }

    class StringToUInt16Converter : ObjectConverter<string, UInt16>
    {
        public override UInt16 Convert(string input) { return UInt16.Parse(input); }
        public override bool TryConvert(string input, out UInt16 output) { return UInt16.TryParse(input, out output); }
    }

    class StringToInt32Converter : ObjectConverter<string, int>
    {
        public override int Convert(string input) { return int.Parse(input); }
        public override bool TryConvert(string input, out int output) { return int.TryParse(input, out output); }
    }

    class StringToUInt32Converter : ObjectConverter<string, UInt32>
    {
        public override UInt32 Convert(string input) { return UInt32.Parse(input); }
        public override bool TryConvert(string input, out UInt32 output) { return UInt32.TryParse(input, out output); }
    }

    class StringToInt64Converter : ObjectConverter<string, Int64>
    {
        public override Int64 Convert(string input) { return Int64.Parse(input); }
        public override bool TryConvert(string input, out Int64 output) { return Int64.TryParse(input, out output); }
    }

    class StringToUInt64Converter : ObjectConverter<string, UInt64>
    {
        public override UInt64 Convert(string input) { return UInt64.Parse(input); }
        public override bool TryConvert(string input, out UInt64 output) { return UInt64.TryParse(input, out output); }
    }

    class StringToSingleConverter : ObjectConverter<string, Single>
    {
        public override Single Convert(string input) { return Single.Parse(input); }
        public override bool TryConvert(string input, out Single output) { return Single.TryParse(input, out output); }
    }

    class StringToDoubleConverter : ObjectConverter<string, Double>
    {
        public override Double Convert(string input) { return Double.Parse(input); }
        public override bool TryConvert(string input, out Double output) { return Double.TryParse(input, out output); }
    }

    //class StringToTypeIdConverter : ObjectConverter<string, TypeId>
    //{
    //    public override TypeId Convert(string input) { return TypeId.Parse(input); }
    //    public override bool TryConvert(string input, out TypeId output) { return TypeId.TryParse(input, out output); }
    //}
}
