namespace Brimborium.Text;

/// <summary>
/// Provides JSON conversion functionality for <see cref="StringSlice"/> type.
/// </summary>
/// <remarks>
/// This converter supports two formats:
/// 1. Direct string format: "text"
/// 2. Object format: {"Text": "text"}
/// 
/// When serializing, it always uses the direct string format.
/// When deserializing, it accepts both formats for compatibility.
/// </remarks>
public class StringSliceConverter : JsonConverter<StringSlice> {
    private static readonly JsonEncodedText PropName_Text = JsonEncodedText.Encode("Text");
    
    public override StringSlice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (JsonTokenType.Null == reader.TokenType) {
            return new StringSlice(string.Empty);
        }
        if (JsonTokenType.String == reader.TokenType) {
            return new StringSlice(reader.GetString() ?? string.Empty);
        }
        if (JsonTokenType.StartObject == reader.TokenType) {
            if (reader.Read()) {
                if (JsonTokenType.PropertyName == reader.TokenType) {
                    if (reader.ValueSpan.SequenceEqual(PropName_Text.EncodedUtf8Bytes)) {
                        //if (reader.GetString() == "Text") {
                        if (reader.Read()) {
                            if (JsonTokenType.String == reader.TokenType) {
                                var value = reader.GetString() ?? string.Empty;
                                if (reader.Read()) {
                                    if (JsonTokenType.EndObject == reader.TokenType) {
                                        return new StringSlice(value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, StringSlice value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.AsSpan());
    }
}

public class StringSliceConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        return typeof(StringSlice).Equals(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        return new StringSliceConverter();
    }
}
