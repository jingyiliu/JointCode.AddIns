//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Resolving
{
    abstract class CollisionKey
    {
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(CollisionKey left, CollisionKey right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(CollisionKey left, CollisionKey right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    class AddinCollisionKey : CollisionKey
    {
        Guid _guid;

        internal AddinCollisionKey(Guid guid)
        { _guid = guid; }

        public override bool Equals(object obj)
        {
            var that = obj as AddinCollisionKey;
            return that != null && _guid == that._guid;
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }

    abstract class StringCollisionKey : CollisionKey
    {
        readonly string _value;

        protected StringCollisionKey(string value)
        {
            Requires.Instance.NotNull(value, "value");
            _value = value;
        }

        public override bool Equals(object obj)
        {
            var that = obj as StringCollisionKey;
            return that != null && _value == that._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }

    class ExtensionPointCollisionKey : StringCollisionKey
    {
        internal ExtensionPointCollisionKey(string value) : base(value) { }
    }

    class ExtensionBuilderCollisionKey : StringCollisionKey
    {
        internal ExtensionBuilderCollisionKey(string value) : base(value) { }
    }

    class ExtensionCollisionKey : StringCollisionKey
    {
        internal ExtensionCollisionKey(string value) : base(value) { }
    }
}