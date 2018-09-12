//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Extension;
using JointCode.Events;

namespace JointCode.AddIns
{
    public class AddinOptions
    {
        IExtensionBuilderFactory _ebFactory;
        IExtensionPointFactory _epFactory;
        IMessageDialog _messageDialog;
        INameConvention _nameConvention;
        IServiceProvider _serviceProvider;
        IEventBroker _eventBroker;
        IStringLocalizer _stringLocalizer;
        AssemblyLoadPolicy _assemblyLoadPolicy;
        AddinFileSettings _fileSettings;

        private AddinOptions() { }

        public static AddinOptions Create()
        {
            return new AddinOptions();
        }

        public AddinOptions WithFileSettings(AddinFileSettings fileSettings)
        {
            _fileSettings = fileSettings;
            return this;
        }

        public AddinOptions WithAssemblyLoadPolicy(AssemblyLoadPolicy assemblyLoadPolicy)
        {
            _assemblyLoadPolicy = assemblyLoadPolicy;
            return this;
        }

        public AddinOptions WithMessageDialog(IMessageDialog messageDialog)
        {
            _messageDialog = messageDialog;
            return this;
        }

        public AddinOptions WithNameConvention(INameConvention nameConvention)
        {
            _nameConvention = nameConvention;
            return this;
        }

        public AddinOptions WithExtensionBuilderFactory(IExtensionBuilderFactory extensionBuilderFactory)
        {
            _ebFactory = extensionBuilderFactory;
            return this;
        }

        public AddinOptions WithExtensionPointFactory(IExtensionPointFactory extensionPointFactory)
        {
            _epFactory = extensionPointFactory;
            return this;
        }

        public AddinOptions WithEventBroker(IEventBroker eventBroker)
        {
            _eventBroker = eventBroker;
            return this;
        }

        public AddinOptions WithServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }

        public AddinOptions WithStringLocalizer(IStringLocalizer stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
            return this;
        }

        public AddinFileSettings FileSettings { get { return _fileSettings; } }
        public AssemblyLoadPolicy AssemblyLoadPolicy { get { return _assemblyLoadPolicy; } }
        public IMessageDialog MessageDialog { get { return _messageDialog; } }
        public INameConvention NameConvention { get { return _nameConvention; } }
        public IExtensionBuilderFactory ExtensionBuilderFactory { get { return _ebFactory; } }
        public IExtensionPointFactory ExtensionPointFactory { get { return _epFactory; } }

        public IEventBroker EventBroker { get { return _eventBroker; } }
        public IServiceProvider ServiceProvider { get { return _serviceProvider; } }
        public IStringLocalizer StringLocalizer { get { return _stringLocalizer; } }
    }
}
