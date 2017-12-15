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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Runtime;
using JointCode.Common.Conversion;
using JointCode.Common.Helpers;
using Mono.Cecil;

namespace JointCode.AddIns.Resolving.Assets
{
    enum AssemblyKind
    {
        Default,
        // assemblies provided by runtime (.net/mono) or the application itself.
        ProbableAssembly,
        AddinAssembly
    }

    partial class AssemblyResolution
    {
        class CecilAssemblyKey : AssemblyKey
        {
            private CecilAssemblyKey(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken)
                : base(name, version, cultrue, publicKeyToken) { }
            internal static CecilAssemblyKey Create(AssemblyNameDefinition assemblyName)
            {
                return new CecilAssemblyKey(assemblyName.Name, assemblyName.Version, new CultureInfo(assemblyName.Culture), assemblyName.PublicKeyToken);
            }
            internal static CecilAssemblyKey Create(AssemblyNameReference assemblyName)
            {
                return new CecilAssemblyKey(assemblyName.Name, assemblyName.Version, new CultureInfo(assemblyName.Culture), assemblyName.PublicKeyToken);
            }
        }

        #region 0.9.10
        //using System;
        //using System.Collections.Generic;
        //using System.IO;
        //using System.Text;
        //using Microsoft.Win32;
        //using Mono.Cecil;

        //namespace JointCode.AddIns.Resolving.Assets
        //{
        //    public abstract class BaseAssemblyResolver : IAssemblyResolver
        //    {
        //        static readonly bool on_mono = Type.GetType("Mono.Runtime") != null;

        //        readonly List<string> directories;

        //        List<string> gac_paths;

        //        public void AddSearchDirectory(string directory)
        //        {
        //            directories.Add(directory);
        //        }

        //        public void RemoveSearchDirectory(string directory)
        //        {
        //            directories.Remove(directory);
        //        }

        //        public string[] GetSearchDirectories()
        //        {
        //            var directories = new string[this.directories.size];
        //            Array.Copy(this.directories.items, directories, directories.Length);
        //            return directories;
        //        }

        //        public event AssemblyResolveEventHandler ResolveFailure;

        //        protected BaseAssemblyResolver()
        //        {
        //            directories = new List<string>(2) { ".", "bin" };
        //        }

        //        AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
        //        {
        //            if (parameters.AssemblyResolver == null)
        //                parameters.AssemblyResolver = this;

        //            return ModuleDefinition.ReadModule(file, parameters).Assembly;
        //        }

        //        public virtual AssemblyDefinition Resolve(AssemblyNameReference name)
        //        {
        //            return Resolve(name, new ReaderParameters());
        //        }

        //        public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        //        {
        //            Mixin.CheckName(name);
        //            Mixin.CheckParameters(parameters);

        //            var assembly = SearchDirectory(name, directories, parameters);
        //            if (assembly != null)
        //                return assembly;

        //            if (name.IsRetargetable)
        //            {
        //                // if the reference is retargetable, zero it
        //                name = new AssemblyNameReference(name.Name, Mixin.ZeroVersion)
        //                {
        //                    PublicKeyToken = Empty<byte>.Array,
        //                };
        //            }

        //            var framework_dir = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);

        //            if (IsZero(name.Version))
        //            {
        //                assembly = SearchDirectory(name, new[] { framework_dir }, parameters);
        //                if (assembly != null)
        //                    return assembly;
        //            }

        //            if (name.Name == "mscorlib")
        //            {
        //                assembly = GetCorlib(name, parameters);
        //                if (assembly != null)
        //                    return assembly;
        //            }

        //            assembly = GetAssemblyInGac(name, parameters);
        //            if (assembly != null)
        //                return assembly;

        //            assembly = SearchDirectory(name, new[] { framework_dir }, parameters);
        //            if (assembly != null)
        //                return assembly;

        //            if (ResolveFailure != null)
        //            {
        //                assembly = ResolveFailure(this, name);
        //                if (assembly != null)
        //                    return assembly;
        //            }

