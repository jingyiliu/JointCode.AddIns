using JointCode.Common;
using System;
using System.Collections.Generic;

namespace JointCode.AddIns
{
    partial class ServiceHandle
    {
        static class UniqueIdAllocator
        {
            static object _sync = new object();
            static int _currentId = int.MinValue;

            internal static int Allocate()
            {
                var result = _currentId;
                lock (_sync)
                {
                    if (_currentId == int.MaxValue)
                        _currentId = int.MinValue;
                    else
                        _currentId += 1;
                }
                return result;
            }
        }

        class ServiceHandleComparer : IEqualityComparer<ServiceHandle>
        {
            public bool Equals(ServiceHandle x, ServiceHandle y)
            {
                return x.ContractType == y.ContractType;
            }

            public int GetHashCode(ServiceHandle obj)
            {
                return obj.ContractType.GetHashCode();
            }
        }

        public static readonly IEqualityComparer<ServiceHandle> EqualityComparer = new ServiceHandleComparer();

        public static ServiceHandle CreateAndAllocateId(Type contractType)
        {
            var uniqueId = UniqueIdAllocator.Allocate();
            return new ServiceHandle(contractType, uniqueId);
        }

        public static ServiceHandle Create(Type contractType)
        { return new ServiceHandle(contractType); }
    }

    public partial class ServiceHandle : IEquatable<ServiceHandle>
    {
        int _uniqueId;
        readonly Type _contractType;

        private ServiceHandle(Type contractType)
        {
            _contractType = contractType;
        }

        private ServiceHandle(Type contractType, int uniqueId)
        {
            _uniqueId = uniqueId;
            _contractType = contractType;
        }

        public Type ContractType { get { return _contractType; } }
        public int UniqueId { get { return _uniqueId; } }

        public bool Equals(ServiceHandle other)
        {
            return other == null ? false : _uniqueId == other._uniqueId;
        }
    }

    public interface IServiceProvider
    {
        object WrappedServiceContainer { get; }

        ServiceHandle AddService(Type contractType, object serviceInstance);
        //ServiceHandle AddService(Type contractType, MyFunc<object> instanceCreator);

        object GetService(Type contractType);
        List<object> GetServices(Type contractType);

        //ServiceHandle AddService<TContract>(TContract serviceInstance);
        ServiceHandle AddService<TContract>(MyFunc<TContract> instanceCreator);

        TContract GetService<TContract>();
        List<TContract> GetServices<TContract>();

        void RemoveService(ServiceHandle serviceHandle);
    }
}