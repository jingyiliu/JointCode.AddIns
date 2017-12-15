//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.AddIns.Loading.Loaders;

namespace JointCode.AddIns
{
    /// <summary>
    /// A Condition
    /// </summary>
    /// <remarks>
    /// The <see cref="Condition"/> instance is created when needed, thus delay the loading of assembly that this <see cref="Condition"/> resides in.
    /// </remarks>
    /// <example>
    /// public class OpenFileCondition : Condition
    /// {
    ///     public OpenFileCondition()
    ///     {
    ///         // It's important to notify changes in the status of assembly _condition, to make sure the Instance points are properly updated.
    ///         TextEditorApp.OpenFileChanged += NotifyChanged;
    ///     }
    ///     public override bool Evaluate()
    ///     {
    ///         // Check against the Instance of the currently open document
    ///         return TextEditorApp.OpenFileExtension == ".xml" ? true : false;
    ///     }
    /// }
    /// </example>
    public abstract class Condition
    {
        ILoader _loader;
        /// <summary>
        /// Gets or sets the loader (an <see cref="Loading.Loaders.Loader"/> or <see cref="ExtensionPointLoader"/>) associated with this <see cref="Condition"/>.
        /// </summary>
        /// <value>
        /// The loader.
        /// </value>
        internal ILoader Loader
        {
            get { return _loader; }
            set { _loader = value; }
        }

        /// <summary>
        /// Evaluates the condition.
        /// </summary>
        /// <returns>
        /// 'true' if the condition is satisfied.
        /// </returns>
        public abstract bool Evaluate();

        /// <summary>
        /// Notifies that the condition has changed, and that it has to be re-evaluated.
        /// </summary>
        /// <remarks>
        /// This method is to be associated with some external event. It is important to avoid re-load or re-unload an fileExtension.
        /// </remarks>
        internal void NotifyChanged(IAddinContext adnContext)
        {
            if (_loader != null)
            {
                if (Evaluate())
                {
                    _loader.Load(adnContext);
                }
                else
                {
                    _loader.Unload(adnContext);
                }
            }
        }
    }
}