        //            throw new AssemblyResolutionException(name);
        //        }

        //        AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
        //        {
        //            var extensions = name.IsWindowsRuntime ? new[] { ".winmd", ".dll" } : new[] { ".exe", ".dll" };
        //            foreach (var directory in directories)
        //            {
        //                foreach (var extension in extensions)
        //                {
        //                    string file = Path.Combine(directory, name.Name + extension);
        //                    if (!File.Exists(file))
        //                        continue;
        //                    try
        //                    {
        //                        return GetAssembly(file, parameters);
        //                    }
        //                    catch (System.BadImageFormatException)
        //                    {
        //                        continue;
        //                    }
        //                }
        //            }

        //            return null;
        //        }

        //        static bool IsZero(Version version)
        //        {
        //            return version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0;
        //        }

        //        AssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
        //        {
        //            var version = reference.Version;
        //            var corlib = typeof(object).Assembly.GetName();

        //            if (corlib.Version == version || IsZero(version))
        //                return GetAssembly(typeof(object).Module.FullyQualifiedName, parameters);

        //            var path = Directory.GetParent(
        //                Directory.GetParent(
        //                    typeof(object).Module.FullyQualifiedName).FullName
        //                ).FullName;

        //            if (on_mono)
        //            {
        //                if (version.Major == 1)
        //                    path = Path.Combine(path, "1.0");
        //                else if (version.Major == 2)
        //                {
        //                    if (version.MajorRevision == 5)
        //                        path = Path.Combine(path, "2.1");
        //                    else
        //                        path = Path.Combine(path, "2.0");
        //                }
        //                else if (version.Major == 4)
        //                    path = Path.Combine(path, "4.0");
        //                else
        //                    throw new NotSupportedException("Version not supported: " + version);
        //            }
        //            else
        //            {
        //                switch (version.Major)
        //                {
        //                    case 1:
        //                        if (version.MajorRevision == 3300)
        //                            path = Path.Combine(path, "v1.0.3705");
        //                        else
        //                            path = Path.Combine(path, "v1.0.5000.0");
        //                        break;
        //                    case 2:
        //                        path = Path.Combine(path, "v2.0.50727");
        //                        break;
        //                    case 4:
        //                        path = Path.Combine(path, "v4.0.30319");
        //                        break;
        //                    default:
        //                        throw new NotSupportedException("Version not supported: " + version);
        //                }
        //            }

        //            var file = Path.Combine(path, "mscorlib.dll");
        //            if (File.Exists(file))
        //                return GetAssembly(file, parameters);

        //            return null;
        //        }

        //        static List<string> GetGacPaths()
        //        {
        //            if (on_mono)
        //                return GetDefaultMonoGacPaths();

        //            var paths = new List<string>(2);
        //            var windir = Environment.GetEnvironmentVariable("WINDIR");
        //            if (windir == null)
        //                return paths;

        //            paths.Add(Path.Combine(windir, "assembly"));
        //            paths.Add(Path.Combine(windir, Path.Combine("Microsoft.NET", "assembly")));
        //            return paths;
        //        }

        //        static List<string> GetDefaultMonoGacPaths()
        //        {
        //            var paths = new List<string>(1);
        //            var gac = GetCurrentMonoGac();
        //            if (gac != null)
        //                paths.Add(gac);

        //            var gac_paths_env = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
        //            if (string.IsNullOrEmpty(gac_paths_env))
        //                return paths;

        //            var prefixes = gac_paths_env.Split(Path.PathSeparator);
        //            foreach (var prefix in prefixes)
        //            {
        //                if (string.IsNullOrEmpty(prefix))
        //                    continue;

        //                var gac_path = Path.Combine(Path.Combine(Path.Combine(prefix, "lib"), "mono"), "gac");
        //                if (Directory.Exists(gac_path) && !paths.Contains(gac))
        //                    paths.Add(gac_path);
        //            }

        //            return paths;
        //        }

