using System;
using System.Collections;
using System.Collections.Generic;

namespace JointCode.AddIns.Metadata
{
    class AddinRecordSet : IEnumerable<AddinRecord>
//: List<AddinRecord>
    {
        readonly List<AddinRecord> _addinRecords = new List<AddinRecord>();

        internal List<AddinRecord> InnerList { get { return _addinRecords; } }

        internal int Count { get { return _addinRecords.Count; } }

        internal AddinRecord this[int index]
        {
            get { return _addinRecords[index]; }
            set { _addinRecords[index] = value; }
        }

        internal AddinRecord SelectFirst()
        {
            return DoSelectFirstOrNull(true);
        }

        internal AddinRecord SelectFirstOrNull()
        {
            return DoSelectFirstOrNull(false);
        }

        AddinRecord DoSelectFirstOrNull(bool fastFail)
        {
            if (_addinRecords.Count == 0)
            {
                if (fastFail)
                    throw new InvalidOperationException("The addin set is empty!");
                return null;
            }

            var i = 0;
            var result = _addinRecords[i];
            if (result.Enabled)
                return result;

            if (_addinRecords.Count <= 1)
            {
                if (fastFail)
                    throw GetNoMatchingItemException();
                return null;
            }

            for (int j = 1; j < _addinRecords.Count; j++)
            {
                result = _addinRecords[j];
                if (result.Enabled)
                    return result;
            }

            if (fastFail)
                throw GetNoMatchingItemException();
            return null;
        }

        Exception GetNoMatchingItemException()
        {
            return new InvalidOperationException("No qualified enabled addin found in the addin set!");
        }

        internal void Add(AddinRecord addinRecord)
        {
            AddinRecord.InsetAddinByUid(_addinRecords, addinRecord);
        }

        internal bool Remove(AddinRecord addinRecord)
        {
            return _addinRecords.Remove(addinRecord);
        }

        internal bool Contains(AddinRecord addinRecord)
        {
            return _addinRecords.Contains(addinRecord);
        }

        public IEnumerator<AddinRecord> GetEnumerator()
        {
            return _addinRecords.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}