using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Core
{
    public class AddinRepository
    {
        readonly Dictionary<Guid, Addin> _guid2Addins;

        internal AddinRepository()
        {
            _guid2Addins = new Dictionary<Guid, Addin>();
        }

        public int AddinCount { get { return _guid2Addins.Count; }}

        public IEnumerable<Addin> Addins
        {
            get
            {
                foreach (var guid2Addin in _guid2Addins)
                    yield return guid2Addin.Value;
            }
        }

        internal void Reset() { _guid2Addins.Clear(); }

        internal Addin[] GetStartedAddins()
        {
            var result = new List<Addin>();
            foreach (var guid2Addin in _guid2Addins)
            {
                if (guid2Addin.Value.Started)
                    result.Add(guid2Addin.Value);
            }
            return result.ToArray();
        }

        internal void AddAddin(Addin addin)
        {
            _guid2Addins.Add(addin.Header.AddinId.Guid, addin);
        }

        internal Addin GetAddin(ref Guid guid)
        {
            return _guid2Addins[guid];
        }

        internal bool TryGetAddin(ref Guid guid, out Addin addin)
        {
            return _guid2Addins.TryGetValue(guid, out addin);
        }

        public Addin Get(Guid guid)
        {
            return _guid2Addins[guid];
        }

        public bool TryGet(Guid guid, out Addin addin)
        {
            return _guid2Addins.TryGetValue(guid, out addin);
        }

        public Addin Get(string name)
        {
            foreach (var guid2Addin in _guid2Addins)
            {
                if (guid2Addin.Value.AddinRecord.AddinHeader.Name == name)
                    return guid2Addin.Value;
            }
            return null;
        }

        public bool TryGet(string name, out Addin addin)
        {
            addin = null;
            foreach (var guid2Addin in _guid2Addins)
            {
                if (guid2Addin.Value.AddinRecord.AddinHeader.Name == name)
                {
                    addin = guid2Addin.Value;
                    break;
                }
            }
            return addin != null;
        }
    }
}