using System.Collections.Generic;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Mvc.UiElements
{
    public abstract class JcMvcMenuItem
    {
        List<JcMvcMenuItem> _children;

        //public string Area { get; set; }
        //public string Text { get; set; }
        //public string Action { get; set; }
        //public string Controller { get; set; }
        ////public string Class { get; set; }
        ////public string Id { get; set; }
        //public bool Visible { get; set; }

        public int ChildCount { get { return _children == null ? 0 : _children.Count; } }

        public IEnumerable<JcMvcMenuItem> Children { get { return _children; } }

        public JcMvcMenuItem this[int i] { get { return _children[i]; } }

        public void AddChild(JcMvcMenuItem child)
        {
            Requires.Instance.NotNull(child, "child");
            _children = _children ?? new List<JcMvcMenuItem>();
            _children.Add(child);
        }

        public void InsertChild(int index, JcMvcMenuItem child)
        {
            Requires.Instance.NotNull(child, "child");
            _children = _children ?? new List<JcMvcMenuItem>();
            _children.Insert(index, child);
        }

        public bool RemoveChild(JcMvcMenuItem child)
        {
            Requires.Instance.NotNull(child, "child");
            return _children == null ? false : _children.Remove(child);
        }

        public virtual string GetHtmlString()
        {
            return string.Empty;
        }
    }
}
