//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using JointCode.Common;
using JointCode.Events;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns
{
    // In fact, this is the only way to interact with the framework API, and the framework sends these tickets to each Bundle through their BundleActivator when the Bundle launches.
    /// <summary>
    /// The <see cref="IAddinContext"/> is the bridge between the addin and addin framework.
    /// When the code needs to interact with the framework at any time, you will use the <see cref="IAddinContext"/>. 
    /// </summary>
    public interface IAddinContext
    {
        Addin Addin { get; }
        AddinFramework Framework { get; }

        //event ServiceChanged { get; }

        #region Services

        ServiceHandle AddService(Type serviceType, object serviceInstance);
        //ServiceHandle AddService(Type serviceType, MyFunc<object> instanceCreator);

        object GetService(Type serviceType);
        List<object> GetServices(Type serviceType);

        //ServiceHandle AddService<T>(T serviceInstance);
        ServiceHandle AddService<T>(MyFunc<T> instanceCreator);

        T GetService<T>();
        List<T> GetServices<T>();

        void RemoveService(ServiceHandle serviceHandle);

        #endregion

        #region Events

        // 以下方法适用于能够控制事件源代码的情况，即能够更改事件源的事件触发逻辑
        #region Publication (Controlled Event Source)

        EventPublication AddAndGetEventPublication(string uri, Type arg1Type, Type arg2Type);

        EventPublication<TEventArgs1, TEventArgs2> AddAndGetEventPublication<TEventArgs1, TEventArgs2>(string uri)
            where TEventArgs1 : class
            where TEventArgs2 : class;

        void RemoveEventPublication(EventPublication publication);

        #endregion

        // 以下方法适用于无法控制事件源代码的情况，即无法更改事件源的事件触发逻辑
        #region Publication (Uncontrolled Event Source)

        #region Instance Event

        PublicationToken AddEventPublication(string uri, object eventSource, string eventName);

        #endregion

        #region Static Event

        PublicationToken AddEventPublication(string uri, Type eventSourceType, string eventName);

        #endregion

        void RemoveEventPublication(PublicationToken publicationToken);

        #endregion

        #region Subscription (Event Handler)

        SubscriptionToken AddEventSubscription<TEventArgs1, TEventArgs2>
            (string uri, MyAction<TEventArgs1, TEventArgs2> action, ThreadOption threadOption)
            where TEventArgs1 : class
            where TEventArgs2 : class;

        void RemoveEventSubscription(SubscriptionToken subscriptionToken);

        #endregion

        #endregion

        string GetLocalizedString(string msgid);
    }
}
