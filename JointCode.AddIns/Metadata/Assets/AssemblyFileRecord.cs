//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.Configuration;
using System.Diagnostics;
using JointCode.AddIns.Resolving.Assets;
using JointCode.Common;
using JointCode.Common.IO;
using System.IO;

namespace JointCode.AddIns.Metadata.Assets
{
    //class AssemblyFileRecordSet : List<AssemblyFileRecord>
    //{
    //}

    class AssemblyFileRecord : AssemblyFileResolution, ISerializableRecord
    {
        internal static MyFunc<AssemblyFileRecord> Factory = () => new AssemblyFileRecord();

        string _loadPath, _fullPath;

        // 程序集的加载路径。在 AddinOptions 中，如果使用了 shadow copy，该路径可能与程序集在插件中的位置不同
        internal string LoadPath { get { return _loadPath ?? _fullPath; } }
        // 程序集在插件中的完整路径
        internal string FullPath { get { return _fullPath; } }

        internal bool Loaded { get; set; }

        internal void SetDirectory(string directory)
        {
            if (_fullPath == null)
                _fullPath = Path.Combine(directory, FilePath);
        }

        // 由于阴影复制程序集的原文件可能被替换，并再次被加入阴影复制文件夹中，因此我们这里以程序集的文件版本号来区分相同程序集的不同版本
        internal void SetShadowCopyDirectory(string shadowCopyDirectory)
        {
            if (_loadPath != null)
                return;

            var fvi = FileVersionInfo.GetVersionInfo(FullPath);

            var fileName = Path.GetFileName(FilePath);

            var lastIndex = fileName.LastIndexOf('.');
            var newFileName = fileName.Substring(0, lastIndex + 1)
                + fvi.FileMajorPart + '.' + fvi.FileMinorPart + '.' + fvi.FileBuildPart + '.' + fvi.FilePrivatePart 
                + '.' + fileName.Substring(lastIndex + 1, fileName.Length - lastIndex - 1);

            _loadPath = Path.Combine(shadowCopyDirectory, newFileName);
        }

        public void Read(Stream reader)
        {
            FilePath = reader.ReadString();
            //IsPublic = reader.ReadBoolean();
            Uid = reader.ReadInt32();
            LastWriteTime = reader.ReadDateTime();
        }

        public void Write(Stream writer)
        {
            writer.WriteString(FilePath);
            //writer.WriteBoolean(IsPublic);
            writer.WriteInt32(Uid);
            writer.WriteDateTime(LastWriteTime);
        }
    }

    static class AssemblyFileRecordExtensions
    {
        public static bool Exists(this AssemblyFileRecord item)
        {
            return File.Exists(item.FilePath);
        }

        public static bool UnChanged(this AssemblyFileRecord item)
        {
            return item.LastWriteTime == File.GetLastWriteTime(item.FilePath);
        }
    }
}