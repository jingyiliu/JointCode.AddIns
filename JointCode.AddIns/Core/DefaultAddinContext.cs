//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//


using System;
using System.Collections.Generic;
using JointCode.Common;
using JointCode.Events;

namespace JointCode.AddIns.Core
{
    class DefaultAddinContext : IAddinContext
    {
        readonly AddinFramework _addinFramework;
        readonly Addin _addin;

        internal DefaultAddinContext(AddinFramework addinFramework, Addin addin)
        {
            _addinFramework = addinFramework;
            _addin = addin;
        }

        public Addin Addin { get { return _addin; } }
        public AddinFramework Framework { get { return _addinFramework; } }

        public ServiceHandle AddService(Type serviceType, object serviceInstance)
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.AddService(serviceType, serviceInstance);
        }

        public object GetService(Type serviceType)
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.GetService(serviceType);
        }

        public List<object> GetServices(Type serviceType)
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.GetServices(serviceType);
        }

        public ServiceHandle AddService<T>(MyFunc<T> instanceCreator)
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.AddService(instanceCreator);
        }

        public T GetService<T>()
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.GetService<T>();
        }

        public List<T> GetServices<T>()
        {
            VerfifyAddinStarted();
            return _addinFramework.ServiceProvider.GetServices<T>();
        }

        public void RemoveService(ServiceHandle serviceHandle)
        {
            VerfifyAddinStarted();
            _addinFramework.ServiceProvider.RemoveService(serviceHandle);
        }

        public EventPublication AddAndGetEventPublication(string uri, Type arg1Type, Type arg2Type)
        {
            VerfifyAddinStarted();
            return _addinFramework.EventBroker.AddAndGetEventPublication(uri, arg1Type, arg2Type);
        }

        public EventPublication<TEventArgs1, TEventArgs2> AddAndGetEventPublication<TEventArgs1, TEventArgs2>(string uri) where TEventArgs1 : class where TEventArgs2 : class
        {
            VerfifyAddinStarted();
            return _addinFramework.EventBroker.AddAndGetEventPublication<TEventArgs1, TEventArgs2>(uri);
        }

        public void RemoveEventPublication(EventPublication publication)
        {
            VerfifyAddinStarted();
            _addinFramework.EventBroker.RemoveEventPublication(publication);
        }

        public PublicationToken AddEventPublication(string uri, object eventSource, string eventName)
        {
            VerfifyAddinStarted();
            return _addinFramework.EventBroker.AddEventPublication(uri, eventSource, eventName);
        }

        public PublicationToken AddEventPublication(string uri, Type eventSourceType, string eventName)
        {
            VerfifyAddinStarted();
            return _addinFramework.EventBroker.AddEventPublication(uri, eventSourceType, eventName);
        }

        public void RemoveEventPublication(PublicationToken publicationToken)
        {
            VerfifyAddinStarted();
            _addinFramework.EventBroker.RemoveEventPublication(publicationToken);
        }

        public SubscriptionToken
            AddEventSubscription<TEventArgs1, TEventArgs2>(string uri, MyAction<TEventArgs1, TEventArgs2> action, ThreadOption threadOption) where TEventArgs1 : class where TEventArgs2 : class
        {
            VerfifyAddinStarted();
            return _addinFramework.EventBroker.AddEventSubscription(uri, action, threadOption);
        }

        public void RemoveEventSubscription(SubscriptionToken subscriptionToken)
        {
            VerfifyAddinStarted();
            _addinFramework.EventBroker.RemoveEventSubscription(subscriptionToken);
        }

        public string GetLocalizedString(string msgid)
        {
            VerfifyAddinStarted();
            return _addinFramework.StringLocalizer.GetLocalizedString(msgid);
        }

        void VerfifyAddinStarted()
        {
            if (!_addin.Started)
                throw new InvalidOperationException("Can not use the addin context object before the addin starts or after it is stopped!");
        }
    }
}