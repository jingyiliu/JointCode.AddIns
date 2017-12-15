//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.Common;

namespace JointCode.AddIns.Metadata.Assets
{
    class ExtendedAddinRecord : ReferencedAddinRecord, IEquatable<ExtendedAddinRecord>, ISerializableRecord
    {
        internal new static MyFunc<ExtendedAddinRecord> Factory = () => new ExtendedAddinRecord();

        #region IEquatable<DependedAddinRecord> Members

        public bool Equals(ExtendedAddinRecord other)
        {
            return Uid == other.Uid;
        }

        #endregion
    }
}