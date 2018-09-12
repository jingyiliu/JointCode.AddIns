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

namespace JointCode.AddIns.Core.Runtime
{
    abstract class AssemblyKey : IEquatable<AssemblyKey>//, IEqualityComparer<AssemblyKey>
    {
        static readonly AssemblyKeyEqualityComparer _comparer = new AssemblyKeyEqualityComparer();
        protected string _name;
        protected Version _version;
        protected CultureInfo _cultrue;
        protected byte[] _publicKeyToken;
        //protected Version _compatVersion;

        protected AssemblyKey() { HashCode = 0; }
        protected AssemblyKey(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken)
        {
        	_name = name;
        	_version = version;
        	_cultrue = cultrue;
        	_publicKeyToken = publicKeyToken;
            HashCode = 0;
        }

        internal static IEqualityComparer<AssemblyKey> EqualityComparer { get { return _comparer; } }

        internal int HashCode { get; set; }
        internal string Name { get { return _name; } }
        internal Version Version { get { return _version; } }
        //internal Version CompatVersion { get { return _compatVersion; } }
        internal CultureInfo CultureInfo { get { return _cultrue; } }
        internal byte[] PublicKeyToken { get { return _publicKeyToken; } }

        #region IEquatable<AssemblyKey>
        public bool Equals(AssemblyKey other)
        {
            return other == null
                ? false
                : ReferenceEquals(this, other) || EqualsInternal(other);
        } 
        #endregion

        #region Object
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            var obj = other as AssemblyKey;
            if (obj == null)
                return false;
            return EqualsInternal(obj);
        }

        bool EqualsInternal(AssemblyKey other)
        {
            var result = _comparer.Equals(this, other);
            if (!result)
                return false;
            if (_publicKeyToken == null && other._publicKeyToken == null)
                return true;
            if (_publicKeyToken != null && other._publicKeyToken != null && _publicKeyToken.Length == other._publicKeyToken.Length)
            {
                for (int i = 0; i < _publicKeyToken.Length; i++)
                {
                    if (_publicKeyToken[i] != other._publicKeyToken[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }
        #endregion

        //#region IEqualityComparer<AssemblyKey>
        //public bool Equals(AssemblyKey x, AssemblyKey y)
        //{
        //    return _comparer.Equals(x, y);
        //}

        //public int GetHashCode(AssemblyKey obj)
        //{
        //    return _comparer.GetHashCode(obj);
        //} 
        //#endregion
    }

    class AssemblyKeyEqualityComparer : IEqualityComparer<AssemblyKey>
    {
        #region IEqualityComparer<AssemblyKey> Members

        //x = old object in the dictionary, y = new object passing to the dictionary
        public bool Equals(AssemblyKey x, AssemblyKey y)
        {
            return x != null && y != null 
                && x.Name == y.Name
                && x.Version.CompareTo(y.Version) == 0
                && x.CultureInfo.Equals(y.CultureInfo);
        }

        public int GetHashCode(AssemblyKey obj)
        {
            //if (obj == null)
            //    return 0;
            if (obj.HashCode != 0)
                return obj.HashCode;

            //Get HashCode of AssemblyName.PublicKeyToken
            var hashCode = 17;
            if (obj.PublicKeyToken != null)
            {
                unchecked
                {
                    foreach (byte b in obj.PublicKeyToken)
                        hashCode = hashCode * 31 + b.GetHashCode();
                }
            }
            //Get HashCode of AssemblyName.Name
            var name = obj.Name;
            var length = name.Length;
            if (length > 0)
            {
                // Compute hash for strings with length greater than 1
                var firstChar = name[0];          // First char of string we use
                var lastChar = name[length - 1];  // Last char
                // Compute hash code from two characters
                int part1 = firstChar + length;
                hashCode = (89 * part1) + lastChar + length; //89 = better distribution
            }

            obj.HashCode = hashCode;
            return hashCode;
        }

        #endregion
    }
}
