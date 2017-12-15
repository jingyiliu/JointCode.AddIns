//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Core;
using JointCode.AddIns.Loading;

namespace JointCode.AddIns
{
    public class AddinConfiguration
    {
    	readonly IExtensionBuilderFactory _ebFactory;
        readonly IExtensionPointFactory _epFactory;
        readonly IMessageDialog _messageDialog;
        readonly FileConfiguration _fileConfig;
        readonly INameConvention _nameConvention;

        public AddinConfiguration()
            : this(new ConsoleMessageDialog(),
            new FileConfiguration(),
            new DefaultNameConvention(),
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(IMessageDialog dialog)
            : this(dialog,
            new FileConfiguration(),
            new DefaultNameConvention(),
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(FileConfiguration fileConfig)
            : this(new ConsoleMessageDialog(), fileConfig, new DefaultNameConvention(),
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(IMessageDialog dialog, FileConfiguration fileConfig)
            : this(dialog, fileConfig, new DefaultNameConvention(),
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(INameConvention nameConvention)
            : this(new ConsoleMessageDialog(), 
            new FileConfiguration(),
            nameConvention, 
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(IMessageDialog dialog, INameConvention nameConvention)
            : this(dialog, 
            new FileConfiguration(),
            nameConvention, 
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(FileConfiguration fileConfig, INameConvention nameConvention)
            : this(new ConsoleMessageDialog(), fileConfig, nameConvention, 
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

        public AddinConfiguration(IMessageDialog dialog, FileConfiguration fileConfig, INameConvention nameConvention)
            : this(dialog, fileConfig, nameConvention, 
            new ReflectionExtensionBuilderFactory(), new ReflectionExtensionPointFactory()) { }

    	public AddinConfiguration(IMessageDialog dialog, FileConfiguration fileConfig, INameConvention nameConvention,
            IExtensionBuilderFactory extensionBuilderFactory, IExtensionPointFactory extensionPointFactory)
    	{
    	    _messageDialog = dialog;
    	    _fileConfig = fileConfig;
    	    _nameConvention = nameConvention;
    	    _ebFactory = extensionBuilderFactory;
    	    _epFactory = extensionPointFactory;
    	}

    	public IMessageDialog MessageDialog { get { return _messageDialog; } }
        public FileConfiguration FileConfiguration { get { return _fileConfig; } }
        public INameConvention NameConvention { get { return _nameConvention; } }
        public IExtensionBuilderFactory ExtensionBuilderFactory { get { return _ebFactory; } }
        public IExtensionPointFactory ExtensionPointFactory { get { return _epFactory; } }
    }
}
