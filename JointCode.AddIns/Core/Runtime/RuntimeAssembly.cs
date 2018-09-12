//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Metadata.Assets;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace JointCode.AddIns.Core.Runtime
{
    abstract class RuntimeAssembly
    {
        //Version _compatVersion;
        protected Assembly _assembly;
        readonly AssemblyKey _assemblyKey;

        protected RuntimeAssembly(AssemblyKey assemblyKey)
        {
            _assemblyKey = assemblyKey;
        }

        internal Assembly Assembly
        {
            get { return _assembly; }
        }

        internal abstract int Uid { get; }

        internal AssemblyKey AssemblyKey
        {
            get { return _assemblyKey; }
        }

        internal string Name
        {
            get { return _assemblyKey.Name; }
        }

        internal Version Version
        {
            get { return _assemblyKey.Version; }
        }

        internal CultureInfo CultureInfo
        {
            get { return _assemblyKey.CultureInfo; }
        }

        internal byte[] PublicKeyToken
        {
            get { return _assemblyKey.PublicKeyToken; }
        }

        internal abstract AssemblyFileRecord AssemblyFile { get; set; }

        internal abstract Assembly LoadAssembly(AssemblyLoadPolicy assemblyLoadPolicy);
    }

    class AddinRuntimeAssembly : RuntimeAssembly
    {
        readonly Addin _hostingAddin;
        AssemblyFileRecord _assemblyFile;

        internal AddinRuntimeAssembly(Addin hostingAddin, AssemblyKey assemblyKey, AssemblyFileRecord assemblyFile) : base(assemblyKey)
        {
            _hostingAddin = hostingAddin;
            _assemblyFile = assemblyFile;
        }

        internal override int Uid
        {
            get { return _assemblyFile.Uid; }
        }

        //internal string FullPath
        //{
        //    get { return _assemblyFile.FullPath; }
        //}

        //internal bool Loaded
        //{
        //    get { return _assemblyFile.Loaded; }
        //    set { _assemblyFile.Loaded = value; }
        //}

        internal override AssemblyFileRecord AssemblyFile
        {
            get { return _assemblyFile; }
            set { _assemblyFile = value; }
        }

        #region Assembly.Load、LoadFrom 与 LoadFile 区别
        // 1. Load()方法接收一个String或AssemblyName类型作为参数，这个参数实际上是需要加载的程序集的强名称（名称，版本，语言，公钥标记）。
        //    例如.NET 2.0中的FileIOPermission类，它的强名称是：
        //    System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
        //    对于弱命名的程序集，则只会有程序集名称，而不会有版本，语言和公钥标记，例如 TestClassLibrary。
        // 细节
        // 1.1 CLR内部普遍使用了Load()方法来加载程序集，在Load()方法的内部，CLR首先会应用这个程序集的版本绑定重定向策略，接着在GAC中查找目标程序集。
        //     如果GAC中没有找到，则会在应用程序目录和子目录中寻找（应用配置文件的codebase所指定的位置）。
        // 1.2 如果希望加载弱命名程序集，Load()方法就不会去GAC中查找。
        // 1.3 当Load()找到目标程序集时，就会加载它，并返回一个相应Assembly对象的引用。
        // 1.4 当没有找到程序集时，会抛出System.IO.FileNotFoundException异常。
        // 1.5 当存在特定CPU架构的程序集时，CLR会优先加载当前架构的程序集(例如x86版本优先于IL中立版本)
        // 1.6 如果希望强迫加载某个架构版本的程序集，需要在强名称中加以指定。ProcessorArchitecture可以为x86 IA64 AMD64或MSIL，当然还有None
        // 1.7 Load方法与Win32函数中的LoadLibrary方法等价
        // 2. LoadFrom()方法可以从指定文件中加载程序集，通过查找程序集的AssemblyRef元数据表，得知所有引用和需要的程序集，然后在内部调用Load()方法进行加载。
        // 细节
        // 2.1 LoadFrom()首先会打开程序集文件，通过GetAssemblyName方法得到程序集名称，然后关闭文件，最后将得到的AssemblyName对象传入Load()方法中
        // 2.2 随后，Load()方法会再次打开这个文件进行加载。所以，LoadFrom()加载一个程序集时，会多次打开文件，造成了效率低下的现象（与Load相比）。
        // 2.3 由于内部调用了Load()，所以LoadFrom()方法还是会应用版本绑定重定向策略，也会在GAC和各个指定位置中进行查找。
        // 2.4 LoadFrom()会直接返回Load()的结果——一个Assembly对象的引用。
        // 2.5 如果目标程序集已经加载过，LoadFrom()不会重新进行加载。
        // 2.6 LoadFrom支持从一个URL加载程序集(如"http://www.abc.com/test.dll")，这个程序集会被下载到用户缓存文件夹中。
        // 2.7 从URL加载程序集时，如果计算机未联网，LoadFrom会抛出一个异常。如果IE被设置为“脱机工作”，则不会抛出异常，转而从缓存中寻找已下载的文件。
        // 3. LoadFile()从一个指定文件中加载程序集，它和LoadFrom()的不同之处在于LoadFile()不会加载目标程序集所引用和依赖的其他程序集。您需要自己控制并显示
        //    加载所有依赖的程序集。
        // 细节
        // 3.1 LoadFile()不会解析任何依赖
        // 3.2  LoadFile()可以多次加载同一程序集
        // 3.3  显式加载依赖程序集的方法是，注册AppDomain的AssemblyResolve事件 
        #endregion
        internal override Assembly LoadAssembly(AssemblyLoadPolicy assemblyLoadPolicy)
        {
            if (_assembly != null)
                return _assembly;

            // if the addin has not started, start it first.
            if (!_hostingAddin.Started)
                _hostingAddin.Start();

            var loadMethod = assemblyLoadPolicy.GetAssemblyLoadMethod(_hostingAddin);

            switch (loadMethod)
            {
                case AssemblyLoadMethod.Load:
                    var assemblyName = AssemblyName.GetAssemblyName(_assemblyFile.LoadPath);
                    _assembly = Assembly.Load(assemblyName);
                    break;
                case AssemblyLoadMethod.LoadFrom:
                    _assembly = Assembly.LoadFrom(_assemblyFile.LoadPath);
                    break;
                case AssemblyLoadMethod.LoadFile:
                    _assembly = Assembly.LoadFile(_assemblyFile.LoadPath);
                    break;
                case AssemblyLoadMethod.LoadBytes:
                    // 使用 Assembly.Load(byte[]) 重载，可以在加载程序集的同时，避免程序集被锁定，因此我们可以在运行时方便地替换和删除该程序集，
                    // 而不会影响应用程序运行。另外，这种替换是原地替换，也意味着我们如果要升级或更新程序集，不必将程序集拷贝到一个阴影文件夹。
                    var asmBytes = File.ReadAllBytes(_assemblyFile.LoadPath);
                    _assembly = Assembly.Load(asmBytes);
                    break;
                default:
                    _assembly = Assembly.LoadFrom(_assemblyFile.LoadPath);
                    break;
            }

            return _assembly;
        }
    }

    class AppRuntimeAssembly : RuntimeAssembly
    {
        internal AppRuntimeAssembly(AssemblyKey assemblyKey, Assembly assembly) : base(assemblyKey)
        {
            _assembly = assembly;
        }

        internal override int Uid { get { return int.MinValue; } }
        internal override AssemblyFileRecord AssemblyFile { get { return null; } set { } }
        internal override Assembly LoadAssembly(AssemblyLoadPolicy assemblyLoadPolicy)
        {
            return _assembly;
        }
    }
}
