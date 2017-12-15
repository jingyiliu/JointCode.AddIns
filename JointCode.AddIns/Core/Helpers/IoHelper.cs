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
using System.Threading;

namespace JointCode.AddIns.Core.Helpers
{
    static class IoHelper
    {
        //打开文件时一般习惯直接使用
        //FileStream fs = new FileStream(fileName, FileMode.Open);
        //这个方法打开文件的时候是以只读共享的方式打开的，但若此文件已被一个拥有写权限的进程打开的话，就无法读取了，因此需要使用
        //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //设置文件共享方式为读写：FileShare.ReadWrite，这样的话问题就解决了。
        internal static Stream OpenReadWriteShare(string filePath)
        {
            return File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        //Let's say we want to open a file, which will be used by multiple processes.
        //I start with an assumption that the file is write locked until it gets changed. 
        //So the flow looks like that:
        //1) We try to open the file
        //2) If we have an IOException, we wait until the file gets changed
        //3) We try to open file again, if failed - wait again
        //4) If file opened successfully, we perform an action passed as a parameter
        internal static Stream SaveOpen(string filePath)
        {
            var autoResetEvent = new AutoResetEvent(false);

            while (true)
            {
                try
                {
                    return File.Open(filePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.Write);
                }
                catch (IOException)
                {
                    var fileSystemWatcher =
                        new FileSystemWatcher(Path.GetDirectoryName(filePath)) { EnableRaisingEvents = true };

                    fileSystemWatcher.Changed += (o, e) =>
                        {
                            if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(filePath))
                            {
                                autoResetEvent.Set();
                            }
                        };

                    autoResetEvent.WaitOne(100);
                }
            }
        }

        ///// <summary>
        ///// Try to do an action on a file until a specific amount of time
        ///// </summary>
        ///// <param name="path">Path of the file</param>
        ///// <param name="action">Action to execute on file</param>
        ///// <param name="milliSecondMax">Maimum amount of time to try to do the action</param>
        ///// <returns>true if action occur and false otherwise</returns>
        //public static bool SaveOpen(string path, Action<FileStream> action, int milliSecondMax = Timeout.Infinite)
        //{
        //    bool result = false;
        //    DateTime dateTimestart = DateTime.Now;
        //    Tuple<AutoResetEvent, FileSystemWatcher> tuple = null;

        //    while (true)
        //    {
        //        try
        //        {
        //            using (var file = File.Open(path,
        //                                        FileMode.OpenOrCreate,
        //                                        FileAccess.ReadWrite,
        //                                        FileShare.Write))
        //            {
        //                action(file);
        //                result = true;
        //                break;
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            // Init only once and only if needed. Prevent against many instantiation in case of multhreaded 
        //            // file access concurrency (if file is frequently accessed by someone else). Better memory usage.
        //            if (tuple == null)
        //            {
        //                var autoResetEvent = new AutoResetEvent(true);
        //                var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path))
        //                {
        //                    EnableRaisingEvents = true
        //                };

        //                fileSystemWatcher.Changed +=
        //                    (o, e) =>
        //                    {
        //                        if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(path))
        //                        {
        //                            autoResetEvent.Set();
        //                        }
        //                    };

        //                tuple = new Tuple<AutoResetEvent, FileSystemWatcher>(autoResetEvent, fileSystemWatcher);
        //            }

        //            int milliSecond = Timeout.Infinite;
        //            if (milliSecondMax != Timeout.Infinite)
        //            {
        //                milliSecond = (int)(DateTime.Now - dateTimestart).TotalMilliseconds;
        //                if (milliSecond >= milliSecondMax)
        //                {
        //                    result = false;
        //                    break;
        //                }
        //            }

        //            tuple.Item1.WaitOne(milliSecond);
        //        }
        //    }

        //    if (tuple != null && tuple.Item1 != null) // Dispose of resources now (don't wait the GC).
        //    {
        //        tuple.Item1.Dispose();
        //        tuple.Item2.Dispose();
        //    }

        //    return result;
        //}

        internal static string GetRelativePath(string path, string relativeTo)
        {
            return path.StartsWith(relativeTo, StringComparison.InvariantCultureIgnoreCase) ? path.Substring(relativeTo.Length + 1) : path;
        }

        internal static void CreateFile(string directory, string fullFilePath)
        {
            if (File.Exists(fullFilePath))
                return;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var stream = File.Create(fullFilePath);
            stream.Close();
        }

        internal static DateTime GetLastWriteTime(string fullFilePath)
        {
            return File.GetLastWriteTime(fullFilePath);
        }

        internal static int GetFileHash(string fullFilePath)
        {
            return 0;
        }
    }
}
