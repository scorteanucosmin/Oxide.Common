using System;

namespace Oxide.DependencyInjection
{
    /// <summary>
    /// A collection of services
    /// </summary>
    public interface IServiceCollection
    {
        /// <summary>
        /// Adds a service to this collection
        /// </summary>
        /// <param name="serviceDescriptor"></param>
        void Add(ServiceDescriptor serviceDescriptor);

        /// <summary>
        /// Creates the service provider
        /// </summary>
        /// <returns></returns>
        IServiceProvider BuildServiceProvider();
    }
}
