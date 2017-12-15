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
    abstract class AssemblyKey : IEquatable<AssemblyKey>
    {
        readonly AssemblyKeyEqualityComparer _comparer = new AssemblyKeyEqualityComparer();
        protected string _name;
        protected Version _version;
        protected CultureInfo _cultrue;
        protected byte[] _publicKeyToken;
//        protected Version _compatVersion;

        protected AssemblyKey(string name, Version version, CultureInfo cultrue, byte[] publicKeyToken)
        {
        	_name = name;
        	_version = version;
        	_cultrue = cultrue;
        	_publicKeyToken = publicKeyToken;
        }

        internal string Name
        {
            get { return _name; }
        }

        internal Version Version
        {
            get { return _version; }
        }

//        internal Version CompatVersion
//        {
//            get { return _compatVersion; }
//        }

        internal CultureInfo CultureInfo
        {
            get { return _cultrue; }
        }

        internal byte[] PublicKeyToken
        {
            get { return _publicKeyToken; }
        }

        public bool Equals(AssemblyKey other)
        {
            return other == null
                ? false
                : _comparer.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }
    }

    class AssemblyKeyEqualityComparer : IEqualityComparer<AssemblyKey>
    {
        int _hashCode = 0;

        #region IEqualityComparer<AssemblyKey> Members

        //x = old object in the dictionary, y = new object passing to the dictionary
        public bool Equals(AssemblyKey x, AssemblyKey y)
        {
            return x.Name == y.Name
                && x.Version.CompareTo(y.Version) == 0
                && x.CultureInfo.Equals(y.CultureInfo);
        }

        public int GetHashCode(AssemblyKey obj)
        {
            //if (obj == null)
            //    return 0;
            if (_hashCode != 0)
                return _hashCode;

            //Get HashCode of AssemblyName.PublicKeyToken
            _hashCode = 17;
            if (obj.PublicKeyToken != null)
            {
                unchecked
                {
                    foreach (byte b in obj.PublicKeyToken)
                        _hashCode = _hashCode * 31 + b.GetHashCode();
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
                _hashCode = (89 * part1) + lastChar + length; //89 = better distribution
            }

            return _hashCode;
        }

        #endregion
    }
}
