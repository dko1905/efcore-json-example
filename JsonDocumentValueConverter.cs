using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TestApp;

/// <summary>
/// EF Core Converter for <c>JsonNode</c> into string and vice-versa.
/// </summary>
public sealed class JsonNodeValueConverter : ValueConverter<JsonNode, JsonElement>
{
    public JsonNodeValueConverter()
        : this(null) { }

    public JsonNodeValueConverter(ConverterMappingHints? mappingHints = null)
        : base(
            jsonDoc => JsonSerializer.SerializeToElement(jsonDoc, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<JsonNode>(value, (JsonSerializerOptions?)null)!,
            mappingHints
        ) { }
}
