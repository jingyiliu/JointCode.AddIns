using JointCode.AddIns.Mvc.UiElements;
using System.Web.Mvc;

namespace SampleSite.MenuDefinition
{
    public class MvcMenuItem : JcMvcMenuItem
    {
        public string Url { get; set; }
        public string Text { get; set; }
        //public string Class { get; set; }
        //public string Id { get; set; }
        public bool Visible { get; set; }

        // this extension is going to used in the _Layout.cshtml of web project (SampleSite)
        public override string GetHtmlString()
        {
            var li = new TagBuilder("li");

            var a = new TagBuilder("a");
            a.MergeAttribute("href", Url);
            a.SetInnerText(Text);

            li.InnerHtml = a.ToString(TagRenderMode.Normal);
            if (!Visible)
                li.AddCssClass("hidden");

            return li.ToString(TagRenderMode.Normal);
        }
    }

    public class MvcMenuStrip : JcMvcMenuStrip<MvcMenuItem>
    {
        public override string GetHtmlString()
        {
            if (ChildCount == 0)
                return null;

            // assume that we only have one level of menu items.
            var result = string.Empty;
            foreach (var child in Children)
                result += child.GetHtmlString();

            return result;
        }
    }
}
