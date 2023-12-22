namespace Oxide.Data;

/// <summary>
/// A interface used for reading data
/// </summary>
public interface IDataReader
{
    /// <summary>
    /// Reads data from this <see cref="IDataReader"/>
    /// </summary>
    /// <typeparam name="T">The type used to bind data to</typeparam>
    /// <param name="sourceStr">Used to tell the reader where to read from</param>
    /// <returns>A object instance with data bound to it</returns>
    T Read<T>(string sourceStr);
}
