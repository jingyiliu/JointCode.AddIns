using System;
using System.Collections.Generic;
using System.IO;
using JointCode.Common;
using JointCode.Common.Extensions;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core
{
    public enum AssemblyLoadMethod
    {
        Load,
        LoadFrom,
        LoadFile,
        LoadBytes
    }

    public abstract class AssemblyLoadPolicy : Disposable
    {
        public const string DefaultShadowCopyDirectory = "ShadowCopies";
        /// <summary>
        /// If this value is <c>true</c>, the addin assemblies will be copied to a temporary directory (called <c>ShadowCopyDirectory</c>) before loading, and 
        /// then they are loaded from there.
        /// If it is <c>false</c>, the addin assemblies will be read into a byte array, and then load from that array, with a price that some information of 
        /// the assembly will be lost (e.g, the location / codebase).
        /// Either way, the original assembly won't be locked, and can be removed / updated after the addin has been loaded.
        /// </summary>
        bool _useShadowCopy;
        /// <summary>
        /// A temporary directory to copy the assemblies before they are loaded.
        /// </summary>
        string _shadowCopyDirectory;
        /// <summary>
        /// Gets the private assembly probing directories. 
        /// This value will affect how assemblies are found at the system runtime and the addin resolving time. 
        /// </summary>
        string[] _privateAssemblyProbingDirectories;

        protected AssemblyLoadPolicy() { }

        /// <summary>
        /// Create an instance of <see cref="AssemblyLoadPolicy"/>
        /// </summary>
        /// <param name="useShadowCopy">Whether or not to use shadow copy policy to assembly loading.</param>
        /// <param name="shadowCopyDirectory">
        /// A temporary directory to copy the assemblies, and then they are loaded from there.
        /// Only valid when the <see cref="useShadowCopy"/> is set to true.
        /// If the path is relative, it is relative to the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>, which is usually the application base directory.
        /// Notes that the calling code must make sure that it has the right permission to access the specified shadow copy directory.
        /// </param>
        /// <param name="privateAssemblyProbingDirectories">
        /// Private directories where the assembly resolver can find the assembly references when it load the assembly at the system runtime and the addin resolving time. 
        /// If the path is relative, it is relative to the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>, which is usually the application base directory.
        /// </param>
        protected AssemblyLoadPolicy(bool useShadowCopy, string shadowCopyDirectory, string[] privateAssemblyProbingDirectories)
        {
            Initialize(useShadowCopy, shadowCopyDirectory, privateAssemblyProbingDirectories);
        }

        protected void Initialize(bool useShadowCopy, string shadowCopyDirectory, string[] privateAssemblyProbingDirectories)
        {
            _useShadowCopy = useShadowCopy;
            if (useShadowCopy)
            {
                if (shadowCopyDirectory.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("The [shadowCopyDirectory] can not be null or empty when the [useShadowCopy] is set to true!");
                //_shadowCopyDirectory = Path.IsPathRooted(shadowCopyDirectory) ? shadowCopyDirectory : Path.Combine(SystemHelper.AppDirectory, shadowCopyDirectory);
            }
            Initialize(SystemHelper.AppDirectory, shadowCopyDirectory, privateAssemblyProbingDirectories);
        }

        protected override void DoDispose()
        {
            if (_privateAssemblyProbingDirectories != null)
            {
                var privateProbingPaths = GetPrivateAssemblyProbingPaths();
                var probingPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
                probingPath = probingPath.Replace(privateProbingPaths, string.Empty);
                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = probingPath;
            }
        }

        /// <summary>
        /// If this value is <c>true</c>, the addin assemblies will be copied to a temporary directory (called <c>ShadowCopyDirectory</c>) before loading, and 
        /// then they are loaded from there.
        /// If it is <c>false</c>, the addin assemblies will be read into a byte array, and then load from that array, with a price that some information of 
        /// the assembly will be lost (e.g, the location / codebase).
        /// Either way, the original assembly won't be locked, and can be removed / updated after the addin has been loaded.
        /// </summary>
        public bool UseShadowCopy { get { return _useShadowCopy; } }

        /// <summary>
        /// A temporary directory to copy the assemblies before they are loaded.
        /// </summary>
        public string ShadowCopyDirectory { get { return _shadowCopyDirectory; } }

        /// <summary>
        /// Gets the private assembly probing directories. 
        /// This value will affect how assemblies are found at the system runtime and addin resolving time. 
        /// </summary>
        public string[] PrivateAssemblyProbingDirectories { get { return _privateAssemblyProbingDirectories; } }

        void Initialize(string appDirectory, string shadowCopyDirectory, string[] privateAssemblyProbingDirs)
        {
            if (shadowCopyDirectory.IsNullOrWhiteSpace())
            {
                _shadowCopyDirectory = Path.Combine(appDirectory, DefaultShadowCopyDirectory);
            }
            else if (!Path.IsPathRooted(shadowCopyDirectory))
            {
                //如果 shadowCopyDirectory 为相对路径
                _shadowCopyDirectory = Path.Combine(appDirectory, shadowCopyDirectory);
            }
            else
            {
                //如果 shadowCopyDirectory 为绝对路径
                _shadowCopyDirectory = shadowCopyDirectory;
            }

            _privateAssemblyProbingDirectories = privateAssemblyProbingDirs == null || privateAssemblyProbingDirs.Length == 0
                ? null
                : AddDirectories(appDirectory, privateAssemblyProbingDirs).ToArray();

            if (_privateAssemblyProbingDirectories != null)
            {
                var privateProbingPaths = GetPrivateAssemblyProbingPaths();
                var probingPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
                probingPath = probingPath + privateProbingPaths;
                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = probingPath;
            }
        }

        List<string> AddDirectories(string appDirectory, string[] dirs)
        {
            var result = new List<string>();
            foreach (var dir in dirs)
            {
                if (dir.IsNullOrWhiteSpace())
                    continue;

                var directory = Path.IsPathRooted(dir)
                    ? dir
                    : Path.Combine(appDirectory, dir);

                if (!result.Contains(directory))
                    result.Add(directory);
            }

            CheckDirectories(dirs);

            return result.Count > 0 ? result : null;
        }

        //a directory can not be a subdirectory of another directory.
        static void CheckDirectories(string[] dirs)
        {
            if (dirs.Length == 1)
                return;
            foreach (var dir in dirs)
            {
                foreach (var dir2 in dirs)
                {
                    if (ReferenceEquals(dir, dir2))
                        continue;
                    if (dir.StartsWith(dir2))
                        throw new InvalidDataException
                            (string.Format("An assembly probing directory can not be a subdirectory of another assembly probing directory! The direcoty {0} is a subdirectory of {1}, which has been specified as an assembly probing directory!", dir, dir2));
                }
            }
        }

        string GetPrivateAssemblyProbingPaths()
        {
            var result = string.Empty;
            for (int i = 0; i < _privateAssemblyProbingDirectories.Length; i++)
                result += _privateAssemblyProbingDirectories[i] + ";";
            return result;
        }

        public abstract AssemblyLoadMethod GetAssemblyLoadMethod(Addin addin);
    }
}