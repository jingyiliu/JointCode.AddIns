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
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core.Helpers
{
    static class IoHelper
    {
        internal static void ClearContent(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Truncate, FileAccess.ReadWrite);
            fs.Close();
        }

        //打开文件时一般习惯直接使用
        //FileStream fs = new FileStream(fileName, FileMode.Open);
        //这个方法打开文件的时候是以只读共享的方式打开的，但若此文件已被一个拥有写权限的进程打开的话，就无法读取了，因此需要使用
        //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //设置文件共享方式为读写：FileShare.ReadWrite，这样的话问题就解决了。
        internal static FileStream OpenReadWrite(string filePath)
        {
            if (File.Exists(filePath))
                return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Open the specified file for read, and does not share it with any other process.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static FileStream OpenRead(string filePath)
        {
            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Open the specified file for write, and does not share it with any other process.
        /// If the file does not exist, it will create the file first.
        /// Notes that this method will clear the content of file before return.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static FileStream OpenWrite(string filePath)
        {
            if (File.Exists(filePath))
            {
                var result = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                result.Seek(0, SeekOrigin.Begin);
                result.SetLength(0); //清空txt文件
                return result;
            }
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        //Let's say we want to open a file, which will be used by multiple processes.
        //I start with an assumption that the file is write locked until it gets changed. 
        //So the flow looks like that:
        //1) We try to open the file
        //2) If we have an IOException, we wait until the file gets changed
        //3) We try to open file again, if failed - wait again
        //4) If file opened successfully, we perform an action passed as a parameter
        /// <summary>
        /// Tries to open a file, and if the file is occupied by another process, wait until that 
        /// process release the file, and then open it again.
        /// </summary>
        internal static FileStream OpenSafely(string filePath)
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
        //public static bool OpenSafely(string path, Action<FileStream> action, int milliSecondMax = Timeout.Infinite)
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

        //internal static void CreateFile(string fullFilePath)
        //{
        //    if (File.Exists(fullFilePath))
        //        return;
        //    var directory = Path.GetDirectoryName(fullFilePath);
        //    if (!Directory.Exists(directory))
        //        Directory.CreateDirectory(directory);
        //    var stream = File.Create(fullFilePath);
        //    stream.Close();
        //}

        //internal static void CreateFile(string directory, string fullFilePath)
        //{
        //    if (File.Exists(fullFilePath))
        //        return;
        //    if (!Directory.Exists(directory))
        //        Directory.CreateDirectory(directory);
        //    var stream = File.Create(fullFilePath);
        //    stream.Close();
        //}

        internal static DateTime GetLastWriteTime(string fullFilePath)
        {
            return File.GetLastWriteTime(fullFilePath);
        }

        internal static FileInfo GetFileInfo(string fullFilePath)
        {
            return new FileInfo(fullFilePath);
        }

        internal static string GetFileHash(string fullFilePath)
        {
            return HashHelper.ComputeMD5(fullFilePath);
        }
    }

    //using System;
    //using System.IO;

    //namespace JointCode.AddIns.Core.Helpers
    //{
    //    class FileHelper
    //    {
    //        void CleanDirectory(string dir)
    //        {
    //            foreach (string file in Directory.GetFiles(dir, "*.new"))
    //                File.Delete(file);

    //            foreach (string sdir in Directory.GetDirectories(dir))
    //                CleanDirectory(sdir);
    //        }

    //        IDisposable FileLock(FileAccess access, int timeout)
    //        {
    //            DateTime tim = DateTime.Now;
    //            DateTime wt = tim;

    //            FileShare share = access == FileAccess.Read ? FileShare.Read : FileShare.None;
    //            string path = Path.GetDirectoryName(DatabaseLockFile);

    //            if (!Directory.Exists(path))
    //                Directory.CreateDirectory(path);

    //            do
    //            {
    //                try
    //                {
    //                    return new FileStream(DatabaseLockFile, FileMode.OpenOrCreate, access, share);
    //                }
    //                catch (IOException)
    //                {
    //                    // Wait and try again
    //                    if ((DateTime.Now - wt).TotalSeconds >= 4)
    //                    {
    //                        Console.WriteLine("Waiting for " + access + " add-in database lock");
    //                        wt = DateTime.Now;
    //                    }

    //                }
    //                System.Threading.Thread.Sleep(100);
    //            }
    //            while (timeout <= 0 || (DateTime.Now - tim).TotalMilliseconds < timeout);

    //            throw new Exception("Lock timed out");
    //        }

    //        public Stream Create(string fileName)
    //        {
    //            if (inTransaction)
    //            {
    //                deletedFiles.Remove(fileName);
    //                deletedDirs.Remove(Path.GetDirectoryName(fileName));
    //                foldersToUpdate[Path.GetDirectoryName(fileName)] = null;
    //                return File.Create(fileName + ".new");
    //            }
    //            else
    //                return File.Create(fileName);
    //        }

    //        public void Rename(string fileName, string newName)
    //        {
    //            if (inTransaction)
    //            {
    //                deletedFiles.Remove(newName);
    //                deletedDirs.Remove(Path.GetDirectoryName(newName));
    //                foldersToUpdate[Path.GetDirectoryName(newName)] = null;
    //                string s = File.Exists(fileName + ".new") ? fileName + ".new" : fileName;
    //                File.Copy(s, newName + ".new");
    //                Delete(fileName);
    //            }
    //            else
    //                File.Move(fileName, newName);
    //        }

    //        public Stream OpenRead(string fileName)
    //        {
    //            if (inTransaction)
    //            {
    //                if (deletedFiles.Contains(fileName))
    //                    throw new FileNotFoundException();
    //                if (File.Exists(fileName + ".new"))
    //                    return File.OpenRead(fileName + ".new");
    //            }
    //            return File.OpenRead(fileName);
    //        }

    //        public void Delete(string fileName)
    //        {
    //            if (inTransaction)
    //            {
    //                if (deletedFiles.Contains(fileName))
    //                    return;
    //                if (File.Exists(fileName + ".new"))
    //                    File.Delete(fileName + ".new");
    //                if (File.Exists(fileName))
    //                    deletedFiles[fileName] = null;
    //            }
    //            else
    //            {
    //                File.Delete(fileName);
    //            }
    //        }

    //        public void DeleteDir(string dirName)
    //        {
    //            if (inTransaction)
    //            {
    //                if (deletedDirs.Contains(dirName))
    //                    return;
    //                if (Directory.Exists(dirName + ".new"))
    //                    Directory.Delete(dirName + ".new", true);
    //                if (Directory.Exists(dirName))
    //                    deletedDirs[dirName] = null;
    //            }
    //            else
    //            {
    //                Directory.Delete(dirName, true);
    //            }
    //        }


    //        public bool Exists(string fileName)
    //        {
    //            if (inTransaction)
    //            {
    //                if (deletedFiles.Contains(fileName))
    //                    return false;
    //                if (File.Exists(fileName + ".new"))
    //                    return true;
    //            }
    //            return File.Exists(fileName);
    //        }

    //        public bool DirExists(string dir)
    //        {
    //            return Directory.Exists(dir);
    //        }

    //        public void CreateDir(string dir)
    //        {
    //            Directory.CreateDirectory(dir);
    //        }

    //        public string[] GetDirectories(string dir)
    //        {
    //            return Directory.GetDirectories(dir);
    //        }

    //        public bool DirectoryIsEmpty(string dir)
    //        {
    //            foreach (string f in Directory.GetFiles(dir))
    //            {
    //                if (!inTransaction || !deletedFiles.Contains(f))
    //                    return false;
    //            }
    //            return true;
    //        }

    //        public string[] GetDirectoryFiles(string dir, string pattern)
    //        {
    //            if (pattern == null || pattern.Length == 0 || pattern.EndsWith("*"))
    //                throw new NotSupportedException();

    //            if (inTransaction)
    //            {
    //                Hashtable files = new Hashtable();
    //                foreach (string f in Directory.GetFiles(dir, pattern))
    //                {
    //                    if (!deletedFiles.Contains(f))
    //                        files[f] = f;
    //                }
    //                foreach (string f in Directory.GetFiles(dir, pattern + ".new"))
    //                {
    //                    string ofile = f.Substring(0, f.Length - 4);
    //                    files[ofile] = ofile;
    //                }
    //                string[] res = new string[files.Count];
    //                int n = 0;
    //                foreach (string s in files.Keys)
    //                    res[n++] = s;
    //                return res;
    //            }
    //            else
    //                return Directory.GetFiles(dir, pattern);
    //        }
    //    }
    //}
}
