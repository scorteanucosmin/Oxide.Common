using System;

namespace Oxide.DependencyInjection
{
    /// <summary>
    /// Used with the <see cref="ActivationUtility"/> for resolving member assignments
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Determines if this resolver is able to resolve the context
        /// </summary>
        /// <param name="declaringType">The type the resolver is trying to resolve to</param>
        /// <param name="requestedType">The requested type of the Property, Field, or Parameter</param>
        /// <param name="name">The name of the Property, Field, or Parameter</param>
        /// <param name="resolveType">The type of resolution</param>
        /// <param name="obj">The raw object resolving to</param>
        /// <returns>True if can be resolved</returns>
        bool CanResolve(Type declaringType, Type requestedType, string name, DependencyResolveType resolveType, object obj);

        /// <summary>
        /// Resolved the requested type
        /// </summary>
        /// <param name="declaringType">The type the resolver is trying to resolve to</param>
        /// <param name="requestedType">The requested type of the Property, Field, or Parameter</param>
        /// <param name="name">The name of the Property, Field, or Parameter</param>
        /// <param name="resolveType">The type of resolution</param>
        /// <param name="obj">The raw object resolving to</param>
        /// <returns>The resolved instance of the requested type</returns>
        object Resolve(Type declaringType, Type requestedType, string name, DependencyResolveType resolveType, object obj);
    }
}
