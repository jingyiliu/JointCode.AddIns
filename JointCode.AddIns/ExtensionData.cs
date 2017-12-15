//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Collections.Generic;
using JointCode.AddIns.Core.Serialization;

namespace JointCode.AddIns
{
    public class ExtensionData
    {
        internal Dictionary<string, SerializableHolder> _items;

        protected ExtensionData() { }
        internal ExtensionData(Dictionary<string, SerializableHolder> items) { _items = items; }

        internal Dictionary<string, SerializableHolder> Items { get { return _items; } }

        public object this[string key] { get { return _items != null ? _items[key].Value : null; } }

        public bool TryGet(string key, out object value)
        {
            if (_items == null)
            {
                value = null;
                return false;
            }
            SerializableHolder holder;
            if (_items.TryGetValue(key, out holder))
            {
                value = holder.Value;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_items == null)
            {
                value = default(T);
                return false;
            }
            SerializableHolder holder;
            if (_items.TryGetValue(key, out holder))
            {
                value = (T)holder.Value;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}