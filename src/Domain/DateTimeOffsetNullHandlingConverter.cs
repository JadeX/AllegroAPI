namespace JadeX.AllegroAPI.Domain;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateTimeOffsetNullHandlingConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null
            ? default
            : reader.GetDateTimeOffset();

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