        //        static string GetCurrentMonoGac()
        //        {
        //            return Path.Combine(
        //                Directory.GetParent(
        //                    Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName)).FullName,
        //                "gac");
        //        }

        //        AssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
        //        {
        //            if (reference.PublicKeyToken == null || reference.PublicKeyToken.Length == 0)
        //                return null;

        //            if (gac_paths == null)
        //                gac_paths = GetGacPaths();

        //            if (on_mono)
        //                return GetAssemblyInMonoGac(reference, parameters);

        //            return GetAssemblyInNetGac(reference, parameters);
        //        }

        //        AssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
        //        {
        //            for (int i = 0; i < gac_paths.Count; i++)
        //            {
        //                var gac_path = gac_paths[i];
        //                var file = GetAssemblyFile(reference, string.Empty, gac_path);
        //                if (File.Exists(file))
        //                    return GetAssembly(file, parameters);
        //            }

        //            return null;
        //        }

        //        AssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
        //        {
        //            var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC_64", "GAC" };
        //            var prefixes = new[] { string.Empty, "v4.0_" };

        //            for (int i = 0; i < 2; i++)
        //            {
        //                for (int j = 0; j < gacs.Length; j++)
        //                {
        //                    var gac = Path.Combine(gac_paths[i], gacs[j]);
        //                    var file = GetAssemblyFile(reference, prefixes[i], gac);
        //                    if (Directory.Exists(gac) && File.Exists(file))
        //                        return GetAssembly(file, parameters);
        //                }
        //            }

        //            return null;
        //        }

        //        static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
        //        {
        //            var gac_folder = new StringBuilder()
        //                .Append(prefix)
        //                .Append(reference.Version)
        //                .Append("__");

        //            for (int i = 0; i < reference.PublicKeyToken.Length; i++)
        //                gac_folder.Append(reference.PublicKeyToken[i].ToString("x2"));

        //            return Path.Combine(
        //                Path.Combine(
        //                    Path.Combine(gac, reference.Name), gac_folder.ToString()),
        //                reference.Name + ".dll");
        //        }

        //        public void Dispose()
        //        {
        //            Dispose(true);
        //            GC.SuppressFinalize(this);
        //        }

        //        protected virtual void Dispose(bool disposing)
        //        {
        //        }
        //    }

        //    public class DefaultAssemblyResolver : BaseAssemblyResolver
        //    {

        //        readonly IDictionary<string, AssemblyDefinition> cache;

        //        public DefaultAssemblyResolver()
        //        {
        //            cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);
        //        }

        //        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        //        {
        //            Mixin.CheckName(name);

        //            AssemblyDefinition assembly;
        //            if (cache.TryGetValue(name.FullName, out assembly))
        //                return assembly;

        //            assembly = base.Resolve(name);
        //            cache[name.FullName] = assembly;

        //            return assembly;
        //        }

        //        protected void RegisterAssembly(AssemblyDefinition assembly)
        //        {
        //            if (assembly == null)
        //                throw new ArgumentNullException("assembly");

        //            var name = assembly.Name.FullName;
        //            if (cache.ContainsKey(name))
        //                return;

        //            cache[name] = assembly;
        //        }

        //        protected override void Dispose(bool disposing)
        //        {
        //            foreach (var assembly in cache.Values)
        //                assembly.Dispose();

        //            cache.Clear();

        //            base.Dispose(disposing);
        //        }
        //    }

        //    public class WindowsRuntimeAssemblyResolver : DefaultAssemblyResolver
        //    {
        //        readonly Dictionary<string, AssemblyDefinition> assemblies = new Dictionary<string, AssemblyDefinition>();

        //        public static WindowsRuntimeAssemblyResolver CreateInstance()
        //        {
        //            if (Platform.OnMono)
        //                return null;
        //            try
        //            {
        //                return new WindowsRuntimeAssemblyResolver();
        //            }
        //            catch
        //            {
        //                return null;
        //            }
        //        }

