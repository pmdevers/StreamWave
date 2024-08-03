namespace StreamWave.EntityFramework;

/// <summary>
/// Defines methods for serializing and deserializing event data in an event-sourced system.
/// </summary>
public interface IEventSerializer
{
    /// <summary>
    /// Deserializes the provided byte array into an object of the specified type.
    /// </summary>
    /// <param name="data">The byte array containing the serialized data.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <returns>The deserialized object, or null if the data cannot be deserialized.</returns>
    object? Deserialize(byte[] data, Type type);

    /// <summary>
    /// Serializes the provided object into a byte array.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="type">The type of the object being serialized.</param>
    /// <returns>A byte array containing the serialized data.</returns>
    byte[] Serialize(object value, Type type);
}
