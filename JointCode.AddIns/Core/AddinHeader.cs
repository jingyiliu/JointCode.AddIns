using System;
using System.Collections.Generic;
using JointCode.AddIns.Core;

namespace JointCode.AddIns.Core
{
    public class AddinHeader
    {
        Dictionary<string, string> _innerProperties; // 静态属性，这是配置的属性
        //Dictionary<string, object> _dynamicProperties; // 动态属性，这是运行时添加或删除的属性

        public AddinId AddinId { get; internal set; }
        //internal bool Enabled { get; set; }
        public Version Version { get; internal set; }
        public Version CompatVersion { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public AddinCategory AddinCategory { get; internal set; }

        //internal string Url { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Properties { get { return _innerProperties; } }
        public int PropertyCount { get { return _innerProperties == null ? 0 : _innerProperties.Count; } }
        internal Dictionary<string, string> InnerProperties { get { return _innerProperties; } set { _innerProperties = value; } }

        public bool TryGetProperty(string key, out string value)
        {
            if (_innerProperties != null)
                return _innerProperties.TryGetValue(key, out value);
            value = null;
            return false;
        }

        public bool ContainsPropertyKey(string key)
        {
            return _innerProperties.ContainsKey(key);
        }

        //public void AddProperty(string key, object value)
        //{
        //    _dynamicProperties = _dynamicProperties ?? new Dictionary<string, object>();
        //    _dynamicProperties.Add(key, value);
        //}

        //public void RemoveProperty(string key)
        //{
        //    if (_dynamicProperties == null)
        //        return;
        //    _dynamicProperties.Remove(key);
        //}

        //public bool TryGetProperty(string key, out object value)
        //{
        //    if (_dynamicProperties != null)
        //        return _dynamicProperties.TryGetValue(key, out value);
        //    value = null;
        //    return false;
        //}
    }
}