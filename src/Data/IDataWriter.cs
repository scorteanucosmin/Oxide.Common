namespace Oxide.Data;

/// <summary>
/// A interface used for writing data
/// </summary>
public interface IDataWriter
{
    /// <summary>
    /// Writes data to this <see cref="IDataWriter"/>
    /// </summary>
    /// <param name="data">The data to write</param>
    /// <param name="destinationStr">Used to instruct writer where to write</param>
    /// <typeparam name="T">The type used to map key values</typeparam>
    void Write<T>(T data, string destinationStr);
}
