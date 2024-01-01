using System;
using System.Collections.Generic;
using Oxide.Data.Formatters;

namespace Oxide.Data.StorageDrivers;

/// <summary>
/// A storage driver that supports formatted data
/// </summary>
public interface IFormattableStorageDriver : IStorageDriver
{
    /// <summary>
    /// The default <see cref="IDataFormatter"/> to use when one isn't specified
    /// </summary>
    IDataFormatter DefaultFormatter { get; }

    /// <summary>
    /// A list of <see cref="IDataFormatter"/>'s that this <see cref="IFormattableStorageDriver"/> is capable of using
    /// </summary>
    IEnumerable<IDataFormatter> Formatters { get; }

    /// <summary>
    /// Writes formatted data to this storage driver
    /// </summary>
    /// <param name="key">The key to write</param>
    /// <param name="data">The data</param>
    /// <param name="formatter">The <see cref="IDataFormatter"/> to use when writing, null will use default</param>
    void Write(string key, object data, IDataFormatter formatter = null);

    /// <summary>
    /// Reads formatted data from this storage driver
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <param name="dataType">The type used to graph data onto</param>
    /// <param name="formatter">The <see cref="IDataFormatter"/> to use when reading, null will use default</param>
    /// <param name="existingGraph">Tells the driver to populate a existing object</param>
    /// <returns></returns>
    object Read(string key, Type dataType, IDataFormatter formatter = null, object existingGraph = null);

    /// <summary>
    /// Will perform a thread-safe update of available <see cref="IDataFormatter"/>
    /// </summary>
    /// <param name="updateFormattersCallback">Once the thread is locked this callback will executed</param>
    void UpdateFormatters(Func<ICollection<IDataFormatter>, IDataFormatter> updateFormattersCallback);
}
