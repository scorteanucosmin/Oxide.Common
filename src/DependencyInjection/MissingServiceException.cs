using System;

namespace Oxide.DependencyInjection
{
    public class MissingServiceException : NullReferenceException
    {
        public Type Service { get; }

        public MissingServiceException(Type serviceType) : base($"Missing required service: {serviceType.FullName}")
        {
            Service = serviceType;
        }
    }
}
