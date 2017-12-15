//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System.ComponentModel.Design;
using JointCode.AddIns.Core;
using JointCode.AddIns.Core.Runtime;
using JointCode.Common.Logging;

namespace JointCode.AddIns
{
    //public interface IServiceContext
    //{
    //    AddinInfo AddinInfo { get; }
    //    IAddin Addin { get; }
    //    IServiceManager ServiceManager { get; }
    //    IAddinManager AddinManager { get; }
    //    IEventAggregator EventAggregator { get; }
    //    ILogger Logger { get; }
    //    IStringLocalizer StringLocalizer { get; }

    //    //event Activated { get; }
    //    //event Deactivated { get; }
    //    //event ServiceChanged { get; }

    //    #region Services

    //    // The services will not be registered to the IoC container directly, 
    //    // instead they will be registered to an addin service pool (using an
    //    // JointCode.AddIns framework private class, IoCData, for example), and then 
    //    // when they are first requested, the JointCode.AddIns framework will read
    //    // the IoCData to get the registration configurations and register the
    //    // service to the IoC container actually. 
    //    // As such, the RegisterService method should not create any dependencies
    //    // between addins.

    //    void RegisterService(Type serviceType);
    //    void UnregisterService(Type serviceType);
    //    object GetService(Type serviceType);
    //    object[] GetServices(Type serviceType);

    //    void RegisterExtension(ExtensionData extensionData);
    //    void UnregisterExtension(ExtensionData extensionData);

    //    #endregion
    //}

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAddinContext
    {
        RuntimeSystem RuntimeSystem { get; }
        AddinFileSystem AddinFileSystem { get; }
        //    AddinInfo AddinInfo { get; }
        //    IAddin Addin { get; }
        //    IServiceManager ServiceManager { get; }
        //    IAddinManager AddinManager { get; }
        //    IEventAggregator EventAggregator { get; }
        //    ILogger Logger { get; }
        //    IStringLocalizer StringLocalizer { get; }
        //IAddinManager AddinManager { get; }
        //AddinSystem AddinSystem { get; }
        IServiceContainer ServiceContainer { get; }
        //IEventAggregator EventAggregator { get; }
        ILogger Logger { get; }
        //IStringLocalizer StringLocalizer { get; }

        //event Activated { get; }
        //event Deactivated { get; }
        //event ServiceChanged { get; }

        // Register the services first, and then ExtensionData. This is because 
        // nothing should depend on an ExtensionData, but an ExtensionData might
        // depends on other services.

        #region Services

        // The services will not be registered to the IoC container directly, 
        // instead they will be registered to an addin service pool (using an
        // JointCode.AddIns framework private class, ServiceData, for example), and then 
        // when they are first requested, the JointCode.AddIns framework will read
        // the ServiceData to get the registration configurations and register the
        // service to the IoC container actually. 
        // As such, the RegisterService method should not create any dependencies
        // between addins.

        //        void RegisterService(Type serviceType);
        //        void UnregisterService(Type serviceType);
        //        object GetService(Type serviceType);
        //        object[] GetServices(Type serviceType); 

        #endregion

        //        void SubscribeExtensionEvent(string extensionPath, Action<ExtensionEventArgs> eventHandler);
        //        void UnsubscribeExtensionEvent(string extensionPath);
        //        void PublishExtensionEvent(ExtensionEventArgs args);
    }
}
