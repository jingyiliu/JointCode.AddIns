using JointCode.AddIns.Extension;
using JointCode.AddIns.Mvc.UiElements;

namespace JointCode.AddIns.Mvc.Extension
{
    public abstract class JcMvcMenuItemExtensionBuilder<TExtension> : ICompositeExtensionBuilder<TExtension> where TExtension : JcMvcMenuItem
    {
        TExtension _menuItem;

        //public string Text { get; set; }
        //public string Action { get; set; }
        //public string Controller { get; set; }
        ////public string TagName { get; set; }
        ////public string Class { get; set; }
        ////public string Id { get; set; }
        //public bool Visible { get; set; }

        public TExtension BuildExtension(IAddinContext adnContext)
        {
            if (_menuItem != null)
                return _menuItem;
            _menuItem = BuildMenuItem(adnContext);
            return _menuItem;
        }

        protected abstract TExtension BuildMenuItem(IAddinContext adnContext);

        public void AddChildExtension(TExtension child)
        {
            _menuItem.AddChild(child);
        }

        public void InsertChildExtension(int index, TExtension child)
        {
            _menuItem.InsertChild(index, child);
        }

        public void RemoveChildExtension(TExtension child)
        {
            _menuItem.RemoveChild(child);
        }
    }
}