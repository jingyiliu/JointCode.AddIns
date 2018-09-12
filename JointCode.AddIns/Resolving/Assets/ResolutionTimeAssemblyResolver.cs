using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JointCode.AddIns.Core;
using JointCode.Common.Helpers;
using Mono.Cecil;

namespace JointCode.AddIns.Resolving.Assets
{
    // Mono.Cecil 的 AssemblyDefinition 的装饰类，增加了一个 IsRuntimeProvided 属性，因为
    // 我们认为运行时 (.net、mono 等) 提供的程序集（默认为 Gac 程序集）是稳定和持续存在的，
    // 因此在解析插件的程序集引用时不必考虑它们，也不必将插件程序集对于这些程序集的引用写
    // 入到持久化文件中。
    class CompositeAssemblyDefinition
    {
        internal AssemblyDefinition AssemblyDefinition { get; set; }
        internal bool IsRuntimeProvided { get; set; }
    }

    abstract partial class BaseAssemblyResolver
    {
        struct RuntimeVersion : IEquatable<RuntimeVersion>, IEqualityComparer<RuntimeVersion>
        {
            internal Platform Platform { get; set; }
            internal AssemblyVersion Version { get; set; }

            public bool Equals(RuntimeVersion other)
            {
                RuntimeVersion obj;
                try
                {
                    obj = (RuntimeVersion)other;
                }
                catch
                {
                    return false;
                }
                return Platform == obj.Platform
                    && Version.Major == obj.Version.Major
                    && Version.Minor == obj.Version.Minor
                    && Version.Build == obj.Version.Build
                    && Version.Build == obj.Version.Build
                    && Version.MajorRevision == obj.Version.MajorRevision
                    && Version.MinorRevision == obj.Version.MinorRevision;
            }

            public bool Equals(RuntimeVersion x, RuntimeVersion y)
            {
                return x.Platform == y.Platform
                    && x.Version.Major == y.Version.Major
                    && x.Version.Minor == y.Version.Minor
                    && x.Version.Build == y.Version.Build
                    && x.Version.Build == y.Version.Build
                    && x.Version.MajorRevision == y.Version.MajorRevision
                    && x.Version.MinorRevision == y.Version.MinorRevision;
            }

            public int GetHashCode(RuntimeVersion obj)
            {
                var result = obj.Platform.GetHashCode() ^ obj.Version.Major;
                if (obj.Version.Minor > 0)
                    result ^= obj.Version.Minor;
                if (obj.Version.Build > 0)
                    result ^= obj.Version.Build;
                if (obj.Version.Revision > 0)
                    result ^= obj.Version.Revision;
                if (obj.Version.MajorRevision > 0)
                    result ^= obj.Version.MajorRevision;
                if (obj.Version.MinorRevision > 0)
                    result ^= obj.Version.MinorRevision;
                return result;
            }
        }

        readonly Dictionary<RuntimeVersion, string> _runtimeVersion2Directories;
        readonly List<string> _directories;

        void InitializeRuntimeVersionDirectories()
        {
            var rv = new RuntimeVersion { Platform = Platform.Mono, Version = new AssemblyVersion { Major = 1 } };
            _runtimeVersion2Directories.Add(rv, "1.0");

            rv = new RuntimeVersion { Platform = Platform.Mono, Version = new AssemblyVersion { Major = 2 } };
            _runtimeVersion2Directories.Add(rv, "2.0");

            rv = new RuntimeVersion { Platform = Platform.Mono, Version = new AssemblyVersion { Major = 2, MajorRevision = 5 } };
            _runtimeVersion2Directories.Add(rv, "2.1");

            rv = new RuntimeVersion { Platform = Platform.Mono, Version = new AssemblyVersion { Major = 4 } };
            _runtimeVersion2Directories.Add(rv, "4.0");

            rv = new RuntimeVersion { Platform = Platform.Net, Version = new AssemblyVersion { Major = 1 } };
            _runtimeVersion2Directories.Add(rv, "v1.0.5000.0");

            rv = new RuntimeVersion { Platform = Platform.Net, Version = new AssemblyVersion { Major = 1, MajorRevision = 3300 } };
            _runtimeVersion2Directories.Add(rv, "v1.0.3705");

            rv = new RuntimeVersion { Platform = Platform.Net, Version = new AssemblyVersion { Major = 2 } };
            _runtimeVersion2Directories.Add(rv, "v2.0.50727");

            rv = new RuntimeVersion { Platform = Platform.Net, Version = new AssemblyVersion { Major = 4 } };
            _runtimeVersion2Directories.Add(rv, "v4.0.30319");
        }

