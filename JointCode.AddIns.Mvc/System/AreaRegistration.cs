using System.Web.Mvc;
using System.Web.Routing;

namespace JointCode.AddIns.Mvc.System
{
    public abstract class AreaRegistration
    {
        public abstract string AreaName { get; }
        public abstract void RegisterArea(AreaRegistrationContext context);
        public abstract void UnregisterArea(RouteCollection routes);
    }
}