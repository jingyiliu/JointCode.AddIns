//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core.Data;
using System.Collections.Generic;

namespace JointCode.AddIns.Extension
{
    public class ExtensionData
    {
        internal Dictionary<string, DataHolder> _items;

        protected ExtensionData() { }
        internal ExtensionData(Dictionary<string, DataHolder> items) { _items = items; }

        internal Dictionary<string, DataHolder> Items { get { return _items; } }

        public object this[string key] { get { return _items != null ? _items[key].Value : null; } }

        public bool TryGet(string key, out object value)
        {
            if (_items == null)
            {
                value = null;
                return false;
            }
            DataHolder holder;
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
            DataHolder holder;
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