using System;
using System.IO;

namespace Oxide.Data.Formatters;

/// <summary>
/// Formats data to and from a specific data standard
/// </summary>
public interface IDataFormatter
{
    /// <summary>
    /// The file extension to be used when this <see cref="IDataFormatter"/> is used to write to disk
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// The Mime type used when this <see cref="IDataFormatter"/> is used to sent data over the internet
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Formats and serializes <paramref name="graph"/> to the <paramref name="serializationStream"/>
    /// </summary>
    /// <param name="serializationStream">The stream to write to</param>
    /// <param name="graphType">The type used to graph values onto the stream</param>
    /// <param name="graph">The context that is being used to pull data from</param>
    void Serialize(Stream serializationStream, Type graphType, object graph);

    /// <summary>
    /// Deserializes and rebind data back onto a object
    /// </summary>
    /// <param name="graphType">The type used as reference when reading from the stream</param>
    /// <param name="serializationStream">The source stream</param>
    /// <param name="existingGraph">Reuse a existing object and populates data onto</param>
    /// <returns>The new or existing graph</returns>
    object Deserialize(Type graphType, Stream serializationStream, object existingGraph = null);
}
