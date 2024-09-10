namespace IceCraft.Api.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using Semver;

public class SemVersionRangeConverter : JsonConverter<SemVersionRange>
{
    public override SemVersionRange? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str == null ? null : SemVersionRange.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, SemVersionRange value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}