        internal void AddRuntimeVersion(Platform platform, AssemblyVersion version, string dir)
        {
            var rv = new RuntimeVersion { Version = version, Platform = platform };
            _runtimeVersion2Directories[rv] = dir;
        }

        internal void RemoveRuntimeVersion(Platform platform, AssemblyVersion version)
        {
            var rv = new RuntimeVersion { Version = version, Platform = platform };
            _runtimeVersion2Directories.Remove(rv);
        }

        string GetDirectoryByRuntimeVersion(Platform platform, AssemblyVersion version)
        {
            var rv = new RuntimeVersion { Version = version, Platform = platform };
            string result;
            if (!_runtimeVersion2Directories.TryGetValue(rv, out result))
                throw new NotSupportedException("Version not supported: " + version);
            return result;
        }

        internal void AddSearchDirectory(string directory)
        {
            _directories.Add(GetFullPath(directory));
        }

        internal void RemoveSearchDirectory(string directory)
        {
            _directories.Remove(GetFullPath(directory));
        }

        internal List<string> GetSearchDirectories()
        {
            return new List<string>(_directories);
        }
    }

    abstract partial class BaseAssemblyResolver : IAssemblyResolver
    {
        static readonly bool _onMono = Type.GetType("Mono.Runtime") != null;

#if !SILVERLIGHT && !CF
        List<string> _gacPaths;
#endif

        protected BaseAssemblyResolver()
        {
            //// 使用空的可探测目录，这意味着就算是应用程序自身根目录下的程序集也探测不到，
            //// 只能探测运行时（.net framework、mono 等）提供的程序集
            //_directories = new List<string>(); 

            _directories = new List<string>(2)
                {
                    GetFullPath("."), // .net / mono 等平台的程序集根目录
                    GetFullPath("bin") // Asp.net 等平台的程序集根目录
                };

            _runtimeVersion2Directories = new Dictionary<RuntimeVersion, string>();
            InitializeRuntimeVersionDirectories();
        }

        string GetFullPath(string dir)
        {
            return Path.IsPathRooted(dir) ? dir : Path.Combine(SystemHelper.AppDirectory, dir);
        }

        public event AssemblyResolveEventHandler ResolveFailure;

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

        public virtual AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters());
        }

        public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            //return DoResolve(name, parameters, true);
            throw new NotImplementedException();
        }

        protected CompositeAssemblyDefinition DoResolve(AssemblyNameReference name, ReaderParameters parameters, bool fastFail)
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
                var asm = ResolveFailure(this, name);
                if (asm != null)
                    return new CompositeAssemblyDefinition { AssemblyDefinition = asm, IsRuntimeProvided = false };
            }

            if (fastFail)
                throw new AssemblyResolutionException(name);
            return null;
        }

        CompositeAssemblyDefinition GetAssembly(string file, ReaderParameters parameters, bool isRuntimeProvided)
        {
            if (parameters.AssemblyResolver == null)
                parameters.AssemblyResolver = this;
            return new CompositeAssemblyDefinition
            {
                AssemblyDefinition = ModuleDefinition.ReadModule(file, parameters).Assembly,
                IsRuntimeProvided = isRuntimeProvided
            };
        }

        CompositeAssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
        {
            var extensions = new[] { ".exe", ".dll" };
            foreach (var directory in directories)
            {
                foreach (var extension in extensions)
                {
                    string file = Path.Combine(directory, name.Name + extension);
                    if (File.Exists(file))
                        return GetAssembly(file, parameters, false);
                }
            }

            return null;
        }

        static bool IsZero(Version version)
        {
            return version == null || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0);
        }

