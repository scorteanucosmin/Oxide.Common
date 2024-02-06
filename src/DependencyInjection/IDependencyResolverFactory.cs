using System;

namespace Oxide.DependencyInjection
{
    /// <summary>
    /// A factory interface for handling dependency resolution
    /// </summary>
    public interface IDependencyResolverFactory
    {
        /// <summary>
        /// Resolved the requested type
        /// </summary>
        /// <param name="declaringType">The type the resolver is trying to resolve to</param>
        /// <param name="requestedType">The requested type of the Property, Field, or Parameter</param>
        /// <param name="name">The name of the Property, Field, or Parameter</param>
        /// <param name="resolveType">The type of resolution</param>
        /// <param name="obj">The raw object resolving to</param>
        /// <returns>The resolved instance of the requested type</returns>
        object ResolveService(Type declaringType, Type requestedType, string name, DependencyResolveType resolveType, object obj);

        /// <summary>
        /// Register a resolver with this factory for use with resolution
        /// </summary>
        /// <typeparam name="TResolver">The resolver type</typeparam>
        IDependencyResolverFactory RegisterServiceResolver<TResolver>() where TResolver : IDependencyResolver;
    }
}
