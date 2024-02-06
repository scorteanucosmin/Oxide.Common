using System;

namespace Oxide.DependencyInjection
{
    public sealed class ServiceDescriptor
    {
        public Type ServiceType { get; }

        public Type ImplementationType { get; }

        public object ImplementationInstance { get; private set; }

        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new InvalidCastException("Implementation needs to be assignable to service type");
            }

            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, object implementationInstance) : this(serviceType, implementationInstance.GetType(), ServiceLifetime.Singleton)
        {
            ImplementationInstance = implementationInstance;
        }

        public ServiceDescriptor(Type serviceType, Delegate factory) : this(serviceType, serviceType, ServiceLifetime.Transient)
        {
            ImplementationInstance = factory;
        }

        public bool SetInstance(object instance)
        {
            if (ImplementationInstance != null)
            {
                return false;
            }

            ImplementationInstance = instance;
            return true;
        }
    }
}
