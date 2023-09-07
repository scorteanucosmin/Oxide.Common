#if NET35 || NET40

namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AssemblyMetadataAttribute : Attribute
    {
        public string Key { get; protected set; }

        public string Value { get; protected set; }

        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}

#endif
