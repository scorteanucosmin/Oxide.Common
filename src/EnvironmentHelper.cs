using System;

namespace Oxide
{
    /// <summary>
    /// Helper methods for interacting with OxideMod specific environmental variables
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Gets a OxideMod environmental variable
        /// </summary>
        /// <param name="key">The environmental variable</param>
        /// <returns></returns>
        public static string GetVariable(string key) => Environment.GetEnvironmentVariable(NormalizeKey(key));

        /// <summary>
        /// Sets a OxideMod environmental variable for the process
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="force"></param>
        /// <exception cref="InvalidOperationException"></exception>
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
