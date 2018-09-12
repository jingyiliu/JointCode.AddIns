using System;
using System.Globalization;

namespace JointCode.AddIns.Core.Dependencies
{
    [Serializable]
    public class DependencyDescription
    {
        public DependencyDescription() { }
        public DependencyDescription(AssemblyDependency[] applicationAssemblies, AddinDependency[] extendedAddins)
        {
            ApplicationAssemblies = applicationAssemblies;
            ExtendedAddins = extendedAddins;
        }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
        /// <summary>
        /// Application assemblies that this addin depends on.
        /// </summary>
        public AssemblyDependency[] ApplicationAssemblies { get; set; }
        /// <summary>
        /// assemblies provided by other addins that this addin depends on.
        /// </summary>
        public AssemblyDependency[] AddinAssemblies { get; set; }
        /// <summary>
        /// Other addins that this addin extends (provide extensions / extension builders for them).
        /// </summary>
        public AddinDependency[] ExtendedAddins { get; set; }
    }

    [Serializable]
    public class AssemblyDependency : IEquatable<AssemblyDependency>
    {
        readonly string _name;
        readonly Version _version;
        readonly CultureInfo _cultrue;
        readonly byte[] _publicKeyToken;
        //readonly Version _compatVersion;

        public AssemblyDependency(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken)
        {
            _name = name;
            _version = version;
            _cultrue = cultrue;
            _publicKeyToken = publicKeyToken;
        }

        public string Name { get { return _name; } }
        public Version Version { get { return _version; } }
        //public Version CompatVersion { get { return _compatVersion; } }
        public CultureInfo CultureInfo { get { return _cultrue; } }
        public byte[] PublicKeyToken { get { return _publicKeyToken; } }

        public bool Equals(AssemblyDependency other)
        {
            var result = _name == other._name
                && _version.CompareTo(other._version) == 0
                && _cultrue.Equals(other._cultrue);
            if (!result)
                return false;
            if (_publicKeyToken == null && other._publicKeyToken == null)
                return true;
            if ((_publicKeyToken == null && other._publicKeyToken != null)
                || (_publicKeyToken != null && other._publicKeyToken == null)
                || (_publicKeyToken.Length != other._publicKeyToken.Length))
                return false;
            for (int i = 0; i < _publicKeyToken.Length; i++)
            {
                if (_publicKeyToken[i] != other._publicKeyToken[i])
                    return false;
            }
            return true;
        }
    }

    [Serializable]
    public class AddinDependency
    {
        public AddinDependency(Guid guid, Version version, Version compatibleVersion)
        {
            Guid = guid;
            Version = version;
            CompatibleVersion = compatibleVersion;
        }

        public Guid Guid { get; set; }
        public Version Version { get; set; }
        public Version CompatibleVersion { get; set; }
    }
}