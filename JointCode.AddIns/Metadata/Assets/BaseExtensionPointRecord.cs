//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.IO;
using JointCode.Common;
using JointCode.Common.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    class BaseExtensionPointRecord : ISerializableRecord
    {
        internal static MyFunc<BaseExtensionPointRecord> Factory = () => new BaseExtensionPointRecord();

        // Guid 可以在不同应用程序之间唯一地标识一个 Addin，而由不同 Addin 提供的所有 ExtensionPoint 在一个应用程序中必须唯一。
        /// <summary>
        /// Gets the id of <see cref="IExtensionPoint"/> that uniquely identify an <see cref="IExtensionPoint"/> within an application.
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of an <see cref="ExtensionPointRecord"/>.
        /// </summary>
        internal int Uid { get; set; }

        public void Read(Stream reader)
        {
            Id = reader.ReadString();
            Uid = reader.ReadInt32();
        }

        public void Write(Stream writer)
        {
            writer.WriteString(Id);
            writer.WriteInt32(Uid);
        }
    }
}