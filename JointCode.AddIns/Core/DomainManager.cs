////
//// Authors:
////   刘静谊 (Johnny Liu) <jingeelio@163.com>
////
//// Copyright (c) 2017 刘静谊 (Johnny Liu)
////
//// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
////

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using JointCode.Common.Helpers;

//namespace JointCode.AddIns.Core
//{
//    class DomainManager : IDisposable
//    {
//        const BindingFlags BINDINGFLAGS = BindingFlags.CreateInstance | BindingFlags.Instance;
//        Dictionary<string, AppDomain> _domains = new Dictionary<string, AppDomain>();
        
//        public T CreateMarshalObject<T>(string domainName) where T : MarshalByRefObject
//        {
//            AppDomain domain;
//            if (!_domains.TryGetValue(domainName, out domain))
//                domain = CreateDomain(domainName);
//            return CreateMarshalObject<T>(domain);
//        }

//        public T CreateMarshalObject<T>(AppDomain domain) where T : MarshalByRefObject
//        {
//            var assemblyFile = ReflectionHelper.Location(typeof(T).Assembly);
//            //var assemblyFile = typeof(T).Assembly.Location;
//            var obj = CreateMarshalObject(domain, assemblyFile, typeof(T).FullName);
//            return obj as T;
//        }

//        //assembly: might be assembly path or assembly name where the marshalTypeFullName resides
//        public object CreateMarshalObject(AppDomain domain, string assembly, string marshalTypeFullName)
//        {
//            if (assembly.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
//                assembly.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
//            {
//                try
//                {
//                    return File.Exists(assembly)
//                        ? domain.CreateInstanceFromAndUnwrap(assembly, marshalTypeFullName)
//                        : null;
//                }
//                catch(Exception e)
//                {
//                    return null;
//                }
//            }
//            else
//            {
//                try
//                {
//                    return domain.CreateInstanceAndUnwrap(assembly, marshalTypeFullName);
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//        }

//        #region Appdomain
//        public AppDomain CreateDomain(string domainName)
//        {
//            //The [applicationBase] and [shadowCopyDirectories] must be set correctly, otherwise a "could not find assembly" exception might throw.
//            return CreateDomain(domainName, null, null);
//        }

//        /// <summary>
//        /// Creates the domain.
//        /// </summary>
//        /// <param name="domainName">FriendName of the domain.</param>
//        /// <param name="applicationBase">The directory within which this AppDomain will search for referenced assemblies.</param>
//        /// <param name="shadowCopyDirectories">The shadow copy directories.</param>
//        /// <returns></returns>
//        public AppDomain CreateDomain(string domainName, string applicationBase, string shadowCopyDirectories)
//        {
//            AppDomain domain;
//            if (!_domains.TryGetValue(domainName, out domain))
//            {
//                AppDomainSetup setup = new AppDomainSetup();
//                //***********************************************************************************************************************************************
//                //使用 ShadowCopyDirectories 属性，您可以限制要进行影像复制的程序集。
//                //对应用程序域启用影像复制时，系统默认会复制应用程序路径（即 ApplicationBase 和 PrivateBinPath 属性指定的目录）下的所有程序集。
//                //通过指定一个文件夹，令其中仅包含要进行影像复制的程序集及目录，然后将该文件夹赋给 ShadowCopyDirectories 属性，这样系统仅会对
//                //指定目录及其子目录下的程序集进行影像复制。如果有多个路径，可以使用分号分隔路径。
//                setup.ShadowCopyFiles = "true";
//                setup.ShadowCopyDirectories = shadowCopyDirectories;

//                //***********************************************************************************************************************************************
//                //ApplicationBase 指定了该应用程序域在搜索程序集时，要在其中搜索程序集的目录。
//                setup.ApplicationBase = applicationBase;
//                //除了 ApplicationBase 目录之外，该应用程序域可能还需要在应用程序主目录中搜索程序集，因为具体 MarshalByRefObject 继承类所在的
//                //程序集可能位于应用程序主目录下。这时可以将 PrivateBinPath 设置为应用程序主目录。
//                setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;

//                //***********************************************************************************************************************************************
//                //另外，也可以使用 CachePath 属性和 ApplicationName 属性来指定要进行影像复制的文件所在的位置。
//                //通过将 ApplicationName 属性作为子目录连接到 CachePath 属性，这样便构成自定义位置的基路径。程序集将被影像复制到此路径的子目录下，而不是基路径本身。
//                //注意，如果未设置 ApplicationName 属性，则忽略 CachePath 属性并使用下载缓存，而不会引发异常。
//                //如果您指定了自定义位置，那么当不再需要这些目录和已进行影像复制的文件时，您还需负责将它们清除，因为它们不会自动被删除。
//                //需要为影像复制的文件设置自定义位置的原因可能有两个。如果应用程序生成了大量副本，则您可能需要为影像复制的文件设置自定义位置。限制下载缓存的因素是大小而非生
//                //存期，因此公共语言运行库可能会尝试删除仍然在使用的文件。设置自定义位置的另一个原因是：运行应用程序的用户对公共语言运行库 用作下载缓存的目录位置不具有写访问
//                //权限。
//                //setup.ApplicationName = domainName;
//                //setup.CachePath = "";
//                //setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile; 

//                domain = AppDomain.CreateDomain(domainName, null, setup);
//                _domains.Add(domainName, domain);
//            }
//            return domain;
//        }

//        public void UnloadDomain(string domainName)
//        {
//            AppDomain domain;
//            if (_domains.TryGetValue(domainName, out domain))
//            {
//                _domains.Remove(domainName);
//                AppDomain.Unload(domain);
//            }
//        } 

//        #endregion

//        #region Disposable
        
//        public void Dispose()
//        {
//        	Dispose(true);
//        }

//        void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                foreach (var domain in _domains.Values)
//                    AppDomain.Unload(domain);
//                _domains.Clear();
//            }
//        }

//        #endregion
//    }
//}
