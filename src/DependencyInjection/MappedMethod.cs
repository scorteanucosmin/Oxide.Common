using System;
using System.Reflection;

namespace Oxide.DependencyInjection
{
    internal class MappedMethod : IComparable<MappedMethod>
    {
        private static readonly Type StrType = typeof(string);

        private ConstructorInfo Constructor { get; }

        private ParameterInfo[] Parameters { get; }

        private int ExactMapped { get; }

        private int TotalMapped { get; }

        private int?[] Mapped { get; }

        private object[] MappedParameters { get; }

        public MappedMethod(ConstructorInfo constructor, object[] arguments)
        {
            Constructor = constructor;
            Parameters = constructor.GetParameters();
            Mapped = new int?[Parameters.Length];
            MappedParameters = new object[Parameters.Length];
            TotalMapped = Map(arguments, out int exact);
            ExactMapped = exact;
        }

        private int Map(object[] arguments, out int exact)
        {
            exact = 0;
            if (Parameters.Length == 0)
            {
                return 0;
            }

            int mapped = 0;
            for (int i = 0; i < Parameters.Length; i++)
            {
                ParameterInfo p = Parameters[i];
                Type pType = p.ParameterType;

                for (int n = 0; n < arguments.Length; n++)
                {
                    if (IsPositionMapped(n))
                    {
                        continue;
                    }

                    object a = arguments[n];

                    if (a == null)
                    {
                        if (p.IsOut || pType.IsByRef)
                        {
                            Mapped[p.Position] = n;
                            mapped++;
                            break;
                        }

                        continue;
                    }

                    if (p.IsOut)
                    {
                        break;
                    }

                    Type aType = a.GetType();

                    if (pType.IsAssignableFrom(aType))
                    {
                        Mapped[p.Position] = n;
                        mapped++;

                        if (pType == aType)
                        {
                            exact++;
                        }
                        break;
                    }
                }
            }

            return mapped;
        }

        private bool IsPositionMapped(int pos)
        {
            for (int i = 0; i < Mapped.Length; i++)
            {
                if (Mapped[i] == pos)
                {
                    return true;
                }
            }

            return false;
        }

        public object Invoke(IServiceProvider provider, object[] arguments)
        {
            try
            {
                for (int i = 0; i < Parameters.Length; i++)
                {
                    ParameterInfo p = Parameters[i];

                    if (p.IsOut)
                    {
                        continue;
                    }

                    int? argPosition = Mapped[p.Position];

                    if (argPosition.HasValue)
                    {
                        MappedParameters[p.Position] = arguments[argPosition.Value];
                    }
                    else if (p.ParameterType.IsPrimitive || p.ParameterType == StrType)
                    {
                        if (p.IsOptionalParameter())
                        {
                            MappedParameters[p.Position] = p.DefaultValue;
                        }
                        else
                        {
                            MappedParameters[p.Position] = p.ParameterType != StrType ? Activator.CreateInstance(p.ParameterType) : null;
                        }
                    }
                    else if (provider != null)
                    {
                        IDependencyResolverFactory resolver = provider.GetService<IDependencyResolverFactory>();

                        if (p.IsOptionalParameter())
                        {
                            MappedParameters[p.Position] = resolver?.ResolveService(p) ?? provider.GetService(p.ParameterType) ?? p.DefaultValue;
                        }
                        else
                        {
                            MappedParameters[p.Position] = resolver?.ResolveService(p) ?? provider.GetRequiredService(p.ParameterType);
                        }
                    }
                }

                object value = Constructor.Invoke(MappedParameters);

                for (int i = 0; i < Parameters.Length; i++)
                {
                    ParameterInfo parameter = Parameters[i];
                    int? mapped = Mapped[parameter.Position];

                    if (mapped.HasValue && (parameter.IsOut || parameter.ParameterType.IsByRef))
                    {
                        arguments[mapped.Value] = MappedParameters[parameter.Position];
                    }
                }

                return value;
            }
            finally
            {
                for (int i = 0; i < MappedParameters.Length; i++)
                {
                    MappedParameters[i] = null;
                }
            }
        }

        public int CompareTo(MappedMethod other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            int exactMappedComparison = ExactMapped.CompareTo(other.ExactMapped);
            return exactMappedComparison != 0 ? exactMappedComparison : TotalMapped.CompareTo(other.TotalMapped);
        }
    }
}
