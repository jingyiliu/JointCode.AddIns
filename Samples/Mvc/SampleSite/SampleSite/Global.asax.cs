using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using JointCode.AddIns.Mvc;
using SampleSite.MenuDefinition;

namespace SampleSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            JcMvc.Initialize();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            PrepareMenu();
        }

        void PrepareMenu()
        {
            var menustrip = new MvcMenuStrip();

            //var menuItem0 = new MvcMenuItem { Text = "主页", Url = "/" };
            //var menuItem1 = new MvcMenuItem { Text = "关于", Url = "/Home/About" };
            //var menuItem2 = new MvcMenuItem { Text = "联系方式", Url = "/Home/Contact" };
            //menustrip.AddChild(menuItem0);
            //menustrip.AddChild(menuItem1);
            //menustrip.AddChild(menuItem2);

            JcMvc.AddinEngine.LoadExtensionPoint(menustrip);
            //var ul = new TagBuilder("ul");
            //ul.AddCssClass("nav navbar-nav");
            //ul.SetInnerText(menustrip.GetHtmlString());
            //ul.ToString(TagRenderMode.Normal);

            JcMvc.AddinEngine.Framework.SetProperty(menustrip);
        }
    }
}
