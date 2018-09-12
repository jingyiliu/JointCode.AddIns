using JointCode.Common;
using System;
using System.Collections.Generic;
using JointCode.Common.Helpers;

namespace JointCode.AddIns.Core
{
    partial class DefaultServiceProvider
    {
        abstract class ServiceRegistration
        {
            readonly ServiceHandle _serviceHandle;
            protected int _serviceCount;

            protected ServiceRegistration(ServiceHandle serviceHandle)
            {
                _serviceHandle = serviceHandle;
            }

            internal int ServiceCount { get { return _serviceCount; } }
            internal ServiceHandle ServiceHandle { get { return _serviceHandle; } }

            internal abstract object GetService();
        }

        class SimpleServiceRegistration : ServiceRegistration
        {
            readonly object _serviceInstance;
            internal SimpleServiceRegistration(ServiceHandle serviceHandle, object serviceInstance) : base(serviceHandle)
            {
                _serviceCount = 1;
                _serviceInstance = serviceInstance;
            }
            internal override object GetService()
            {
                return _serviceInstance;
            }
        }

        class SimpleFuncServiceRegistration<TContract> : ServiceRegistration
        {
            readonly MyFunc<TContract> _instanceCreator;
            internal SimpleFuncServiceRegistration(ServiceHandle serviceHandle, MyFunc<TContract> instanceCreator) : base(serviceHandle)
            {
                _serviceCount = 1;
                _instanceCreator = instanceCreator;
            }
            internal override object GetService()
            {
                return _instanceCreator();
            }
        }

        class ComplexServiceRegistration : ServiceRegistration
        {
            readonly List<ServiceRegistration> _registrations = new List<ServiceRegistration>();

            internal ComplexServiceRegistration(ServiceRegistration registration) : base(registration.ServiceHandle)
            {
                _registrations.Add(registration);
                _serviceCount = 1;
            }
            internal void AddServiceRegistration(ServiceRegistration registration)
            {
                lock (this)
                {
                    _registrations.Add(registration);
                    _serviceCount = _registrations.Count;
                }
            }
            internal void RemoveServiceRegistration(ServiceHandle serviceHandle)
            {
                lock (this)
                {
                    for (int i = 0; i < _registrations.Count; i++)
                    {
                        if (_registrations[i].ServiceHandle.Equals(serviceHandle))
                            _registrations.RemoveAt(i);
                    }
                    _serviceCount = _registrations.Count;
                }
            }
            internal override object GetService()
            {
                lock (this)
                    return _registrations[0].GetService();
            }

            internal List<object> GetServices()
            {
                var result = new List<object>(_registrations.Count);
                lock (this)
                {
                    if (_registrations.Count == 0)
                        return new List<object>();
                    foreach (var registration in _registrations)
                        result.Add(registration.GetService());
                }
                return result;
            }

            internal List<T> GetServices<T>()
            {
                var result = new List<T>(_registrations.Count);
                lock (this)
                {
                    if (_registrations.Count == 0)
                        return new List<T>();
                    foreach (var registration in _registrations)
                        result.Add((T)registration.GetService());
                }
                return result;
            }
        }
    }

    partial class DefaultServiceProvider : IServiceProvider
    {
        readonly object _syncObj = new object();
        readonly Dictionary<ServiceHandle, ServiceRegistration> _handle2Registrations = new Dictionary<ServiceHandle, ServiceRegistration>(ServiceHandle.EqualityComparer);

        public object WrappedServiceContainer { get { return this; } }

        public ServiceHandle AddService(Type contractType, object serviceInstance)
        {
            Requires.Instance.NotNull(contractType, "contractType");
            Requires.Instance.NotNull(serviceInstance, "serviceInstance");
            var handle = ServiceHandle.CreateAndAllocateId(contractType);
            return DoAddService(handle, serviceInstance);
        }

