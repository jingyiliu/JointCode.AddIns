//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

namespace JointCode.AddIns
{
    /// <summary>
    /// An IExtensionBuilder
    /// </summary>
    /// <example>
    ////public class MenuTemplate : IComplexExtensionTemplate<ToolStripItem>
    ////{
    ////    string _friendName;
    ////    string _tooltip;
    ////    string _commandType;
    ////    ToolStripMenuItem _menu;
    ////    public string FriendName
    ////    {
    ////        get { return _friendName; }
    ////        set { _friendName = value; }
    ////    }
    ////    public string Tooltip
    ////    {
    ////        get { return _tooltip; }
    ////        set { _tooltip = value; }
    ////    }
    ////    public string CommandType
    ////    {
    ////        get { return _commandType; }
    ////        set { _commandType = value; }
    ////    }
    ////    public ToolStripItem BuildExtension()
    ////    {
    ////        if (_menu == null)
    ////        {
    ////            ToolStripMenuItem menu = new ToolStripMenuItem(_friendName);
    ////            menu.ToolTipText = _tooltip;
    ////            menu.Click += OnMenuClick;
    ////            _menu = menu;
    ////        }
    ////        return _menu;
    ////    }
    ////    public void AddChildExtension(ToolStripItem child)
    ////    {
    ////        _menu.DropDownItems.Add(child);
    ////    }
    ////    public void InsertChildExtension(int idx, ToolStripItem child)
    ////    {
    ////        _menu.DropDownItems.Insert(idx, child);
    ////    }
    ////    public void RemoveChildExtension(ToolStripItem child)
    ////    {
    ////        _menu.DropDownItems.Remove(child);
    ////    }
    ////    void OnMenuClick(object sender, EventArgs e)
    ////    {
    ////        ICommand command = (ICommand)IAddinContext.CreateInstance(_commandType);
    ////        command.Run();
    ////    }
    ////    public IAddinContext IAddinContext { get; set; }
    ////}
    /// </example>
    public interface IExtensionBuilder
    {
    }

    public interface ICompositeExtensionBuilder : IExtensionBuilder
    {
    }

    /// <summary>
    /// The <see cref="IExtensionBuilder"/> represent a template of extension model in the addin manifest, which provides 
    /// everything needed to build an extension object, including any event handlers it needs.
    /// For example, a ToolStripMenuItem extension might need to specify its Text, Icon, and Shortcut properties, these information can be obtained from the 
    /// <see cref="Attribute"/> subclass at the runtime. Also, it might need some kind of event handling, this is implemented with an interface (such as an
    /// ICommand interface) declared by the Attribute subclass itself. The Attribute subclass can instantiate an ICommand object using the new operator in its 
    /// implementation (such as ICommand command = new CommandImplementation), thus minimize Reflection usage.
    /// </summary>
    /// <typeparam name="TExtension"></typeparam>
    /// <remarks>
    /// Besides the <see cref="IExtensionDescriptor"/>, the type parameter <see cref="TExtension"/> will also be used as a contract to match and validate the 
    /// extension and its parent container (the <see cref="IExtensionPoint"/> or an <see cref="ICompositeExtensionBuilder{TExtension}"/>).
    /// 
    /// The type parameter <see cref="TExtension"/> can be:
    /// 1. An interface, thus we can use custom attribute, <see cref="IExtensionActivator"/> or xml to add extensions; 
    /// 2. A class, but that way the user can not use the Attribute to add extensions.
    /// 
    /// The object <see cref="ExtensionItem"/> can represent:
    /// 1. The concrete extension object, such as a ToolStripMenuItem instance, in which case the inherited class of this interface might need to provide 
    ///    the data used to create this ToolStripMenuItem instance, such as IconFile, ToolTip, Label, EventHandlerTypeName...
    /// 2. The data needed to build the extension object (for example, an object that wraps the text, icon file and the tooltips to build a ToolStripMenuItem 
    ///    object), then we provide a naturally support for MVC pattern.
    /// 3. An ICommand implementation used to perform some tasks that does not need any data to build.
    /// </remarks>
    public interface IExtensionBuilder<TExtension> : IExtensionBuilder
    {
        /// <summary>
        /// This property represent an <see cref="ExtensionItem"/> instance.
        /// Normally, it's an ExtensionSerializer instance, which provides data (such as IconFile, ToolTipText...) used to create a concrete 
        /// extension (such as ToolStripMenuItem), and in most cases, it will implement IFileSerializer.
        /// </summary>
        /// <remarks>
        /// There are 3 cases:
        /// 1. If the data comes from manifest, it will be wrapped with a ManifestReader which implements the BinFileReader.
        /// 2. If the data comes from custom Attribute, it will be wrapped with an AttributeReader inherit from BinFileReader, which will
        ///    collect the data (all properties defined in the Attribute by default, except the ones marked with NonDataAttribute) 
        ///    dynamically by using Reflection.
        /// 3. If the data comes from <see cref="IExtensionActivator"/>, the case is like above.
        /// Tips: We can determine whether to reuses the created instance on every request or not.
        /// </remarks>
        TExtension BuildExtension(IAddinContext adnContext);
    }

    /// <summary>
    /// The IComplexExtensionTemplate
    /// </summary>
    /// <typeparam name="TExtension">The contract type.</typeparam>
    /// <remarks>
    /// The TExtension will be used as a contract between the child <see cref="ExtensionItem"/> object and its parent container,
    /// the system will use this contract to get the right parent container and use an <see cref="IExtensionNodePositioner"/> object to 
    /// determine the addinIndex of the current child extension in its parent container.
    /// </remarks>
    public interface ICompositeExtensionBuilder<TExtension> : IExtensionBuilder<TExtension>, ICompositeExtensionBuilder
    {
        /// <summary>
        /// Loads a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void AddChildExtension(TExtension child);
        /// <summary>
        /// Inserts the child extension.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="child">The child.</param>
        void InsertChildExtension(int index, TExtension child);
        /// <summary>
        /// Unloads a child extension.
        /// </summary>
        /// <param name="child">The child extension.</param>
        void RemoveChildExtension(TExtension child);
    }
}
