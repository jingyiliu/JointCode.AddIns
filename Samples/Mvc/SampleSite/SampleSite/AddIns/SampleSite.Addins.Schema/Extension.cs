using JointCode.AddIns;
using JointCode.AddIns.Mvc.Extension;
using SampleSite.MenuDefinition;

namespace SampleSite.Addins.Schema
{
    public class MvcMenuItemExtensionBuilder : JcMvcMenuItemExtensionBuilder<MvcMenuItem>
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; }

        protected override MvcMenuItem BuildMenuItem(IAddinContext adnContext)
        {
            return new MvcMenuItem
            {
                Url = this.Url,
                Text = this.Text,
                Visible = this.Visible
            };
        }
    }

    public class MvcMenuStripExtensionPoint : JcMvcMenuStripExtensionPoint<MvcMenuItem, MvcMenuStrip>
    {
    }
}