        ServiceHandle DoAddService(ServiceHandle handle, object serviceInstance)
        {
            lock (_syncObj)
            {
                ServiceRegistration registration;
                if (_handle2Registrations.TryGetValue(handle, out registration))
                {
                    if (registration.ServiceCount == 1)
                    {
                        var cRegistration = new ComplexServiceRegistration(registration);
                        cRegistration.AddServiceRegistration(new SimpleServiceRegistration(handle, serviceInstance));
                        _handle2Registrations[handle] = cRegistration;
                    }
                    else
                    {
                        var cRegistration = registration as ComplexServiceRegistration;
                        cRegistration.AddServiceRegistration(new SimpleServiceRegistration(handle, serviceInstance));
                    }
                }
                else
                {
                    registration = new SimpleServiceRegistration(handle, serviceInstance);
                    _handle2Registrations.Add(handle, registration);
                }
            }
                
            return handle;
        }

        public object GetService(Type contractType)
        {
            var serviceHandle = ServiceHandle.Create(contractType);
            lock (_syncObj)
            {
                ServiceRegistration registration;
                return _handle2Registrations.TryGetValue(serviceHandle, out registration)
                    ? registration.GetService()
                    : null;
            }
        }

        public List<object> GetServices(Type contractType)
        {
            var serviceHandle = ServiceHandle.Create(contractType);
            lock (_syncObj)
            {
                ServiceRegistration registration;
                if (!_handle2Registrations.TryGetValue(serviceHandle, out registration))
                    return null;
                if (registration.ServiceCount == 1)
                    return new List<object> { registration.GetService() };
                var cRegistration = registration as ComplexServiceRegistration;
                return cRegistration.GetServices();
            }
        }

        public ServiceHandle AddService<TContract>(MyFunc<TContract> instanceCreator)
        {
            Requires.Instance.NotNull(instanceCreator, "instanceCreator");
            var handle = ServiceHandle.CreateAndAllocateId(typeof(TContract));
            return DoAddService(handle, instanceCreator);
        }

        ServiceHandle DoAddService<TContract>(ServiceHandle handle, MyFunc<TContract> instanceCreator)
        {
            lock (_syncObj)
            {
                ServiceRegistration registration;
                if (_handle2Registrations.TryGetValue(handle, out registration))
                {
                    if (registration.ServiceCount == 1)
                    {
                        var cRegistration = new ComplexServiceRegistration(registration);
                        cRegistration.AddServiceRegistration(new SimpleFuncServiceRegistration<TContract>(handle, instanceCreator));
                        _handle2Registrations[handle] = cRegistration;
                    }
                    else
                    {
                        var cRegistration = registration as ComplexServiceRegistration;
                        cRegistration.AddServiceRegistration(new SimpleFuncServiceRegistration<TContract>(handle, instanceCreator));
                    }
                }
                else
                {
                    registration = new SimpleFuncServiceRegistration<TContract>(handle, instanceCreator);
                    _handle2Registrations.Add(handle, registration);
                }
            }

            return handle;
        }

        public TContract GetService<TContract>()
        {
            var serviceHandle = ServiceHandle.Create(typeof(TContract));
            lock (_syncObj)
            {
                ServiceRegistration registration;
                return _handle2Registrations.TryGetValue(serviceHandle, out registration)
                    ? (TContract)registration.GetService()
                    : default(TContract);
            }
        }

        public List<TContract> GetServices<TContract>()
        {
            var serviceHandle = ServiceHandle.Create(typeof(TContract));
            lock (_syncObj)
            {
                ServiceRegistration registration;
                if (!_handle2Registrations.TryGetValue(serviceHandle, out registration))
                    return null;
                if (registration.ServiceCount == 1)
                    return new List<TContract> { (TContract)registration.GetService() };
                var cRegistration = registration as ComplexServiceRegistration;
                return cRegistration.GetServices<TContract>();
            }
        }

        public void RemoveService(ServiceHandle serviceHandle)
        {
            lock (_syncObj)
            {
                ServiceRegistration registration;
                if (_handle2Registrations.TryGetValue(serviceHandle, out registration))
                {
                    if (registration.ServiceCount <= 1)
                    {
                        _handle2Registrations.Remove(serviceHandle);
                    }
                    else
                    {
                        var cRegistration = registration as ComplexServiceRegistration;
                        cRegistration.RemoveServiceRegistration(serviceHandle);
                    }
                }
            }
        }
    }
}
