//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    interface ISerializableRecord
    {
        void Read(Stream reader);
        void Write(Stream writer);
    }
}