//using System;
//using System.Linq;
//using System.Web.Mvc;
//using System.Web.Routing;

//namespace JointCode.AddIns.Mvc.System
//{
//    public static class JcRouteExtensions
//    {
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url)
//        {
//            return routes.MapJcRoute(name, url, null, null);
//        }
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url, object defaults)
//        {
//            return routes.MapJcRoute(name, url, defaults, null);
//        }
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url, string[] namespaces)
//        {
//            return routes.MapJcRoute(name, url, null, null, namespaces);
//        }
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
//        {
//            return routes.MapJcRoute(name, url, defaults, constraints, null);
//        }
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
//        {
//            return routes.MapJcRoute(name, url, defaults, null, namespaces);
//        }
//        public static JcRoute MapJcRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
//        {
//            if (routes == null) throw new ArgumentNullException("routes");
//            if (url == null) throw new ArgumentNullException("url");

//            var route = new JcRoute(url, new MvcRouteHandler());
//            route.Defaults = new RouteValueDictionary(defaults);
//            route.Constraints = new RouteValueDictionary(constraints);
//            route.DataTokens = new RouteValueDictionary();
//            if ((namespaces != null) && (namespaces.Length > 0))
//                route.DataTokens["Namespaces"] = namespaces;
//            routes.Add(name, route);

//            return route;
//        }

//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url)
//        {
//            return context.MapJcRoute(name, url, null);
//        }
//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url, object defaults)
//        {
//            return context.MapJcRoute(name, url, defaults, null);
//        }
//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url, string[] namespaces)
//        {
//            return context.MapJcRoute(name, url, null, namespaces);
//        }
//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url, object defaults, object constraints)
//        {
//            return context.MapJcRoute(name, url, defaults, constraints, null);
//        }
//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url, object defaults, string[] namespaces)
//        {
//            return context.MapJcRoute(name, url, defaults, null, namespaces);
//        }
//        public static JcRoute MapJcRoute(this AreaRegistrationContext context, string name, string url, object defaults, object constraints, string[] namespaces)
//        {
//            if ((namespaces == null) && (context.Namespaces != null))
//                namespaces = context.Namespaces.ToArray();
//            var route = context.Routes.MapJcRoute(name, url, defaults, constraints, namespaces);
//            route.DataTokens["area"] = context.AreaName;
//            bool flag = (namespaces == null) || (namespaces.Length == 0);
//            route.DataTokens["UseNamespaceFallback"] = flag;
//            return route;
//        }
//    }
//}
