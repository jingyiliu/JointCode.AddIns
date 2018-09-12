using JointCode.AddIns.Extension;
using JointCode.Common.Collections;
using JointCode.Common.Logging;
using System;
using System.Collections.Generic;
using JointCode.AddIns.Core;
using JointCode.Events;

namespace JointCode.AddIns
{
    public partial class AddinFramework
    {
        //readonly AddinConfiguration _configuration;
        readonly INameConvention _nameConvention;
        readonly AddinFileSettings _fileSettings;
        readonly AssemblyLoadPolicy _assemblyLoadPolicy;
        readonly AddinRepository _repository;
        readonly IMessageDialog _messageDialog;
        readonly IExtensionBuilderFactory _ebFactory;
        readonly IExtensionPointFactory _epFactory;
        readonly ILogger _logger;
        readonly IServiceProvider _serviceProvider;
        readonly IEventBroker _eventBroker;
        readonly IStringLocalizer _stringLocalizer;

        internal AddinFramework(AddinOptions options)
        {
            _repository = new AddinRepository();

            _messageDialog = options.MessageDialog ?? new DefaultMessageDialog();
            _nameConvention = options.NameConvention ?? new DefaultNameConvention();
            _fileSettings = options.FileSettings ?? new AddinFileSettings();
            _assemblyLoadPolicy = options.AssemblyLoadPolicy ?? new DefaultAssemblyLoadPolicy();
            _ebFactory = options.ExtensionBuilderFactory ?? new ReflectionExtensionBuilderFactory();
            _epFactory = options.ExtensionPointFactory ?? new ReflectionExtensionPointFactory();
            _eventBroker = options.EventBroker ?? new EventBroker(new EventBrokerOption());
            _serviceProvider = options.ServiceProvider ?? new DefaultServiceProvider();
            _stringLocalizer = options.StringLocalizer ?? new DefaultStringLocalizer();

            LogManager.Initialize(new FileLogSetting(_fileSettings.DataDirectory, AddinFileSettings.LogFileName));
            _logger = LogManager.GetDefaultLogger();

            _properties = new Dictionary<HashKey, object>();
        }
        
        internal IExtensionBuilderFactory ExtensionBuilderFactory { get { return _ebFactory; } }
        internal IExtensionPointFactory ExtensionPointFactory { get { return _epFactory; } }

        public AddinRepository Repository { get { return _repository; } }

        public IMessageDialog MessageDialog { get { return _messageDialog; } }
        public INameConvention NameConvention { get { return _nameConvention; } }
        public AddinFileSettings FileSettings { get { return _fileSettings; } }
        public AssemblyLoadPolicy AssemblyLoadPolicy { get { return _assemblyLoadPolicy; } }
        internal bool UseShadowCopy { get { return _assemblyLoadPolicy.UseShadowCopy; } }
        internal string ShadowCopyDirectory { get { return _assemblyLoadPolicy.ShadowCopyDirectory; } }

        public ILogger Logger { get { return _logger; } }
        public IServiceProvider ServiceProvider { get { return _serviceProvider; } }
        public IEventBroker EventBroker { get { return _eventBroker; } }
        public IStringLocalizer StringLocalizer { get { return _stringLocalizer; } }
    }

    partial class AddinFramework
    {
        class StringTypeHashKey : HashKey
        {
            readonly string _key;
            readonly Type _type;

            internal StringTypeHashKey(string key, Type type) { _key = key; _type = type; }

            public override int GetHashCode()
            {
                return _key.GetHashCode() ^ _type.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                var other = obj as StringTypeHashKey;
                if (other == null)
                    return false;
                return _type == other._type && _key == other._key;
            }
        }

        readonly object _syncObj = new object();
        readonly Dictionary<HashKey, object> _properties;

        public IEnumerable<KeyValuePair<HashKey, object>> Properties { get { return _properties; } }
        public int PropertyCount { get { return _properties == null ? 0 : _properties.Count; } }

        #region string key
        public void SetProperty(string key, object value)
        {
            lock (_syncObj)
                _properties[new HashKey<string>(key)] = value;
        }

        public void UnsetProperty(string key)
        {
            lock (_syncObj)
                _properties.Remove(new HashKey<string>(key));
        }

        public object GetProperty(string key)
        {
            lock (_syncObj)
                return _properties[new HashKey<string>(key)];
        }

        public bool TryGetProperty(string key, out object value)
        {
            lock (_syncObj)
                return _properties.TryGetValue(new HashKey<string>(key), out value);
        }

        public bool ContainsPropertyKey(string key)
        {
            lock (_syncObj)
                return _properties.ContainsKey(new HashKey<string>(key));
        }
        #endregion

        #region string and type key
        public void SetProperty<T>(string key, T value)
        {
            lock (_syncObj)
                _properties[new StringTypeHashKey(key, typeof(T))] = value;
        }

        public void UnsetProperty(string key, Type type)
        {
            lock (_syncObj)
                _properties.Remove(new StringTypeHashKey(key, type));
        }

        public T GetProperty<T>(string key)
        {
            lock (_syncObj)
                return (T)_properties[new StringTypeHashKey(key, typeof(T))];
        }

        public bool TryGetProperty<T>(string key, out T value)
        {
            object val;
            bool result;
            lock (_syncObj)
                result = _properties.TryGetValue(new StringTypeHashKey(key, typeof(T)), out val);

            if (!result)
            {
                value = default(T);
                return false;
            }
            try
            {
                value = (T)val;
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public bool ContainsPropertyKey(string key, Type type)
        {
            lock (_syncObj)
                return _properties.ContainsKey(new StringTypeHashKey(key, type));
        }
        #endregion

        #region type key
        public void SetProperty<T>(T value)
        {
            lock (_syncObj)
                _properties[new HashKey<Type>(typeof(T))] = value;
        }

        public void UnsetProperty(Type type)
        {
            lock (_syncObj)
                _properties.Remove(new HashKey<Type>(type));
        }

        public T GetProperty<T>()
        {
            lock (_syncObj)
                return (T)_properties[new HashKey<Type>(typeof(T))];
        }

        public bool TryGetProperty<T>(out T value)
        {
            object val;
            bool result;
            lock (_syncObj)
                result = _properties.TryGetValue(new HashKey<Type>(typeof(T)), out val);

            if (!result)
            {
                value = default(T);
                return false;
            }
            try
            {
                value = (T)val;
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public bool ContainsPropertyKey(Type type)
        {
            lock (_syncObj)
                return _properties.ContainsKey(new HashKey<Type>(type));
        }
        #endregion
    }
}