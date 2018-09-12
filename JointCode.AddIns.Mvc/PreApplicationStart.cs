//[assembly: System.Web.PreApplicationStartMethod(typeof(JointCode.AddIns.Mvc.PreApplicationStart), "Initialize")]

namespace JointCode.AddIns.Mvc
{
    public class PreApplicationStart
    {
        public static void Initialize() { JcMvc.Initialize(); }
    }
}
