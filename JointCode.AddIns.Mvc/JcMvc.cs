using JointCode.AddIns.Core;
using JointCode.AddIns.Mvc.System;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace JointCode.AddIns.Mvc
{
    public static class JcMvc
    {
        class MvcAssemblyLoadPolicy : AssemblyLoadPolicy
        {
            public MvcAssemblyLoadPolicy()
            {
                var shadowCopyDirectory = AddinFileSettings.DefaultAddinDataDirectory + "\\Assemblies";
                var privateAssemblyProbingDirectories = new[] { shadowCopyDirectory };
                Initialize(true, shadowCopyDirectory, privateAssemblyProbingDirectories);
            }

            public override AssemblyLoadMethod GetAssemblyLoadMethod(Addin addin)
            {
                // 插件程序集加载必须使用 LoadFile 或 LoadFrom。LoadBytes 是不行的。
                return AssemblyLoadMethod.LoadFrom;
            }
        }

        static AddinEngine _addinEngine;

        public static AddinEngine AddinEngine
        {
            get { return _addinEngine; }
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Initialize()
        {
            DoInitialize();

            //注册插件控制器工厂。
            ControllerBuilder.Current.SetControllerFactory(new JcMvcControllerFactory());

            //注册插件模板引擎。 
            //ViewEngines.Engines.Insert(0, new JcMvcRazorViewEngine());
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new JcMvcRazorViewEngine());
        }

        static void DoInitialize()
        {
            //var trustLevel = WebHelper.GetTrustLevel();

            if (_addinEngine != null)
                return;

            var fileSettings = new AddinFileSettings(AddinFileSettings.DefaultAddinDataDirectory, new[] {"bin"}, new[] {AddinFileSettings.DefaultAddinProbingDirectory});
            var addinOptions = AddinOptions.Create().WithAssemblyLoadPolicy(new MvcAssemblyLoadPolicy()).WithFileSettings(fileSettings);
             _addinEngine = new AddinEngine(addinOptions);

            _addinEngine.Initialize(true);

            var addins = _addinEngine.GetAllAddins();
            foreach (var addin in addins)
                PrepareAddin(addin);

            _addinEngine.Start();

            //addins = _addinEngine.GetStartedAddins();
            ////var probingPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            //foreach (var addin in addins)
            //{
            //    //probingPath += addin.File.BaseDirectory + ";";
            //    //var asms = addin.Runtime.LoadAssemblies();
            //    //foreach (var asm in asms)
            //    //    BuildManagerHelper.AddReferencedAssemblyNormally(asm);
            //}
            ////AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = probingPath;

            //var menustrip = new MvcMenuStrip();
            //_addinEngine.LoadExtensionPoint(menustrip);
            //_addinEngine.Framework.SetProperty("MvcMenuStrip", menustrip);
        }

        static void PrepareAddin(Addin addin)
        {
            addin.StatusChanged += OnAddinStatusChanged;
        }

        const string PrivateAreaRegistrations = "PrivateAreaRegistrations";

        static void OnAddinStatusChanged(object sender, AddinStatusChangedEventArgs args)
        {
            if (args.AddinStatus == AddinStatus.Started)
                OnAddinStarted(args.Addin);
            else if (args.AddinStatus == AddinStatus.Stopping)
                OnAddinStopping(args.Addin);
        }

        static void OnAddinStarted(Addin addin)
        {
            //// 添加私有程序集探测路径
            //var probingPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath ?? string.Empty;
            //probingPath += args.Addin.File.BaseDirectory + ";";
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = probingPath;

            // 添加路由
            var asms = addin.Runtime.LoadAssemblies();
            JointCode.AddIns.Mvc.System.AreaRegistration areaReg = null; // private AreaRegistration implementations
            foreach (var asm in asms)
            {
                Type[] types;
                if (!JcMvcUtil.TryGetTypes(asm, out types))
                    continue;
                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    if (!typeof(JointCode.AddIns.Mvc.System.AreaRegistration).IsAssignableFrom(type))
                        continue;
                    var ar = Activator.CreateInstance(type) as JointCode.AddIns.Mvc.System.AreaRegistration;
                    if (ar != null && areaReg != null)
                        throw new ConfigurationException(string.Format("More than one private area registrations has been found in addin [{0}]!", addin.Header.Name));
                    areaReg = ar;
                    AreaRegistrationHelper.RegisterArea(ar, null);
                }
            }
            if (areaReg != null)
            {
                var key = addin.Header.Name + "/" + PrivateAreaRegistrations;
                if (addin.Context.Framework.ContainsPropertyKey(PrivateAreaRegistrations))
                    throw new ConfigurationException(string.Format(
                        "The private area registration key [{0}] for addin [{1}] has been taken, please use another area name and try again!", key, addin.Header.Name));
                addin.Context.Framework.SetProperty(key, areaReg);
            }
            else
            {
                AreaRegistrationHelper.RegisterArea(addin.Header.Name, null); // 添加路由
            }

            // 添加程序集引用
            foreach (var asm in asms)
                BuildManagerHelper.AddReferencedAssembly(asm);
        }

        static void OnAddinStopping(Addin addin)
        {
            // 删除路由
            object ar;
            var key = addin.Header.Name + "/" + PrivateAreaRegistrations;
            if (addin.Context.Framework.TryGetProperty(key, out ar))
            {
                var privateAreaReg = ar as JointCode.AddIns.Mvc.System.AreaRegistration;
                if (privateAreaReg != null)
                    AreaRegistrationHelper.UnregisterArea(privateAreaReg);
                else
                    AreaRegistrationHelper.UnregisterArea(addin.Header.Name);
            }
            else
            {
                AreaRegistrationHelper.UnregisterArea(addin.Header.Name);
            }

            // 删除程序集引用
            foreach (var asm in addin.Runtime.LoadedAssemblies)
                BuildManagerHelper.RemoveReferencedAssembly(asm);

            //// 删除私有程序集探测路径
            //var probingPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            //probingPath.Replace(args.Addin.File.BaseDirectory + ";", string.Empty);
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = probingPath; }
        }
    }
}