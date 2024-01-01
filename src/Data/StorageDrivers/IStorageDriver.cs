using System;

namespace Oxide.Data.StorageDrivers
{
    /// <summary>
    /// Used to read and write data to a specific providers
    /// </summary>
    public interface IStorageDriver
    {
        /// <summary>
        /// Writes data to this storage driver
        /// </summary>
        /// <param name="key">The key to write</param>
        /// <param name="data">The data</param>
        void Write(string key, object data);

        /// <summary>
        /// Reads data from this storage driver
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="dataType">The type used to graph data onto</param>
        /// <param name="existingGraph">Tells the driver to populate a existing object</param>
        /// <returns>The object with data populated</returns>
        /// <exception cref="NullReferenceException">Thrown when the given key doesn't exist</exception>
        object Read(string key, Type dataType, object existingGraph = null);
    }
}
