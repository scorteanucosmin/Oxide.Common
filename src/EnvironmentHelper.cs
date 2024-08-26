using System;

namespace Oxide
{
    /// <summary>
    /// Helper methods for interacting with OxideMod specific environmental variables
    /// </summary>
    public static class EnvironmentHelper
    {
        public const string SECTION_DELIMITER = ":";

        public const string OXIDE_PREFIX = "OXIDE_";

        /// <summary>
        /// Gets a OxideMod environmental variable
        /// </summary>
        /// <param name="key">The environmental variable key</param>
        /// <returns>The environmental value</returns>
        public static string GetVariable(string key) => Environment.GetEnvironmentVariable(NormalizeKey(key));

        /// <summary>
        /// Sets a OxideMod environmental variable for the process
        /// </summary>
        /// <remarks>
        /// Setting forced to true will bypass throwOnExisting
        /// </remarks>
        /// <param name="key">The environmental variable key</param>
        /// <param name="value">The environmental variable value</param>
        /// <param name="throwOnExisting">The exception if the variable already exists</param>
        /// <param name="force">Overwrite exist variable</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetVariable(string key, string value, bool throwOnExisting = false, bool force = false)
        {
            key = NormalizeKey(key);

            string existingValue = !force ? Environment.GetEnvironmentVariable(key) : null;

            if (existingValue != null)
            {
                if (throwOnExisting)
                {
                    throw new InvalidOperationException(
                        $"'{key}' has existing value of '{existingValue}' to override set 'force' to 'true'");
                }

                return;
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

            if (!key.StartsWith(OXIDE_PREFIX, StringComparison.InvariantCultureIgnoreCase))
            {
                key = OXIDE_PREFIX + key;
            }
            else
            {
                key = OXIDE_PREFIX + key.Substring(6);
            }

            return key.Replace(SECTION_DELIMITER, "__").ToUpperInvariant();
        }
    }
}
