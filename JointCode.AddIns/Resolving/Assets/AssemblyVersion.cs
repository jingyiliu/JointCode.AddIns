using System;
using System.Collections.Generic;

namespace JointCode.AddIns.Resolving.Assets
{
    public struct AssemblyVersion : IEquatable<AssemblyVersion>, IEqualityComparer<AssemblyVersion>
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Revision { get; set; }
        public int Build { get; set; }

        public short MajorRevision { get; set; }
        public short MinorRevision { get; set; }

        public static implicit operator AssemblyVersion(Version v)
        {
            return new AssemblyVersion
            {
                Major = v.Major,
                Minor = v.Minor,
                Build = v.Build,
                Revision = v.Revision,
                MajorRevision = v.MajorRevision,
                MinorRevision = v.MinorRevision
            };
        }

        public bool Equals(AssemblyVersion other)
        {
            AssemblyVersion obj;
            try
            {
                obj = (AssemblyVersion)other;
            }
            catch
            {
                return false;
            }
            return Major == obj.Major
                && Minor == obj.Minor
                && Build == obj.Build
                && Build == obj.Build
                && MajorRevision == obj.MajorRevision
                && MinorRevision == obj.MinorRevision;
        }

        public bool Equals(AssemblyVersion x, AssemblyVersion y)
        {
            return x.Major == y.Major
                && x.Minor == y.Minor
                && x.Build == y.Build
                && x.Build == y.Build
                && x.MajorRevision == y.MajorRevision
                && x.MinorRevision == y.MinorRevision;
        }

        public int GetHashCode(AssemblyVersion obj)
        {
            var result = obj.Major;
            if (obj.Minor > 0)
                result ^= obj.Minor;
            if (obj.Build > 0)
                result ^= obj.Build;
            if (obj.Revision > 0)
                result ^= obj.Revision;
            if (obj.MajorRevision > 0)
                result ^= obj.MajorRevision;
            if (obj.MinorRevision > 0)
                result ^= obj.MinorRevision;
            return result;
        }
    }
}