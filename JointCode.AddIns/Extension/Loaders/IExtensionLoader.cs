//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

namespace JointCode.AddIns.Extension.Loaders
{
    interface IExtensionLoader<TExtension> : ILoader
    {
        //TExtension Extension { get; }
        //ICompositeExtensionLoader<TExtension> Parent { get; set; }
        TExtension GetOrCreateExtension(IAddinContext context);
        //void NotifyExtensionChange(ExtensionChange changeType);
    }

    interface ICompositeExtensionLoader : ILoader
    {
        void AddChild(ExtensionLoader extLoader);
        void InsertChild(int index, ExtensionLoader extLoader);
        void RemoveChild(ExtensionLoader extLoader);
    }

    interface ICompositeExtensionLoader<TExtension> : ICompositeExtensionLoader
    {
        void LoadChild(IAddinContext context, IExtensionLoader<TExtension> extLoader);
        void LoadChild(IAddinContext context, int index, IExtensionLoader<TExtension> extLoader);
        void UnloadChild(IAddinContext context, IExtensionLoader<TExtension> extLoader);
    }
}
