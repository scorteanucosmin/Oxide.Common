using Oxide.Data;

namespace Oxide.Configuration;

public interface IConfigurationManager
{
    /// <summary>
    /// Reads a configuration from a source
    /// </summary>
    /// <param name="name">The name of the config without any pathing data</param>
    /// <typeparam name="T">The type used to bind the data</typeparam>
    /// <returns>Configuration with bound values</returns>
    /// <remarks>If the file needs a extension</remarks>
    T ReadConfig<T>(string name = null);

    /// <summary>
    /// Writes a configuration to a source
    /// </summary>
    /// <param name="config">The object to write</param>
    /// <param name="name">The name of the config without any pathing data</param>
    /// <typeparam name="T">The type used to bind the data</typeparam>
    void WriteConfig<T>(T config, string name = null);
}
