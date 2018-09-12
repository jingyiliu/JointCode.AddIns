//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;

namespace JointCode.AddIns.Core.Data
{
    // this class is used to postpone the time of getting the real metadata to be written to the persistence file.
    abstract class DataHolder
    {
        protected object _val;
        internal DataHolder() { }
        internal DataHolder(object val) { _val = val; }
        internal object Value { get { return _val; } }
        internal abstract sbyte KnownTypeCode { get; }
        internal abstract void Read(Stream reader);
        internal abstract void Write(Stream writer);
    }   
}