        //        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        //        {
        //            AssemblyDefinition assembly;
        //            if (assemblies.TryGetValue(name.Name, out assembly))
        //                return assembly;

        //            return base.Resolve(name);
        //        }

        //        private WindowsRuntimeAssemblyResolver()
        //        {
        //            LoadWindowsSdk("v8.1", "8.1", (installationFolder) =>
        //            {
        //                var fileName = Path.Combine(installationFolder, @"References\CommonConfiguration\Neutral\Annotated\Windows.winmd");
        //                var assembly = AssemblyDefinition.ReadAssembly(fileName);
        //                Register(assembly);
        //            });

        //            LoadWindowsSdk("v10.0", "10", (installationFolder) =>
        //            {
        //                var referencesFolder = Path.Combine(installationFolder, "References");
        //                var assemblies = Directory.GetFiles(referencesFolder, "*.winmd", SearchOption.AllDirectories);

        //                foreach (var assemblyPath in assemblies)
        //                {
        //                    var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        //                    Register(assembly);
        //                }
        //            });
        //        }

        //        void Register(AssemblyDefinition assembly)
        //        {
        //            assemblies[assembly.Name.Name] = assembly;
        //            RegisterAssembly(assembly);
        //        }

        //        protected override void Dispose(bool disposing)
        //        {
        //            if (!disposing)
        //                return;

        //            foreach (var assembly in assemblies.Values)
        //                assembly.Dispose();

        //            base.Dispose(true);
        //        }

        //        void LoadWindowsSdk(string registryVersion, string windowsKitsVersion, Action<string> registerAssembliesCallback)
        //        {
        //#if NET_4_0
        //            using (var localMachine32Key = RegistryKey.OpenBaseKey (RegistryHive.LocalMachine, RegistryView.Registry32)) {
        //                using (var sdkKey = localMachine32Key.OpenSubKey (@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v" + registryVersion)) {
        //#else
        //            {
        //                // this will fail on 64-bit process as there's no way (other than pinoke) to read from 32-bit registry view
        //                using (var sdkKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\" + registryVersion))
        //                {
        //#endif
        //                    string installationFolder = null;
        //                    if (sdkKey != null)
        //                        installationFolder = (string)sdkKey.GetValue("InstallationFolder");
        //                    if (string.IsNullOrEmpty(installationFolder))
        //                    {
        //#if NET_4_0
        //                        var programFilesX86 = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
        //#else
        //                        var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        //#endif
        //                        installationFolder = Path.Combine(programFilesX86, @"Windows Kits\" + windowsKitsVersion);
        //                    }
        //                    registerAssembliesCallback(installationFolder);
        //                }
        //            }
        //        }
        //    }
        //} 
        #endregion

        abstract class BaseAssemblyResolver : IAssemblyResolver
        {
            static readonly bool _onMono = Type.GetType("Mono.Runtime") != null;

            readonly List<string> _directories;

#if !SILVERLIGHT && !CF
            List<string> _gacPaths;
#endif

            protected BaseAssemblyResolver()
            {
                _directories = new List<string>(2) { ".", "bin" };
            }

            public event AssemblyResolveEventHandler ResolveFailure;

            internal void AddSearchDirectory(string directory)
            {
                _directories.Add(directory);
            }

            internal void RemoveSearchDirectory(string directory)
            {
                _directories.Remove(directory);
            }

            internal List<string> GetSearchDirectories()
            {
                return new List<string>(_directories);
            }

            public virtual AssemblyDefinition Resolve(string fullName)
            {
                return Resolve(fullName, new ReaderParameters());
            }

            public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                if (fullName == null)
                    throw new ArgumentNullException("fullName");
                return Resolve(AssemblyNameReference.Parse(fullName), parameters);
            }

            AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
            {
                if (parameters.AssemblyResolver == null)
                    parameters.AssemblyResolver = this;
                return ModuleDefinition.ReadModule(file, parameters).Assembly;
            }