#if !SILVERLIGHT && !CF

        CompositeAssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
        {
            var version = reference.Version;
            var corlib = typeof(object).Assembly.GetName();

            if (corlib.Version == version || IsZero(version))
                return GetAssembly(typeof(object).Module.FullyQualifiedName, parameters, true);

            var path = Directory.GetParent(
                Directory.GetParent(
                    typeof(object).Module.FullyQualifiedName).FullName
                ).FullName;

            if (_onMono)
            {
                //if (version.Major == 1)
                //    path = Path.Combine(path, "1.0");
                //else if (version.Major == 2)
                //{
                //    if (version.MajorRevision == 5)
                //        path = Path.Combine(path, "2.1");
                //    else
                //        path = Path.Combine(path, "2.0");
                //}
                //else if (version.Major == 4)
                //    path = Path.Combine(path, "4.0");
                //else
                //    throw new NotSupportedException("Version not supported: " + version);
                var subPath = GetDirectoryByRuntimeVersion(Platform.Mono, version);
                path = Path.Combine(path, subPath);
            }
            else
            {
                //switch (version.Major)
                //{
                //    case 1:
                //        if (version.MajorRevision == 3300)
                //            path = Path.Combine(path, "v1.0.3705");
                //        else
                //            path = Path.Combine(path, "v1.0.5000.0");
                //        break;
                //    case 2:
                //        path = Path.Combine(path, "v2.0.50727");
                //        break;
                //    case 4:
                //        path = Path.Combine(path, "v4.0.30319");
                //        break;
                //    default:
                //        throw new NotSupportedException("Version not supported: " + version);
                //}
                var subPath = GetDirectoryByRuntimeVersion(Platform.Net, version);
                path = Path.Combine(path, subPath);
            }

            var file = Path.Combine(path, "mscorlib.dll");
            if (File.Exists(file))
                return GetAssembly(file, parameters, true);

            return null;
        }

        CompositeAssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
        {
            if (reference.PublicKeyToken == null || reference.PublicKeyToken.Length == 0)
                return null;

            if (_gacPaths == null)
                _gacPaths = GetGacPaths();

            if (_onMono)
                return GetAssemblyInMonoGac(reference, parameters);

            return GetAssemblyInNetGac(reference, parameters);
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

        CompositeAssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
        {
            for (int i = 0; i < _gacPaths.Count; i++)
            {
                var gac_path = _gacPaths[i];
                var file = GetAssemblyFile(reference, string.Empty, gac_path);
                if (File.Exists(file))
                    return GetAssembly(file, parameters, true);
            }

            return null;
        }

        CompositeAssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
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
                        return GetAssembly(file, parameters, true);
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

    // 解析时，相对于运行时 (ResolutionTime vs RunTime)
    class ResolutionTimeAssemblyResolver : BaseAssemblyResolver
    {
        readonly Dictionary<string, CompositeAssemblyDefinition> _cache;

        internal ResolutionTimeAssemblyResolver()
        {
            _cache = new Dictionary<string, CompositeAssemblyDefinition>(StringComparer.Ordinal);
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            CompositeAssemblyDefinition assembly;
            if (_cache.TryGetValue(fullName, out assembly))
                return assembly.AssemblyDefinition;

            assembly = DoResolve(AssemblyNameReference.Parse(fullName), parameters, true);
            if (assembly == null)
                return null; // 所有探测不到的程序集（不是 gac 或应用程序根目录下的程序集）都返回 null
            _cache[fullName] = assembly;
            return assembly.AssemblyDefinition;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CompositeAssemblyDefinition assembly;
            if (_cache.TryGetValue(name.FullName, out assembly))
                return assembly.AssemblyDefinition;

            assembly = DoResolve(name, parameters, false);
            if (assembly == null)
                return null; // 所有探测不到的程序集（不是 gac 或应用程序根目录下的程序集）都返回 null
            _cache[name.FullName] = assembly;
            return assembly.AssemblyDefinition;
        }

        public CompositeAssemblyDefinition ResolveAssembly(string fullName, ReaderParameters parameters)
        {
            CompositeAssemblyDefinition assembly;
            if (_cache.TryGetValue(fullName, out assembly))
                return assembly;

            assembly = DoResolve(AssemblyNameReference.Parse(fullName), parameters, true);
            if (assembly != null)
                _cache[fullName] = assembly;

            return assembly;
        }

        public CompositeAssemblyDefinition ResolveAssembly(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CompositeAssemblyDefinition assembly;
            if (_cache.TryGetValue(name.FullName, out assembly))
                return assembly;

            assembly = DoResolve(name, parameters, false);
            if (assembly != null)
                _cache[name.FullName] = assembly;

            return assembly;
        }
    }
}