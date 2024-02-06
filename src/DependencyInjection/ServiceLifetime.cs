namespace Oxide.DependencyInjection
{
    /// <summary>
    /// Defines the lifetime of a service
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// Only one instance of a service exists
        /// </summary>
        Singleton,

        /// <summary>
        /// A new instance is created on each request
        /// </summary>
        Transient
    }
}
