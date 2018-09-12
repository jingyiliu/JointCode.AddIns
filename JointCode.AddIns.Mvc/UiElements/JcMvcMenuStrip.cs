using JointCode.Common.Helpers;
using System.Collections.Generic;

namespace JointCode.AddIns.Mvc.UiElements
{
    public abstract class JcMvcMenuStrip<TExtension>
        where TExtension : JcMvcMenuItem
    {
        List<TExtension> _children;

        public int ChildCount { get { return _children == null ? 0 : _children.Count; } }

        public IEnumerable<TExtension> Children { get { return _children; } }

        public TExtension this[int i] { get { return _children[i]; } }

        public void AddChild(TExtension child)
        {
            Requires.Instance.NotNull(child, "child");
            _children = _children ?? new List<TExtension>();
            _children.Add(child);
        }

        public void InsertChild(int index, TExtension child)
        {
            Requires.Instance.NotNull(child, "child");
            _children = _children ?? new List<TExtension>();
            _children.Insert(index, child);
        }

        public void RemoveChild(TExtension child)
        {
            Requires.Instance.NotNull(child, "child");
            if (_children != null)
                _children.Remove(child);
        }

        public virtual string GetHtmlString()
        {
            return string.Empty;
        }
    }
}