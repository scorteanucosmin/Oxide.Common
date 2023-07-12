using System;

namespace Oxide
{
    public static class EnvironmentHelper
    {
        public static string GetVariable(string key) => Environment.GetEnvironmentVariable(NormalizeKey(key));

        public static void SetVariable(string key, string value, bool force = false)
        {
            key = NormalizeKey(key);

            if (force)
            {
                Environment.SetEnvironmentVariable(key, value);
                return;
            }

            string existingValue = Environment.GetEnvironmentVariable(key);

            if (existingValue != null)
            {
                throw new InvalidOperationException(
                    $"'{key}' has existing value of '{existingValue}' to override set 'force' to 'true'");
            }

            Environment.SetEnvironmentVariable(key, value);
        }

        private static string NormalizeKey(string key)
        {
            key = key.Trim();
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!key.StartsWith("oxide:", StringComparison.InvariantCultureIgnoreCase))
            {
                key = $"Oxide:{key}";
            }
            else
            {
                key = "Oxide:" + key.Substring(7);
            }

            return key;
        }
    }
}
