using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JointCode.AddIns.Mvc.Web
{
    class WebHelper
    {
        #region .NET
        //权限列表
        static AspNetHostingPermissionLevel[] _trustLevels = new [] {
            AspNetHostingPermissionLevel.Unrestricted,
            AspNetHostingPermissionLevel.High,
            AspNetHostingPermissionLevel.Medium,
            AspNetHostingPermissionLevel.Low,
            AspNetHostingPermissionLevel.Minimal
        };

        /// <summary>
        /// 获得当前应用程序的信任级别
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            var trustLevel = AspNetHostingPermissionLevel.None;
            foreach (AspNetHostingPermissionLevel level in _trustLevels)
            {
                try
                {
                    //通过执行Demand方法检测是否抛出SecurityException异常来设置当前应用程序的信任级别
                    new AspNetHostingPermission(level).Demand();
                    trustLevel = level;
                    break;
                }
                catch (SecurityException ex)
                {
                    continue;
                }
            }
            return trustLevel;
        }

        ///// <summary>
        ///// 修改web.config文件
        ///// </summary>
        ///// <returns></returns>
        //private static bool TryWriteWebConfig()
        //{
        //    try
        //    {
        //        File.SetLastWriteTimeUtc(IOHelper.GetMapPath("~/web.config"), DateTime.UtcNow);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 修改global.asax文件
        ///// </summary>
        ///// <returns></returns>
        //private static bool TryWriteGlobalAsax()
        //{
        //    try
        //    {
        //        File.SetLastWriteTimeUtc(IOHelper.GetMapPath("~/global.asax"), DateTime.UtcNow);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 重启应用程序
        ///// </summary>
        //public static void RestartAppDomain()
        //{
        //    if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)//如果当前信任级别大于Medium，则通过卸载应用程序域的方式重启
        //    {
        //        HttpRuntime.UnloadAppDomain();
        //        TryWriteGlobalAsax();
        //    }
        //    else//通过修改web.config方式重启应用程序
        //    {
        //        bool success = TryWriteWebConfig();
        //        if (!success)
        //        {
        //            throw new Exception("修改web.config文件重启应用程序");
        //        }

        //        success = TryWriteGlobalAsax();
        //        if (!success)
        //        {
        //            throw new Exception("修改global.asax文件重启应用程序");
        //        }
        //    }

        //}

        #endregion
    }
}