            public virtual AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, new ReaderParameters());
            }

            public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                return DoResolve(name, parameters, true);
            }

            protected AssemblyDefinition DoResolve(AssemblyNameReference name, ReaderParameters parameters, bool fastFail)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (parameters == null)
                    parameters = new ReaderParameters();

                var assembly = SearchDirectory(name, _directories, parameters);
                if (assembly != null)
                    return assembly;

#if !SILVERLIGHT && !CF
                if (name.IsRetargetable)
                {
                    // if the reference is retargetable, zero it
                    name = new AssemblyNameReference(name.Name, new Version(0, 0, 0, 0))
                    {
                        PublicKeyToken = SysConstants.EmptyBytes
                    };
                }

                var frameworkDir = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);

                if (IsZero(name.Version))
                {
                    assembly = SearchDirectory(name, new[] { frameworkDir }, parameters);
                    if (assembly != null)
                        return assembly;
                }

                if (name.Name == "mscorlib")
                {
                    assembly = GetCorlib(name, parameters);
                    if (assembly != null)
                        return assembly;
                }

                assembly = GetAssemblyInGac(name, parameters);
                if (assembly != null)
                    return assembly;

                assembly = SearchDirectory(name, new[] { frameworkDir }, parameters);
                if (assembly != null)
                    return assembly;
#endif

                if (ResolveFailure != null)
                {
                    assembly = ResolveFailure(this, name);
                    if (assembly != null)
                        return assembly;
                }

                if (fastFail)
                    throw new AssemblyResolutionException(name);
                return null;
            }

            AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
            {
                var extensions = new[] { ".exe", ".dll" };
                foreach (var directory in directories)
                {
                    foreach (var extension in extensions)
                    {
                        string file = Path.Combine(directory, name.Name + extension);
                        if (File.Exists(file))
                            return GetAssembly(file, parameters);
                    }
                }

                return null;
            }

            static bool IsZero(Version version)
            {
                return version == null || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0);
            }

