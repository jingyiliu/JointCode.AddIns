using System;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace JointCode.AddIns.Mvc.System
{
    public partial class JcMvcRazorViewEngine : RazorViewEngine
    {
        readonly string[] _baseAreaViewLocationFormats;
        readonly string[] _baseAreaMasterLocationFormats;
        readonly string[] _baseAreaPartialViewLocationFormats;
        readonly string[] _baseViewLocationFormats;
        readonly string[] _baseMasterLocationFormats;
        readonly string[] _basePartialViewLocationFormats;

        public JcMvcRazorViewEngine()
        {
            _baseAreaViewLocationFormats = AreaViewLocationFormats;
            _baseAreaMasterLocationFormats = AreaMasterLocationFormats;
            _baseAreaPartialViewLocationFormats = AreaPartialViewLocationFormats;
            _baseViewLocationFormats = ViewLocationFormats;
            _baseMasterLocationFormats = MasterLocationFormats;
            _basePartialViewLocationFormats = PartialViewLocationFormats;
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var addinName = JcMvcUtil.GetAddinName(controllerContext);

            if (addinName == null)
                return base.FindView(controllerContext, viewName, masterName, useCache);

            var addin = JcMvc.AddinEngine.GetAddin(addinName);
            if (addin == null)
                return base.FindView(controllerContext, viewName, masterName, useCache);

            var addinLocation =
                @"~\" + addin.File.BaseDirectory.Replace(HostingEnvironment.ApplicationPhysicalPath,
                    String.Empty);

            SetLocationFormats(addinLocation);
            //InjectIntoCodeGeneration(addin);
            var result = base.FindView(controllerContext, viewName, masterName, useCache);
            ResetLocationFormats();

            return result;
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var addinName = JcMvcUtil.GetAddinName(controllerContext);

            if (addinName == null)
                return base.FindPartialView(controllerContext, partialViewName, useCache);

            var addin = JcMvc.AddinEngine.GetAddin(addinName);
            if (addin == null)
                return base.FindPartialView(controllerContext, partialViewName, useCache);

            var addinLocation =
                @"~\" + addin.File.BaseDirectory.Replace(HostingEnvironment.ApplicationPhysicalPath,
                    String.Empty);

            SetLocationFormats(addinLocation);
            //InjectIntoCodeGeneration(addin);
            var result = base.FindPartialView(controllerContext, partialViewName, useCache);
            ResetLocationFormats();

            return result;
        }

        void SetLocationFormats(string addinLocation)
        {
            AreaViewLocationFormats = _baseAreaViewLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
            AreaMasterLocationFormats = _baseAreaMasterLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
            AreaPartialViewLocationFormats = _baseAreaPartialViewLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
            ViewLocationFormats = _baseViewLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
            MasterLocationFormats = _baseMasterLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
            PartialViewLocationFormats = _basePartialViewLocationFormats.Select(item => item.Replace("~", addinLocation)).ToArray();
        }

        void ResetLocationFormats()
        {
            AreaViewLocationFormats = _baseAreaViewLocationFormats;
            AreaMasterLocationFormats = _baseAreaMasterLocationFormats;
            AreaPartialViewLocationFormats = _baseAreaPartialViewLocationFormats;
            ViewLocationFormats = _baseViewLocationFormats;
            MasterLocationFormats = _baseMasterLocationFormats;
            PartialViewLocationFormats = _basePartialViewLocationFormats;
        }
    }

    //partial class JcMvcRazorViewEngine
    //{
    //    class RazorBuildProviderCodeGeneration
    //    {
    //        readonly Addin _addin;

    //        internal RazorBuildProviderCodeGeneration(Addin addin)
    //        {
    //            _addin = addin;
    //        }

    //        internal void AttachEvent()
    //        {
    //            RazorBuildProvider.CodeGenerationStarted += OnCodeGenerationStarted;
    //            //var evt = typeof(RazorBuildProvider).GetEvent("CodeGenerationStartedInternal",
    //            //    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
    //            //evt.AddEventHandler(null, new EventHandler(OnCodeGenerationStarted));
    //            //var evts = typeof(RazorBuildProvider).GetEvents(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    //            //foreach (var evt in evts)
    //            //{
    //            //}
    //        }

    //        void OnCodeGenerationStarted(object sender, EventArgs e)
    //        {
    //            var provider = (RazorBuildProvider)sender;
    //            var assemblies = _addin.Runtime.LoadAssemblies();
    //            provider.AssemblyBuilder.AddAssemblyReference(typeof(AddinEngine).Assembly);
    //            provider.AssemblyBuilder.AddAssemblyReference(this.GetType().Assembly);
    //            foreach (var assembly in assemblies)
    //                provider.AssemblyBuilder.AddAssemblyReference(assembly);
    //        }
    //    }

    //    /// <summary>
    //    /// 给运行时编译的页面加了引用程序集。
    //    /// </summary>
    //    /// <param name="addin"></param>
    //    void InjectIntoCodeGeneration(Addin addin)
    //    {
    //        var c = new RazorBuildProviderCodeGeneration(addin);
    //        c.AttachEvent();
    //    }
    //}

    ///// <summary>
    ///// ViewEngines.Engines.Add(engine);
    ///// </summary>
    //public class PrecompliedViewEngine : RazorViewEngine, IVirtualPathFactory
    //{
    //    private const string ViewsFolder = "Views";
    //    private const string SharedFolder = "Shared";
    //    private const string AreasFolder = "Areas";
    //    private readonly string[] _viewsExtension = new[] { ".cshtml" };

    //    private const string NormalViewPathFormat = "~/{0}/{1}/{{0}}{2}";
    //    private const string ModuleNormalViewPathFormat = "~/{3}/{4}/{0}/{1}/{{0}}{2}";
    //    private const string ModuleWidgetViewPathFormat = "~/{2}/{3}/{0}/{{0}}{1}";

    //    private const string NormalAreasViewPathFormat = "~/{0}/{{2}}/{1}/{2}/{{0}}{3}";
    //    private const string ModuleNormalAreasViewPathFormat = "~/{4}/{5}/{0}/{{2}}/{1}/{2}/{{0}}{3}";

    //    private static Dictionary<string, Type> PrecompliedViewTypes = new Dictionary<string, Type>();
    //    private static Type WebPageType = typeof(WebPageBase);

    //    public PrecompliedViewEngine(string moduleFolder = "Modules")
    //    {
    //        List<string> areaViewPathList = new List<string>();
    //        List<string> viewPathList = new List<string>();
    //        foreach (string ext in _viewsExtension)
    //        {
    //            areaViewPathList.Add(string.Format(NormalAreasViewPathFormat, AreasFolder, ViewsFolder, "{1}", ext));
    //            areaViewPathList.Add(string.Format(NormalAreasViewPathFormat, AreasFolder, ViewsFolder, SharedFolder, ext));

    //            viewPathList.Add(string.Format(NormalViewPathFormat, ViewsFolder, "{1}", ext));
    //            viewPathList.Add(string.Format(NormalViewPathFormat, ViewsFolder, SharedFolder, ext));
    //        }
    //        string dir = AppDomain.CurrentDomain.BaseDirectory;
    //        dir += moduleFolder;
    //        DirectoryInfo dirInfo = new DirectoryInfo(dir);
    //        if (dirInfo.Exists)
    //        {
    //            foreach (DirectoryInfo item in dirInfo.GetDirectories())
    //            {
    //                foreach (string ext in _viewsExtension)
    //                {
    //                    areaViewPathList.Add(string.Format(ModuleNormalAreasViewPathFormat, AreasFolder, ViewsFolder, "{1}", ext, moduleFolder, item.Name));
    //                    areaViewPathList.Add(string.Format(ModuleNormalAreasViewPathFormat, AreasFolder, ViewsFolder, SharedFolder, ext, moduleFolder, item.Name));

    //                    viewPathList.Add(string.Format(ModuleNormalViewPathFormat, ViewsFolder, "{1}", ext, moduleFolder, item.Name));
    //                    viewPathList.Add(string.Format(ModuleNormalViewPathFormat, ViewsFolder, SharedFolder, ext, moduleFolder, item.Name));

    //                    viewPathList.Add(string.Format(ModuleWidgetViewPathFormat, ViewsFolder, ext, moduleFolder, item.Name));
    //                }
    //            }
    //        }
    //        //init other format in list


    //        //area
    //        AreaViewLocationFormats = AreaPartialViewLocationFormats = AreaMasterLocationFormats = areaViewPathList.ToArray();

    //        //normal
    //        ViewLocationFormats = PartialViewLocationFormats = MasterLocationFormats = viewPathList.ToArray();
    //    }

    //    public static void Regist(IEnumerable<Type> types, string moduleFolder = "Modules")
    //    {
    //        if (types != null)
    //        {
    //            types.Where(type => WebPageType.IsAssignableFrom(type)).Each(type =>
    //            {
    //                var virtualPathAttrs = type.GetCustomAttributes(typeof(PageVirtualPathAttribute), false);
    //                if (virtualPathAttrs.Length > 0)
    //                {
    //                    lock (PrecompliedViewTypes)
    //                    {
    //                        string virtualPath = ((PageVirtualPathAttribute)virtualPathAttrs[0]).VirtualPath.ToUpper();
    //                        if (!PrecompliedViewTypes.ContainsKey(virtualPath))
    //                        {
    //                            PrecompliedViewTypes.Add(virtualPath, type);
    //                        }
    //                    }

    //                }
    //            });
    //        }
    //        var engine = new PrecompliedViewEngine(moduleFolder);
    //        ViewEngines.Engines.Insert(0, engine);
    //        VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
    //    }
    //    public static Type GetViewType(string virtualPath)
    //    {
    //        if (string.IsNullOrWhiteSpace(virtualPath))
    //        {
    //            return null;
    //        }
    //        virtualPath = virtualPath.ToUpper();
    //        if (PrecompliedViewTypes.ContainsKey(virtualPath))
    //        {
    //            return PrecompliedViewTypes[virtualPath];
    //        }
    //        return null;
    //    }

    //    protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
    //    {
    //        var preCompiledViewType = GetViewType(partialPath);
    //        if (preCompiledViewType != null)
    //        {
    //            return new PrecompiledView(partialPath, null, true, preCompiledViewType);
    //        }
    //        return base.CreatePartialView(controllerContext, partialPath);

    //    }
    //    protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
    //    {
    //        var preCompiledViewType = GetViewType(viewPath);
    //        if (preCompiledViewType != null)
    //        {
    //            return new PrecompiledView(viewPath, masterPath, false, preCompiledViewType);
    //        }
    //        return base.CreateView(controllerContext, viewPath, masterPath);
    //    }

    //    protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
    //    {
    //        if (GetViewType(virtualPath) != null)
    //        {
    //            return true;
    //        }
    //        return base.FileExists(controllerContext, virtualPath);
    //    }
    //    public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
    //    {
    //        if (controllerContext.RouteData.Values.ContainsKey("module"))
    //        {
    //            string virtualPath = string.Format("~/Modules/{0}/Views/{1}.cshtml",
    //                controllerContext.RouteData.Values["module"], partialViewName);

    //            if (!FileExists(controllerContext, virtualPath))
    //            {
    //                virtualPath = string.Format("~/Modules/{0}/Views/{1}/{2}.cshtml",
    //                 controllerContext.RouteData.Values["module"],
    //                 controllerContext.RouteData.Values["controller"], partialViewName);
    //                if (!FileExists(controllerContext, virtualPath))
    //                {
    //                    return base.FindPartialView(controllerContext, partialViewName, useCache);
    //                }
    //            }
    //            ViewEngineResult result = new ViewEngineResult(CreatePartialView(controllerContext, virtualPath), this);
    //            return result;
    //        }
    //        return base.FindPartialView(controllerContext, partialViewName, useCache);
    //    }
    //    public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
    //    {
    //        if (controllerContext.RouteData.Values.ContainsKey("module"))
    //        {
    //            string virtualPath = string.Format("~/Modules/{0}/Views/{1}/{2}.cshtml",
    //                controllerContext.RouteData.Values["module"],
    //            controllerContext.RouteData.Values["controller"], viewName);

    //            if (FileExists(controllerContext, virtualPath))
    //            {
    //                ViewEngineResult result = new ViewEngineResult(CreateView(controllerContext, virtualPath, masterName), this);
    //                return result;
    //            }
    //        }
    //        return base.FindView(controllerContext, viewName, masterName, useCache);
    //    }
    //    public override void ReleaseView(ControllerContext controllerContext, IView view)
    //    {
    //        base.ReleaseView(controllerContext, view);
    //    }

    //    public bool Exists(string virtualPath)
    //    {
    //        return GetViewType(virtualPath) != null;
    //    }

    //    public object CreateInstance(string virtualPath)
    //    {
    //        var viewType = GetViewType(virtualPath);
    //        if (viewType != null)
    //        {
    //            return new PrecompiledPageActivator().Create(null, viewType);
    //        }
    //        return null;
    //    }
    //}

    //public class PrecompiledView : IView
    //{
    //    private readonly string[] _viewsExtension = new[] { "cshtml" };
    //    public PrecompiledView(string virtualPath, string layoutPath, bool partialView, Type viewType)
    //    {
    //        VirtualPath = virtualPath;
    //        LayoutPath = layoutPath;
    //        PartialView = partialView;
    //        ViewType = viewType;
    //    }
    //    public string VirtualPath { get; set; }
    //    public bool PartialView { get; set; }
    //    public Type ViewType { get; set; }
    //    public string LayoutPath { get; set; }
    //    public void Render(ViewContext viewContext, TextWriter writer)
    //    {
    //        WebViewPage webViewPage = new PrecompiledPageActivator().Create(viewContext.Controller.ControllerContext, ViewType) as WebViewPage;

    //        if (webViewPage == null)
    //        {
    //            throw new InvalidOperationException("Invalid view type");
    //        }

    //        webViewPage.Layout = LayoutPath;
    //        webViewPage.VirtualPath = VirtualPath;
    //        webViewPage.ViewContext = viewContext;
    //        webViewPage.ViewData = viewContext.ViewData;
    //        webViewPage.InitHelpers();

    //        WebPageRenderingBase startPage = null;
    //        if (!this.PartialView)
    //        {
    //            startPage = StartPage.GetStartPage(webViewPage, "_ViewStart", _viewsExtension);
    //        }
    //        var pageContext = new WebPageContext(viewContext.HttpContext, webViewPage, null);
    //        webViewPage.ExecutePageHierarchy(pageContext, writer, startPage);
    //    }
    //}

    //public class PrecompiledPageActivator : IViewPageActivator
    //{
    //    public object Create(ControllerContext controllerContext, Type type)
    //    {
    //        return DependencyResolver.Current.GetService(type);
    //    }
    //}
}