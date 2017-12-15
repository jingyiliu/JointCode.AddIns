//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class DataFileRecord : DataFileResolution, ISerializableRecord
    {
        internal static MyFunc<DataFileRecord> Factory = () => new DataFileRecord();

        public void Read(Stream reader)
        {
            FilePath = reader.ReadString();
        }

        public void Write(Stream writer)
        {
            writer.WriteString(FilePath);
        }
    }
}