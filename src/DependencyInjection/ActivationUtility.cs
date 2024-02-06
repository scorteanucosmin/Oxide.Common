using System;
using System.Linq;
using System.Reflection;

namespace Oxide.DependencyInjection
{
    /// <summary>
    /// Used for object creation with help of a <see cref="IServiceProvider"/>
    /// </summary>
    public static class ActivationUtility
    {
        private static readonly Type ConstructorAttribute = typeof(ActivatorConstructorAttribute);
        private static readonly object[] Empty = new object[0];

        /// <summary>
        /// Creates a instance of a type
        /// </summary>
        /// <param name="provider">The service provider</param>
        /// <param name="instanceType">The type to activate</param>
        /// <param name="parameters">Optional arguments used for activation</param>
        /// <returns></returns>
        public static object CreateInstance(IServiceProvider provider, Type instanceType, params object[] parameters)
        {
            parameters ??= Empty;
            MappedMethod method = GetBestMatch(instanceType, parameters);
            return method.Invoke(provider, parameters);
        }

        public static Func<IServiceProvider, object> CreateFactory(IServiceProvider provider, Type instanceType, params object[] parameters)
        {
            parameters ??= Empty;
            MappedMethod method = GetBestMatch(instanceType, parameters);
            return s => method.Invoke(s, parameters);
        }

        private static MappedMethod GetBestMatch(Type instanceType, object[] arguments, IDependencyResolverFactory factory = null)
        {
            MappedMethod method = null;
            ConstructorInfo[] constructs = GetValidConstructors(instanceType);

            for (int i = 0; i < constructs.Length; i++)
            {
                MappedMethod m = new MappedMethod(constructs[i], arguments);

                if (method == null)
                {
                    method = m;
                }
                else
                {
                    if (m.CompareTo(method) > 0)
                    {
                        method = m;
                    }
                }
            }

            return method;
        }

        private static ConstructorInfo[] GetValidConstructors(Type instanceType)
        {
            ConstructorInfo[] constructors = instanceType.GetConstructors( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            if (constructors.Length == 0)
            {
                constructors = new ConstructorInfo[] { instanceType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, types: Type.EmptyTypes, null) };
            }

            ConstructorInfo[] filtered = constructors.Where(HasConstructAttribute).ToArray();
            return filtered.Length > 0 ? filtered : constructors.Where(c => c.IsPublic).ToArray();
        }

        private static bool HasConstructAttribute(ConstructorInfo constructor)
        {
            return constructor?.GetCustomAttributes(ConstructorAttribute, false).Length > 0;
        }
    }
}
