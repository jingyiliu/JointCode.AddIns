using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace JointCode.AddIns.Mvc.System
{
    static class AreaRegistrationHelper
    {
        internal static void RegisterArea(AreaRegistration ar, object state)
        {
            var context = new AreaRegistrationContext(ar.AreaName, RouteTable.Routes, state);
            //var str = base.GetType().Namespace;
            //if (str != null)
            //    context.Namespaces.Add(str + ".*");
            ar.RegisterArea(context);
        }

        internal static void UnregisterArea(AreaRegistration ar)
        { ar.UnregisterArea(RouteTable.Routes); }

        internal static void RegisterArea(string areaName, object state)
        {
            var context = new AreaRegistrationContext(areaName, RouteTable.Routes, state);
            //var str = base.GetType().Namespace;
            //if (str != null)
            //    context.Namespaces.Add(str + ".*");
            context.MapRoute(
                areaName,
                areaName + "/{controller}/{action}/{id}",
                new { action = "Index", id = "" }
                //new string[] { "Custom.Namespace.Controllers" }
            );
        }

        internal static void UnregisterArea(string areaName)
        {
            RouteTable.Routes.Remove(RouteTable.Routes[areaName]);
        }
    }
}
