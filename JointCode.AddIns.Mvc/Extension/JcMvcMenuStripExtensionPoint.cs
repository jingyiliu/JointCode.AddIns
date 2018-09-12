using JointCode.AddIns.Extension;
using JointCode.AddIns.Mvc.UiElements;

namespace JointCode.AddIns.Mvc.Extension
{
    public class JcMvcMenuStripExtensionPoint<TExtension, TExtensionRoot> : IExtensionPoint<TExtension, TExtensionRoot> 
        where TExtension : JcMvcMenuItem
        where TExtensionRoot : JcMvcMenuStrip<TExtension>
    {
        TExtensionRoot _root;

        public TExtensionRoot Root { set { _root = value; } }

        public void AddChildExtension(TExtension child)
        {
            _root.AddChild(child);
        }

        public void InsertChildExtension(int index, TExtension child)
        {
            _root.InsertChild(index, child);
        }

        public void RemoveChildExtension(TExtension child)
        {
            _root.RemoveChild(child);
        }
    }
}
