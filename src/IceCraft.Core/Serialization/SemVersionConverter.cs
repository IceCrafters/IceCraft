namespace IceCraft.Core.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Semver;

public class SemVersionConverter : JsonConverter<SemVersion>
{
    public override SemVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        return SemVersion.Parse(str, SemVersionStyles.Strict);
    }

    public override void Write(Utf8JsonWriter writer, SemVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