#if !SILVERLIGHT && !CF
            AssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
            {
                var version = reference.Version;
                var corlib = typeof(object).Assembly.GetName();

                if (corlib.Version == version || IsZero(version))
                    return GetAssembly(typeof(object).Module.FullyQualifiedName, parameters);

                var path = Directory.GetParent(
                    Directory.GetParent(
                        typeof(object).Module.FullyQualifiedName).FullName
                    ).FullName;

                if (_onMono)
                {
                    if (version.Major == 1)
                        path = Path.Combine(path, "1.0");
                    else if (version.Major == 2)
                    {
                        if (version.MajorRevision == 5)
                            path = Path.Combine(path, "2.1");
                        else
                            path = Path.Combine(path, "2.0");
                    }
                    else if (version.Major == 4)
                        path = Path.Combine(path, "4.0");
                    else
                        throw new NotSupportedException("Version not supported: " + version);
                }
                else
                {
                    switch (version.Major)
                    {
                        case 1:
                            if (version.MajorRevision == 3300)
                                path = Path.Combine(path, "v1.0.3705");
                            else
                                path = Path.Combine(path, "v1.0.5000.0");
                            break;
                        case 2:
                            path = Path.Combine(path, "v2.0.50727");
                            break;
                        case 4:
                            path = Path.Combine(path, "v4.0.30319");
                            break;
                        default:
                            throw new NotSupportedException("Version not supported: " + version);
                    }
                }

                var file = Path.Combine(path, "mscorlib.dll");
                if (File.Exists(file))
                    return GetAssembly(file, parameters);

                return null;
            }

            static List<string> GetGacPaths()
            {
                if (_onMono)
                    return GetDefaultMonoGacPaths();

                var paths = new List<string>(2);
                var windir = Environment.GetEnvironmentVariable("WINDIR");
                if (windir == null)
                    return paths;

                paths.Add(Path.Combine(windir, "assembly"));
                paths.Add(Path.Combine(windir, Path.Combine("Microsoft.NET", "assembly")));
                return paths;
            }

            static List<string> GetDefaultMonoGacPaths()
            {
                var paths = new List<string>(1);
                var gac = GetCurrentMonoGac();
                if (gac != null)
                    paths.Add(gac);

                var gac_paths_env = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
                if (string.IsNullOrEmpty(gac_paths_env))
                    return paths;

                var prefixes = gac_paths_env.Split(Path.PathSeparator);
                foreach (var prefix in prefixes)
                {
                    if (string.IsNullOrEmpty(prefix))
                        continue;

                    var gac_path = Path.Combine(Path.Combine(Path.Combine(prefix, "lib"), "mono"), "gac");
                    if (Directory.Exists(gac_path) && !paths.Contains(gac))
                        paths.Add(gac_path);
                }

                return paths;
            }

            static string GetCurrentMonoGac()
            {
                return Path.Combine(
                    Directory.GetParent(
                        Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName)).FullName,
                    "gac");
            }

            AssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                if (reference.PublicKeyToken == null || reference.PublicKeyToken.Length == 0)
                    return null;

                if (_gacPaths == null)
                    _gacPaths = GetGacPaths();

                if (_onMono)
                    return GetAssemblyInMonoGac(reference, parameters);

                return GetAssemblyInNetGac(reference, parameters);
            }

            AssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                for (int i = 0; i < _gacPaths.Count; i++)
                {
                    var gac_path = _gacPaths[i];
                    var file = GetAssemblyFile(reference, string.Empty, gac_path);
                    if (File.Exists(file))
                        return GetAssembly(file, parameters);
                }

                return null;
            }

            AssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
            {
                var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC_64", "GAC" };
                var prefixes = new[] { string.Empty, "v4.0_" };

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < gacs.Length; j++)
                    {
                        var gac = Path.Combine(_gacPaths[i], gacs[j]);
                        var file = GetAssemblyFile(reference, prefixes[i], gac);
                        if (Directory.Exists(gac) && File.Exists(file))
                            return GetAssembly(file, parameters);
                    }
                }

                return null;
            }

            static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
            {
                var gac_folder = new StringBuilder()
                    .Append(prefix)
                    .Append(reference.Version)
                    .Append("__");

                for (int i = 0; i < reference.PublicKeyToken.Length; i++)
                    gac_folder.Append(reference.PublicKeyToken[i].ToString("x2"));

                return Path.Combine(
                    Path.Combine(
                        Path.Combine(gac, reference.Name), gac_folder.ToString()),
                    reference.Name + ".dll");
            }
