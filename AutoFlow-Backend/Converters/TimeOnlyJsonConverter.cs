using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace AutoFlow_Backend.Converters;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] SupportedFormats = ["HH:mm:ss", "HH:mm"];
    private const string WriteFormat = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Time value is required.");
        }

        if (TimeOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new JsonException("Invalid time format. Use HH:mm:ss or HH:mm.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(WriteFormat));
}
