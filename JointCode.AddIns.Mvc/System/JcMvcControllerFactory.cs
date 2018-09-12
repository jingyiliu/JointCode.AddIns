using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace JointCode.AddIns.Mvc.System
{
    // 插件控制器工厂。此处可以集成 IoC 容器，以 controller 的 FullName 作为 key
    public class JcMvcControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// 根据控制器名称及请求信息获得控制器类型。
        /// </summary>
        /// <param name="requestContext">请求信息</param>
        /// <param name="controllerName">控制器名称。</param>
        /// <returns>控制器类型。</returns>
        protected override Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            var addinName = JcMvcUtil.GetAddinName(requestContext);

            if (addinName == null)
                return base.GetControllerType(requestContext, controllerName);

            var addin = JcMvc.AddinEngine.GetAddin(addinName);
            if (addin == null)
                return base.GetControllerType(requestContext, controllerName);

            var assemblies = addin.Runtime.LoadAssemblies();

            var controllerTypeName = controllerName + "Controller";

            foreach (var assembly in assemblies)
            {
                Type[] types;
                if (!JcMvcUtil.TryGetTypes(assembly, out types))
                    continue;

                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
                        continue;
                    if (controllerTypeName.Equals(type.Name, StringComparison.InvariantCultureIgnoreCase)
                        && typeof(IController).IsAssignableFrom(type))
                        return type;
                }
            }

            return base.GetControllerType(requestContext, controllerName);
        }
    }
}