#endif
        }

        class ResolveTimeAssemblyResolver : BaseAssemblyResolver
        {
            readonly Dictionary<string, AssemblyDefinition> _cache;

            internal ResolveTimeAssemblyResolver()
            {
                _cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);
            }

            public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                AssemblyDefinition assembly;
                if (_cache.TryGetValue(fullName, out assembly))
                    return assembly;

                assembly = base.Resolve(fullName, parameters);
                if (assembly != null)
                    _cache[fullName] = assembly;

                return assembly;
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                AssemblyDefinition assembly;
                if (_cache.TryGetValue(name.FullName, out assembly))
                    return assembly;

                assembly = DoResolve(name, parameters, false);
                if (assembly != null)
                    _cache[name.FullName] = assembly;

                return assembly;
            }
        }

        internal static AssemblyResolution CreateAddinAssembly(AddinResolution addin, AssemblyFileResolution assemblyFile)
        {
            return new AssemblyResolution(addin, assemblyFile);
        }

        internal static AssemblyResolution CreateProbableAssembly(AssemblyDefinition assemblyDef)
        {
            return new AssemblyResolution(assemblyDef);
        }

        internal static AssemblyResolution GetProbableAssembly(string assemblyName)
        {
            var assemblyDef = _cecilReaderParameters.AssemblyResolver.Resolve(assemblyName);
            return assemblyDef != null ? new AssemblyResolution(assemblyDef) : null;
        }
    }

    /// <summary>
    /// Represent an assembly during resolution.
    /// Notes that to determine whether two <see cref="AssemblyResolution"/>s is the same, you can not use <see cref="object.ReferenceEquals"/>,
    /// try to use the <see cref="AssemblyResolution.Equals"/> instead.
    /// </summary>
    partial class AssemblyResolution : Resolvable, IEquatable<AssemblyResolution>
    {
        internal static readonly AssemblyDefinition ThisAssembly;
        static ReaderParameters _cecilReaderParameters;

        AssemblyDefinition _assemblyDef;
        CecilAssemblyKey _assemblykey;

        readonly AssemblyFileResolution _assemblyFile;
        AssemblyKind _assemblyKind;
        bool _assemblyReferencesGot;
        Assembly _assembly;

        static AssemblyResolution()
        {
            var assemblyResolver = new ResolveTimeAssemblyResolver();
            _cecilReaderParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            var runtimeAssemblyFullName = typeof (AssemblyResolution).Assembly.FullName;
            var cecilAssemblyName = AssemblyNameReference.Parse(runtimeAssemblyFullName);
            ThisAssembly = assemblyResolver.Resolve(cecilAssemblyName);
            //ThisAssembly = AssemblyDefinition.ReadAssembly(typeof(AssemblyResolution).Assembly.Location, _cecilReaderParameters);
        }

        private AssemblyResolution(AddinResolution declaringAddin, AssemblyFileResolution assemblyFile)
            : base(declaringAddin)
        {
            _assemblyKind = AssemblyKind.AddinAssembly;
            _assemblyFile = assemblyFile;
        }

        private AssemblyResolution(AssemblyDefinition assemblyDef)
            : base(null)
        {
            _assemblyKind = AssemblyKind.ProbableAssembly;
            _assemblyDef = assemblyDef;
        }

        internal static ReaderParameters CecilReaderParameters { get { return _cecilReaderParameters; } }

        internal static void DisposeInternal()
        {
            _cecilReaderParameters = null;
        }

        internal AssemblyKind AssemblyKind { get { return _assemblyKind; } }
        internal AssemblyFileResolution AssemblyFile { get { return _assemblyFile; } }
        internal AssemblyKey AssemblyKey { get { return _assemblykey; } }

        internal int Uid
        {
            get { return _assemblyFile.Uid; } 
            set { _assemblyFile.Uid = value; }
        }

        internal bool TryLoad()
        {
            if (_assemblyDef != null)
                return true;

            AssemblyNameDefinition assemblyName;
            var assemblyPath = GetAbsolutePath();
            try
            {
                _assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath, _cecilReaderParameters);
                assemblyName = _assemblyDef.Name;
            }
            catch (Exception e)
            {
                // log
                return false;
            }

            _assemblykey = CecilAssemblyKey.Create(assemblyName);

            if (assemblyName.IsProbableAssembly(_cecilReaderParameters))
            {
                _assemblyKind = AssemblyKind.ProbableAssembly;
#if NET_4_0
                _assemblyFile.IsPublic = true;
#endif
            }
#if NET_4_0
            if (_assemblyKind == AssemblyKind.AddinAssembly) 
                _assemblyFile.IsPublic = IsPublicAssembly();
#endif
            return true;
        }

        string GetAbsolutePath()
        {
            if (_assemblyKind == AssemblyKind.AddinAssembly)
                return Path.Combine(DeclaringAddin.ManifestFile.Directory, _assemblyFile.FilePath);
            if (_assemblyKind == AssemblyKind.ProbableAssembly)
                return Path.Combine(SystemHelper.AppDirectory, _assemblyFile.FilePath);
            throw new InvalidOperationException();
        }

