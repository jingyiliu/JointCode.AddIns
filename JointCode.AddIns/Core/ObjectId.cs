//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using JointCode.Common.IO;

namespace JointCode.AddIns.Core
{
    public class ObjectId : IEquatable<ObjectId>
    {
        public Guid Guid { get; protected internal set; }
        public int Uid { get; protected internal set; }

        /// <summary>
        /// Gets the owner tag that uniquely identify an <see cref="ObjectId"/> at rumtime. 
        /// The framework compares the pointer of <see cref="Tag"/> to determine whether the requesting (source) addin 
        /// is the same to the requested (target) addin.
        /// An <see cref="ObjectId"/> created by the user code manually at runtime does not have a <see cref="Tag"/>.
        /// </summary>
        internal virtual object Tag
        {
            get { return null; }
        }

        public bool Equals(ObjectId other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ObjectId left, ObjectId right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ObjectId left, ObjectId right)
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

    public class AddinId : ObjectId
    {
        readonly object _ownerTag;

        public AddinId()
        {
            _ownerTag = new object();
            Uid = UidProvider.InvalidAddinUid;
        }

        internal override object Tag
        {
            get { return _ownerTag; }
        }

        internal void Read(Stream reader)
        {
            Guid = reader.ReadGuid();
            Uid = reader.ReadInt32();
        }

        internal void Write(Stream writer)
        {
            writer.WriteGuid(Guid);
            writer.WriteInt32(Uid);
        }
    }
}
