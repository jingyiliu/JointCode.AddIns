//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns.Extension
{
    /// <summary>
    /// Represnent an extension builder.
    /// </summary>
    public interface IExtensionBuilder
    {
    }

    /// <summary>
    /// Represnent a composite extension builder.
    /// </summary>
    public interface ICompositeExtensionBuilder : IExtensionBuilder
    {
    }

    /// <summary>
    /// The <see cref="IExtensionBuilder"/> is responsible for creating extension object. 
    /// It must define everything it need to build extension objects in it's public gettable/settable properties.
    /// </summary>
    /// <typeparam name="TExtension">The extension type.</typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///public class ToolStripComboBoxExtensionBuilder : IExtensionBuilder<ToolStripItem>
    ///{
    ///    public string Name { get; set; }
    ///    public ToolStripItem BuildExtension(IAddinContext adnContext)
    ///    {
    ///        return new ToolStripComboBox { Text = Name };
    ///    }
    ///}
    ///]]>
    /// </code>
    /// </example>
    public interface IExtensionBuilder<TExtension> : IExtensionBuilder
    {
        TExtension BuildExtension(IAddinContext adnContext);
    }

    /// <summary>
    /// The <see cref="IExtensionBuilder"/> is responsible for creating extension object. 
    /// It must define everything it need to build extension objects in it's public gettable/settable properties.
    /// </summary>
    /// <typeparam name="TExtension">The extension type.</typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///public class ToolStripMenuItemExtensionBuilder : ICompositeExtensionBuilder<ToolStripItem>
    ///{
    ///    ToolStripMenuItem _menu;
    ///    IAddinContext _adnContext;
    ///    [ExtensionProperty(Required = true)]
    ///    public string Name { get; set; }
    ///    public string Tooltip { get; set; }
    ///    public AddinTypeHandle CommandType { get; set; }
    ///    public void AddChildExtension(ToolStripItem child)
    ///    {
    ///        _menu.DropDownItems.Add(child);
    ///    }
    ///    public void InsertChildExtension(int idx, ToolStripItem child)
    ///    {
    ///        _menu.DropDownItems.Insert(idx, child);
    ///    }
    ///    public void RemoveChildExtension(ToolStripItem child)
    ///    {
    ///        _menu.DropDownItems.Remove(child);
    ///    }
    ///    public ToolStripItem BuildExtension(IAddinContext adnContext)
    ///    {
    ///        if (_menu != null)
    ///            return _menu;
    ///        _adnContext = adnContext;
    ///        var menu = new ToolStripMenuItem();
    ///        menu.Text = Name;
    ///        menu.ToolTipText = Tooltip;
    ///        if (CommandType != null)
    ///            menu.Click += OnMenuClick; // lazy loading type into the runtime
    ///        _menu = menu;
    ///        return menu;
    ///    }
    ///    void OnMenuClick(object sender, EventArgs e)
    ///    {
    ///        var type = _adnContext.Addin.Runtime.GetType(CommandType);
    ///        var command = (IRootCommand)Activator.CreateInstance(type);
    ///        command.Run();
    ///    }
    ///}
    ///]]>
    /// </code>
    /// </example>
    public interface ICompositeExtensionBuilder<TExtension> : IExtensionBuilder<TExtension>, ICompositeExtensionBuilder
    {
        /// <summary>
        /// Adds a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void AddChildExtension(TExtension child);
        /// <summary>
        /// Inserts a child extension.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="child">The child.</param>
        void InsertChildExtension(int index, TExtension child);
        /// <summary>
        /// Removes a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void RemoveChildExtension(TExtension child);
    }
}
