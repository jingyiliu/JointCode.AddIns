using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace JointCode.AddIns.Mvc.System
{
    static class JcMvcUtil
    {
        public static string GetAddinName(RequestContext requestContext)
        {
            object addinName;
            return requestContext.RouteData.DataTokens.TryGetValue("area", out addinName) ? addinName as string : null;
        }

        public static string GetAddinName(ControllerContext controllerContext)
        {
            object addinName;
            return controllerContext.RouteData.DataTokens.TryGetValue("area", out addinName) ? addinName as string : null;
        }

        public static bool TryGetTypes(Assembly assembly, out Type[] result)
        {
            try
            {
                result = assembly.GetTypes();
                return true;
            }
            catch (Exception e)
            {
                // 在某些情况下，会出现“无法加载一个或多个请求的类型。有关更多信息，请检索 LoaderExceptions 属性”异常。
                // 这多半是由于关联 dll 的问题造成的， 可能是反射的dll代码还依赖于其他 dll，但是并没有提供，或者 dll 有更新，但是没有提供最新的dll引起的，此处我们直接跳过。
                result = null;
                return false;
            }
        }

        ///// <summary>
        ///// 将Web站点下的绝对路径转换为相对于指定页面的虚拟路径
        ///// </summary>
        ///// <param name="page">当前页面指针，一般为this</param>
        ///// <param name="specifiedPath">绝对路径</param>
        ///// <returns>虚拟路径, 型如: ../../</returns>
        //public static string ConvertSpecifiedPathToRelativePathForPage(Page page, string specifiedPath)
        //{
        //    // 根目录虚拟路径
        //    string virtualPath = page.Request.ApplicationPath;
        //    // 根目录绝对路径
        //    string pathRooted = HostingEnvironment.MapPath(virtualPath);
        //    // 页面虚拟路径
        //    string pageVirtualPath = page.Request.Path;
        //    if (!Path.IsPathRooted(specifiedPath) || specifiedPath.IndexOf(pathRooted) == -1)
        //    {
        //        throw new Exception(string.Format("[{0}] 是虚拟路径而不是绝对路径!", specifiedPath));
        //    }
        //    // 转换成相对路径 
        //    //(测试发现，pathRooted 在 VS2005 自带的服务器跟在IIS下根目录或者虚拟目录运行似乎不一样,
        //    // 有此地方后面会加"/", 有些则不会, 为保险起见判断一下)
        //    if (pathRooted.Substring(pathRooted.Length - 1, 1) == "//")
        //    {
        //        specifiedPath = specifiedPath.Replace(pathRooted, "/");
        //    }
        //    else
        //    {
        //        specifiedPath = specifiedPath.Replace(pathRooted, "");
        //    }
        //    string relativePath = specifiedPath.Replace("//", "/");
        //    string[] pageNodes = pageVirtualPath.Split('/');
        //    // 减去最后一个页面和前面一个 "" 值
        //    int pageNodesCount = pageNodes.Length - 2;
        //    for (int i = 0; i < pageNodesCount; i++)
        //    {
        //        relativePath = "/.." + relativePath;
        //    }
        //    if (pageNodesCount > 0)
        //    {
        //        // 如果存在 ".." , 则把最前面的 "/" 去掉
        //        relativePath = relativePath.Substring(1, relativePath.Length - 1);
        //    }
        //    return relativePath;
        //}

        ///// <summary>
        ///// 将Web站点下的绝对路径转换为虚拟路径
        ///// 注：非Web站点下的则不转换
        ///// </summary>
        ///// <param name="page">当前页面指针，一般为this</param>
        ///// <param name="specifiedPath">绝对路径</param>
        ///// <returns>虚拟路径, 型如: ~/</returns>
        //public static string ConvertSpecifiedPathToRelativePath(Page page, string specifiedPath)
        //{
        //    string virtualPath = page.Request.ApplicationPath;
        //    string pathRooted = HostingEnvironment.MapPath(virtualPath);
        //    if (!Path.IsPathRooted(specifiedPath) || specifiedPath.IndexOf(pathRooted) == -1)
        //    {
        //        return specifiedPath;
        //    }
        //    if (pathRooted.Substring(pathRooted.Length - 1, 1) == "//")
        //    {
        //        specifiedPath = specifiedPath.Replace(pathRooted, "~/");
        //    }
        //    else
        //    {
        //        specifiedPath = specifiedPath.Replace(pathRooted, "~");
        //    }
        //    string relativePath = specifiedPath.Replace("//", "/");
        //    return relativePath;
        //}
    }
}