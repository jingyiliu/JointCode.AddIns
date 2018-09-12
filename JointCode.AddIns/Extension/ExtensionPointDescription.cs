namespace JointCode.AddIns.Extension
{
    public class ExtensionPointDescription
    {
        public bool Loaded { get; internal set; }

        // Guid 可以在不同应用程序之间唯一地标识一个 Addin，而由不同 Addin 提供的所有 ExtensionPoint 在一个应用程序中必须唯一。
        /// <summary>
        /// Gets the name of <see cref="IExtensionPoint"/> that uniquely identify an <see cref="IExtensionPoint"/> within an application.
        /// </summary>
        public string Name { get; internal set; }

        public string Description { get; internal set; }

        // The type name of the IExtensionPoint/IExtensionBuilder
        public string TypeName { get; internal set; }
    }
}