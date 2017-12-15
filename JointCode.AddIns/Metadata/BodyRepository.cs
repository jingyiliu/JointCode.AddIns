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
using JointCode.AddIns.Core.Storage;
using JointCode.AddIns.Metadata.Assets;

namespace JointCode.AddIns.Metadata
{
    class BodyRepository
    {	
    	Storage _storage;
    	readonly Dictionary<Guid, AddinBodyRecord> _guid2Addins;
        List<AddinBodyRecord> _deletedAddins, _addedAddins;
        bool _changed;
    	
    	internal BodyRepository()
    	{
    	    _changed = false;
    		_guid2Addins = new Dictionary<Guid, AddinBodyRecord>();
    	}

        internal Storage Storage
        {
            get { return _storage; }
            set { _storage = value; }
        }

        internal bool Changed { get { return _changed; } }

        internal void Add(AddinBodyRecord addin)
        {
            _changed = true;
            _addedAddins = _addedAddins ?? new List<AddinBodyRecord>();
            _addedAddins.Add(addin);
        }

        internal bool Remove(AddinBodyRecord addin)
        {
            _changed = true;
            _deletedAddins = _deletedAddins ?? new List<AddinBodyRecord>();
            _deletedAddins.Add(addin);
            return true;
        }

        internal void Flush()
        {
            if (!_changed)
                return;

            if (_deletedAddins != null)
            {
                foreach (var deletedAddin in _deletedAddins)
                    _storage.DeleteStream(deletedAddin.Guid);
            }

            if (_addedAddins != null)
            {
                foreach (var addedAddin in _addedAddins)
                {
                    if (!_storage.ContainsStream(addedAddin.Guid))
                        _storage.CreateStream(addedAddin.Guid);
                    using (var stream = _storage.OpenStream(addedAddin.Guid))
                        addedAddin.Write(stream);
                }
            }

            _changed = false;
        }

        internal bool TryGet(Guid guid, out AddinBodyRecord addin)
        {
        	if (_guid2Addins.TryGetValue(guid, out addin))
        		return true;
        	
        	if (!_storage.ContainsStream(guid))
                return false;
        	using (var stream = _storage.OpenStream(guid)) 
        	{
        		addin = new AddinBodyRecord(guid);
	        	addin.Read(stream);
        	}
        	
        	_guid2Addins.Add(guid, addin);
            return true;
        }

        internal void ResetCache()
        {
            _guid2Addins.Clear();
        }
    }
}