#if NET_4_0
        bool IsPublicAssembly()
        {
            if (!_assemblyDef.HasCustomAttributes) 
                return true;
    		
            foreach (var attrib in _assemblyDef.CustomAttributes) 
            {
                if (!attrib.HasProperties)
                    continue;

                TypeDefinition attribType;
                try
                {
                    attribType = attrib.AttributeType.Resolve();
                }
                catch
                {
                    return false;
                }
                if (!ReferenceEquals(attribType, _addinAssemblyAttributeType))
                    continue;
    			
                foreach (var prop in attrib.Properties)
                {
                    if (prop.Name == AddinAssemblyAttribute.PropertyIsPublic) 
                        return (bool)prop.Argument.Value;
                }
            }
    		
            return false;
        }
#endif

        internal bool TryGetType(string typeName, out TypeResolution result)
        {
            foreach (var module in _assemblyDef.Modules)
            {
                if (!module.HasTypes)
                    continue;
                foreach (var type in module.Types)
                {
                    if (type.FullName == typeName)
                    {
                        result = new TypeResolution(this, type);
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        // get assembly references for this assembly that can not be probed by the runtime
        internal List<AssemblyKey> GetRequiredAssemblyReferences()
        {
            List<AssemblyKey> result = null;
            foreach (var module in _assemblyDef.Modules)
            {
                if (!module.HasAssemblyReferences)
                    continue;
                foreach (var assemblyReference in module.AssemblyReferences)
                {
                    // if the referenced assembly is a runtime or application assembly that can be loaded by probing mechanism of runtime, 
                    // then the return value is not null; otherwise, returns null.
                    var applicationOrRuntimeAssembly = 
                        _cecilReaderParameters.AssemblyResolver.Resolve(assemblyReference, _cecilReaderParameters);
                    if (applicationOrRuntimeAssembly != null)
                        continue;
                    var assemblyKey = CecilAssemblyKey.Create(assemblyReference);
                    result = result ?? new List<AssemblyKey>();
                    result.Add(assemblyKey);
                }
            }
            return result;
        }

        #region IEquatable implementation
        public bool Equals(AssemblyResolution other)
        {
            return other != null && ReferenceEquals(_assemblyDef, other._assemblyDef);
        }
        #endregion

        internal override ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            if (_assemblyReferencesGot)
                return DeclaringAddin == null ? ResolutionStatus.Success : DeclaringAddin.ResolutionStatus;

            List<AssemblyResolutionSet> refAsmSets;
            if (!ctx.TryGetRequiredAssemblyReferences(dialog, this, out refAsmSets))
                return ResolutionStatus.Failed;
            _assemblyReferencesGot = true;
            if (refAsmSets != null)
            {
                foreach (var refAsmSet in refAsmSets)
                    DeclaringAddin.AddReferencedAssemblySet(refAsmSet);
            }
            return DeclaringAddin == null ? ResolutionStatus.Success : DeclaringAddin.ResolutionStatus;
        }

        internal Assembly GetRuntimeAssembly()
        {
            if (_assembly != null)
                return _assembly;
            _assembly = Assembly.LoadFile(GetAbsolutePath());
            return _assembly;
        }
    }

    // 在一个应用程序中，相同程序集（版本也相同）只允许加载一次。
    // 因此，如果有插件 a 依赖于某程序集（例如 ICSharpCode.SharpZipLib.dll），即使有多个插件都提供了该程序集，
    // 也只需加载其中任意一个插件即可满足插件 a 的需求。
    class AssemblyResolutionSet : List<AssemblyResolution>
    {
        internal int Uid { get { return this[0].Uid; } }

        // get the lowest version of an assembly set
        internal Version Version
        {
            get
            { 
                Version ver = null;
                foreach (var item in this)
                {
                    if (ver == null || ver > item.AssemblyKey.Version)
                        ver = item.AssemblyKey.Version;
                }
                return ver;
            }
        }

        internal ResolutionStatus Resolve(IMessageDialog dialog, ConvertionManager convertionManager, ResolutionContext ctx)
        {
            var result = ResolutionStatus.Success;
            for (int i = 0; i < Count; i++)
            {
                var status = this[i].Resolve(dialog, convertionManager, ctx);
                if (status == ResolutionStatus.Success)
                    return ResolutionStatus.Success;
                result |= status;
            }
            return result;
        }
    }
}