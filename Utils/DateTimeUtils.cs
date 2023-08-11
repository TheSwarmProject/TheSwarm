using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheSwarm.Utils;

/// <summary>
/// Custom DateTime converter for JsonSerializer
/// </summary>
public class JSONDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string format;
    public JSONDateTimeConverter(string format)
    {
        this.format = format;
    }

    public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
    {
        writer.WriteStringValue(date.ToString(format));
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString(), format, null);
    }
}