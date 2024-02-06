using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Oxide.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        private static readonly Type OptionalAttribute = typeof(OptionalAttribute);

        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            return provider.GetService(serviceType) ?? throw new MissingServiceException(serviceType);
        }

        public static TService GetRequiredService<TService>(this IServiceProvider provider)
        {
            return (TService)provider.GetService(typeof(TService)) ?? throw new MissingServiceException(typeof(TService));
        }

        public static TService GetService<TService>(this IServiceProvider provider)
        {
            return (TService)provider.GetService(typeof(TService)) ?? default;
        }

        public static bool IsOptionalParameter(this ParameterInfo parameter)
        {
            return parameter.IsOptional ||
                   parameter.ParameterType.GetCustomAttributes(OptionalAttribute, false).Length > 0;
        }

        #region Dependency Factory

        public static object ResolveService(this IDependencyResolverFactory factory, ParameterInfo parameter)
        {
            return factory.ResolveService(parameter.Member.DeclaringType, parameter.ParameterType, parameter.Name, DependencyResolveType.Parameter, parameter);
        }

        public static object ResolveService(this IDependencyResolverFactory factory, FieldInfo field)
        {
            return factory.ResolveService(field.DeclaringType, field.FieldType, field.Name, DependencyResolveType.Field, field);
        }

        public static object ResolveService(this IDependencyResolverFactory factory, PropertyInfo property)
        {
            return factory.ResolveService(property.DeclaringType, property.PropertyType, property.Name, DependencyResolveType.Property, property);
        }

        #endregion

        #region Service Collection

        public static IServiceCollection AddSingleton(this IServiceCollection collection, Type serviceType, Type implementationType)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton));
            return collection;
        }

        public static IServiceCollection AddSingleton(this IServiceCollection collection, Type serviceType, object implementation)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementation));
            return collection;
        }

        public static IServiceCollection AddSingleton(this IServiceCollection collection, Type implementationType)
        {
            collection.Add(new ServiceDescriptor(implementationType, implementationType, ServiceLifetime.Singleton));
            return collection;
        }

        public static IServiceCollection AddSingleton(this IServiceCollection collection, object implementation)
        {
            Type type = implementation.GetType();
            collection.Add(new ServiceDescriptor(type, implementation));
            return collection;
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection collection)
        {
            collection.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
            return collection;
        }

        public static IServiceCollection AddSingleton<TImplementation>(this IServiceCollection collection)
        {
            Type type = typeof(TImplementation);
            collection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            return collection;
        }

        public static IServiceCollection AddSingleton<TImplementation>(this IServiceCollection collection, TImplementation instance)
        {
            collection.Add(new ServiceDescriptor(typeof(TImplementation), instance));
            return collection;
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection collection, TImplementation instance)
        {
            collection.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
            return collection;
        }

        public static IServiceCollection AddTransient(this IServiceCollection collection, Type serviceType, Type implementationType)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
            return collection;
        }

        public static IServiceCollection AddTransient(this IServiceCollection collection, Type implementationType)
        {
            collection.Add(new ServiceDescriptor(implementationType, implementationType, ServiceLifetime.Transient));
            return collection;
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection collection)
        {
            collection.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
            return collection;
        }

        public static IServiceCollection AddTransient<TImplementation>(this IServiceCollection collection)
        {
            Type type = typeof(TImplementation);
            collection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Transient));
            return collection;
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection collection, Func<IServiceProvider, TService> factory)
        {
            collection.Add(new ServiceDescriptor(typeof(TService), factory));
            return collection;
        }

        #endregion
    }
}
