using System.Text.Json;

namespace StreamWave.EntityFramework;

/// <summary>
/// 
/// </summary>
/// <param name="options"></param>
public class DefaultSerializer(JsonSerializerOptions options) : IEventSerializer
{
    /// <summary>
    /// 
    /// </summary>
    public JsonSerializerOptions Options { get; } = options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object? Deserialize(byte[] data, Type type)
    {
        using var stream = new MemoryStream(data);
        return JsonSerializer.Deserialize(stream, type, Options);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public byte[] Serialize(object value, Type type)
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, value, type, Options);
        stream.Flush();
        return stream.ToArray();
    }
}
