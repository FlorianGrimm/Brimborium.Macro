namespace Brimborium.Text;
#if false
using System.Text.Json.Serialization.Metadata;

//[JsonSerializable(typeof(StringSlice))]
//[GeneratedCode("System.Text.Json.SourceGeneration", "7.0.7.1805")]

//[JsonSerializable(typeof(StringSlice))]
public partial class StringSliceJsonContext : JsonSerializerContext, IJsonTypeInfoResolver {
    //public StringSliceJsonContext(JsonSerializerOptions? options) : base(options) {
    //}

    //protected override JsonSerializerOptions? GeneratedSerializerOptions
    //    => throw new NotImplementedException();

    //public override JsonTypeInfo? GetTypeInfo(Type type) {
    //    throw new NotImplementedException();
    //}

    //JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options) { 
    //    throw new NotImplementedException();
    //}

    private JsonTypeInfo<StringSlice>? _StringSlice;

    private static StringSliceJsonContext? s_defaultContext;

    //private static readonly JsonEncodedText PropName_Text = JsonEncodedText.Encode("Text");

    //private static readonly JsonEncodedText PropName_End = JsonEncodedText.Encode("End");

    //private static readonly JsonEncodedText PropName_Start = JsonEncodedText.Encode("Start");

    //private static readonly JsonEncodedText PropName_IsFromEnd = JsonEncodedText.Encode("IsFromEnd");

    //private static readonly JsonEncodedText PropName_Value = JsonEncodedText.Encode("Value");

    //public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = Create_Int32(base.Options, makeReadOnly: true));

    //public JsonTypeInfo<string> String => _String ?? (_String = Create_String(base.Options, makeReadOnly: true));

    //public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = Create_Boolean(base.Options, makeReadOnly: true));

    //public JsonTypeInfo<Index> Index => _Index ?? (_Index = Create_Index(base.Options, makeReadOnly: true));

    //public JsonTypeInfo<Range> Range => _Range ?? (_Range = Create_Range(base.Options, makeReadOnly: true));

    public JsonTypeInfo<StringSlice> StringSlice => _StringSlice ?? (_StringSlice = Create_StringSlice(base.Options, makeReadOnly: true));

    private static JsonSerializerOptions s_defaultOptions { get; } = new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = false,
        WriteIndented = false
    };


    public static StringSliceJsonContext Default => s_defaultContext ?? (s_defaultContext = new StringSliceJsonContext(new JsonSerializerOptions(s_defaultOptions)));

    protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

    //private JsonTypeInfo<string> Create_String(JsonSerializerOptions options, bool makeReadOnly) {
    //    JsonTypeInfo<string> jsonTypeInfo = null;
    //    JsonConverter customConverter;
    //    jsonTypeInfo = ((options.Converters.Count <= 0 || (customConverter = GetRuntimeProvidedCustomConverter(options, typeof(string))) == null) ? JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter) : JsonMetadataServices.CreateValueInfo<string>(options, customConverter));
    //    if (makeReadOnly) {
    //        jsonTypeInfo.MakeReadOnly();
    //    }
    //    return jsonTypeInfo;
    //}


    //private JsonTypeInfo<Index> Create_Index(JsonSerializerOptions options, bool makeReadOnly) {
    //    JsonSerializerOptions options2 = options;
    //    JsonTypeInfo<Index> jsonTypeInfo = null;
    //    JsonConverter customConverter;
    //    if (options2.Converters.Count > 0 && (customConverter = GetRuntimeProvidedCustomConverter(options2, typeof(Index))) != null) {
    //        jsonTypeInfo = JsonMetadataServices.CreateValueInfo<Index>(options2, customConverter);
    //    } else {
    //        JsonObjectInfoValues<Index> objectInfo = new JsonObjectInfoValues<Index> {
    //            ObjectCreator = () => default(Index),
    //            ObjectWithParameterizedConstructorCreator = null,
    //            PropertyMetadataInitializer = (JsonSerializerContext _) => IndexPropInit(options2),
    //            ConstructorParameterMetadataInitializer = null,
    //            NumberHandling = JsonNumberHandling.Strict,
    //            SerializeHandler = IndexSerializeHandler
    //        };
    //        jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options2, objectInfo);
    //    }
    //    if (makeReadOnly) {
    //        jsonTypeInfo.MakeReadOnly();
    //    }
    //    return jsonTypeInfo;
    //}

    //private static JsonPropertyInfo[] IndexPropInit(JsonSerializerOptions options) {
    //    JsonPropertyInfo[] properties = new JsonPropertyInfo[3];
    //    JsonPropertyInfoValues<bool> jsonPropertyInfoValues = new JsonPropertyInfoValues<bool>();
    //    jsonPropertyInfoValues.IsProperty = true;
    //    jsonPropertyInfoValues.IsPublic = true;
    //    jsonPropertyInfoValues.IsVirtual = false;
    //    jsonPropertyInfoValues.DeclaringType = typeof(Index);
    //    jsonPropertyInfoValues.Converter = null;
    //    jsonPropertyInfoValues.Getter = (object obj) => ((Index)obj).IsFromEnd;
    //    jsonPropertyInfoValues.Setter = null;
    //    jsonPropertyInfoValues.IgnoreCondition = null;
    //    jsonPropertyInfoValues.HasJsonInclude = false;
    //    jsonPropertyInfoValues.IsExtensionData = false;
    //    jsonPropertyInfoValues.NumberHandling = null;
    //    jsonPropertyInfoValues.PropertyName = "IsFromEnd";
    //    jsonPropertyInfoValues.JsonPropertyName = null;
    //    JsonPropertyInfoValues<bool> info0 = jsonPropertyInfoValues;
    //    JsonPropertyInfo propertyInfo0 = (properties[0] = JsonMetadataServices.CreatePropertyInfo(options, info0));
    //    JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
    //    jsonPropertyInfoValues2.IsProperty = true;
    //    jsonPropertyInfoValues2.IsPublic = true;
    //    jsonPropertyInfoValues2.IsVirtual = false;
    //    jsonPropertyInfoValues2.DeclaringType = typeof(Index);
    //    jsonPropertyInfoValues2.Converter = null;
    //    jsonPropertyInfoValues2.Getter = (object obj) => ((Index)obj).Value;
    //    jsonPropertyInfoValues2.Setter = null;
    //    jsonPropertyInfoValues2.IgnoreCondition = null;
    //    jsonPropertyInfoValues2.HasJsonInclude = false;
    //    jsonPropertyInfoValues2.IsExtensionData = false;
    //    jsonPropertyInfoValues2.NumberHandling = null;
    //    jsonPropertyInfoValues2.PropertyName = "Value";
    //    jsonPropertyInfoValues2.JsonPropertyName = null;
    //    JsonPropertyInfoValues<int> info1 = jsonPropertyInfoValues2;
    //    JsonPropertyInfo propertyInfo1 = (properties[1] = JsonMetadataServices.CreatePropertyInfo(options, info1));
    //    jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
    //    jsonPropertyInfoValues2.IsProperty = false;
    //    jsonPropertyInfoValues2.IsPublic = false;
    //    jsonPropertyInfoValues2.IsVirtual = false;
    //    jsonPropertyInfoValues2.DeclaringType = typeof(Index);
    //    jsonPropertyInfoValues2.Converter = null;
    //    jsonPropertyInfoValues2.Getter = null;
    //    jsonPropertyInfoValues2.Setter = null;
    //    jsonPropertyInfoValues2.IgnoreCondition = null;
    //    jsonPropertyInfoValues2.HasJsonInclude = false;
    //    jsonPropertyInfoValues2.IsExtensionData = false;
    //    jsonPropertyInfoValues2.NumberHandling = null;
    //    jsonPropertyInfoValues2.PropertyName = "_dummyPrimitive";
    //    jsonPropertyInfoValues2.JsonPropertyName = null;
    //    JsonPropertyInfoValues<int> info2 = jsonPropertyInfoValues2;
    //    JsonPropertyInfo propertyInfo2 = (properties[2] = JsonMetadataServices.CreatePropertyInfo(options, info2));
    //    return properties;
    //}

    //private void IndexSerializeHandler(Utf8JsonWriter writer, Index value) {
    //    writer.WriteStartObject();
    //    JsonEncodedText propName_IsFromEnd = PropName_IsFromEnd;
    //    Index index = value;
    //    writer.WriteBoolean(propName_IsFromEnd, index.IsFromEnd);
    //    JsonEncodedText propName_Value = PropName_Value;
    //    index = value;
    //    writer.WriteNumber(propName_Value, index.Value);
    //    writer.WriteEndObject();
    //}

    //private JsonTypeInfo<Range> Create_Range(JsonSerializerOptions options, bool makeReadOnly) {
    //    JsonSerializerOptions options2 = options;
    //    JsonTypeInfo<Range> jsonTypeInfo = null;
    //    JsonConverter customConverter;
    //    if (options2.Converters.Count > 0 && (customConverter = GetRuntimeProvidedCustomConverter(options2, typeof(Range))) != null) {
    //        jsonTypeInfo = JsonMetadataServices.CreateValueInfo<Range>(options2, customConverter);
    //    } else {
    //        JsonObjectInfoValues<Range> objectInfo = new JsonObjectInfoValues<Range> {
    //            ObjectCreator = () => default(Range),
    //            ObjectWithParameterizedConstructorCreator = null,
    //            PropertyMetadataInitializer = (JsonSerializerContext _) => RangePropInit(options2),
    //            ConstructorParameterMetadataInitializer = null,
    //            NumberHandling = JsonNumberHandling.Strict,
    //            SerializeHandler = RangeSerializeHandler
    //        };
    //        jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options2, objectInfo);
    //    }
    //    if (makeReadOnly) {
    //        jsonTypeInfo.MakeReadOnly();
    //    }
    //    return jsonTypeInfo;
    //}

    //private static JsonPropertyInfo[] RangePropInit(JsonSerializerOptions options) {
    //    JsonPropertyInfo[] properties = new JsonPropertyInfo[3];
    //    JsonPropertyInfoValues<Index> jsonPropertyInfoValues = new JsonPropertyInfoValues<Index>();
    //    jsonPropertyInfoValues.IsProperty = true;
    //    jsonPropertyInfoValues.IsPublic = true;
    //    jsonPropertyInfoValues.IsVirtual = false;
    //    jsonPropertyInfoValues.DeclaringType = typeof(Range);
    //    jsonPropertyInfoValues.Converter = null;
    //    jsonPropertyInfoValues.Getter = (object obj) => ((Range)obj).End;
    //    jsonPropertyInfoValues.Setter = null;
    //    jsonPropertyInfoValues.IgnoreCondition = null;
    //    jsonPropertyInfoValues.HasJsonInclude = false;
    //    jsonPropertyInfoValues.IsExtensionData = false;
    //    jsonPropertyInfoValues.NumberHandling = null;
    //    jsonPropertyInfoValues.PropertyName = "End";
    //    jsonPropertyInfoValues.JsonPropertyName = null;
    //    JsonPropertyInfoValues<Index> info0 = jsonPropertyInfoValues;
    //    JsonPropertyInfo propertyInfo0 = (properties[0] = JsonMetadataServices.CreatePropertyInfo(options, info0));
    //    jsonPropertyInfoValues = new JsonPropertyInfoValues<Index>();
    //    jsonPropertyInfoValues.IsProperty = true;
    //    jsonPropertyInfoValues.IsPublic = true;
    //    jsonPropertyInfoValues.IsVirtual = false;
    //    jsonPropertyInfoValues.DeclaringType = typeof(Range);
    //    jsonPropertyInfoValues.Converter = null;
    //    jsonPropertyInfoValues.Getter = (object obj) => ((Range)obj).Start;
    //    jsonPropertyInfoValues.Setter = null;
    //    jsonPropertyInfoValues.IgnoreCondition = null;
    //    jsonPropertyInfoValues.HasJsonInclude = false;
    //    jsonPropertyInfoValues.IsExtensionData = false;
    //    jsonPropertyInfoValues.NumberHandling = null;
    //    jsonPropertyInfoValues.PropertyName = "Start";
    //    jsonPropertyInfoValues.JsonPropertyName = null;
    //    JsonPropertyInfoValues<Index> info1 = jsonPropertyInfoValues;
    //    JsonPropertyInfo propertyInfo1 = (properties[1] = JsonMetadataServices.CreatePropertyInfo(options, info1));
    //    JsonPropertyInfoValues<int> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<int>();
    //    jsonPropertyInfoValues2.IsProperty = false;
    //    jsonPropertyInfoValues2.IsPublic = false;
    //    jsonPropertyInfoValues2.IsVirtual = false;
    //    jsonPropertyInfoValues2.DeclaringType = typeof(Range);
    //    jsonPropertyInfoValues2.Converter = null;
    //    jsonPropertyInfoValues2.Getter = null;
    //    jsonPropertyInfoValues2.Setter = null;
    //    jsonPropertyInfoValues2.IgnoreCondition = null;
    //    jsonPropertyInfoValues2.HasJsonInclude = false;
    //    jsonPropertyInfoValues2.IsExtensionData = false;
    //    jsonPropertyInfoValues2.NumberHandling = null;
    //    jsonPropertyInfoValues2.PropertyName = "_dummyPrimitive";
    //    jsonPropertyInfoValues2.JsonPropertyName = null;
    //    JsonPropertyInfoValues<int> info2 = jsonPropertyInfoValues2;
    //    JsonPropertyInfo propertyInfo2 = (properties[2] = JsonMetadataServices.CreatePropertyInfo(options, info2));
    //    return properties;
    //}

    //private void RangeSerializeHandler(Utf8JsonWriter writer, Range value) {
    //    writer.WriteStartObject();
    //    writer.WritePropertyName(PropName_End);
    //    Range range = value;
    //    IndexSerializeHandler(writer, range.End);
    //    writer.WritePropertyName(PropName_Start);
    //    range = value;
    //    IndexSerializeHandler(writer, range.Start);
    //    writer.WriteEndObject();
    //}

    private JsonTypeInfo<StringSlice> Create_StringSlice(JsonSerializerOptions options, bool makeReadOnly) {
        JsonSerializerOptions localOptions = options;
        JsonTypeInfo<StringSlice> jsonTypeInfo;
        if ((0<localOptions.Converters.Count)
            && ( (GetRuntimeProvidedCustomConverter(localOptions, typeof(StringSlice))) 
                is JsonConverter customConverter)
            ) {
            jsonTypeInfo = JsonMetadataServices.CreateValueInfo<StringSlice>(localOptions, customConverter!);
        } else {
            JsonObjectInfoValues<StringSlice> objectInfo = new JsonObjectInfoValues<StringSlice> {
                ObjectCreator = () => new StringSlice(),
                ObjectWithParameterizedConstructorCreator = null,
                PropertyMetadataInitializer = (JsonSerializerContext _) => new JsonPropertyInfo[0],//StringSlicePropInit(localOptions),
                ConstructorParameterMetadataInitializer = null,
                NumberHandling = JsonNumberHandling.Strict,
                SerializeHandler = StringSliceSerializeHandler
            };
            jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(localOptions, objectInfo);
        }
        if (makeReadOnly) {
            jsonTypeInfo.MakeReadOnly();
        }
        return jsonTypeInfo;
    }

    //private static JsonPropertyInfo[] StringSlicePropInit(JsonSerializerOptions options) {
    //    JsonPropertyInfo[] properties = new JsonPropertyInfo[3];
    //    JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
    //    jsonPropertyInfoValues.IsProperty = true;
    //    jsonPropertyInfoValues.IsPublic = true;
    //    jsonPropertyInfoValues.IsVirtual = false;
    //    jsonPropertyInfoValues.DeclaringType = typeof(StringSlice);
    //    jsonPropertyInfoValues.Converter = null;
    //    jsonPropertyInfoValues.Getter = null;
    //    jsonPropertyInfoValues.Setter = null;
    //    jsonPropertyInfoValues.IgnoreCondition = JsonIgnoreCondition.Always;
    //    jsonPropertyInfoValues.HasJsonInclude = false;
    //    jsonPropertyInfoValues.IsExtensionData = false;
    //    jsonPropertyInfoValues.NumberHandling = null;
    //    jsonPropertyInfoValues.PropertyName = "Length";
    //    jsonPropertyInfoValues.JsonPropertyName = null;
    //    JsonPropertyInfoValues<int> info0 = jsonPropertyInfoValues;
    //    JsonPropertyInfo propertyInfo0 = (properties[0] = JsonMetadataServices.CreatePropertyInfo(options, info0));
    //    JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
    //    jsonPropertyInfoValues2.IsProperty = false;
    //    jsonPropertyInfoValues2.IsPublic = true;
    //    jsonPropertyInfoValues2.IsVirtual = false;
    //    jsonPropertyInfoValues2.DeclaringType = typeof(StringSlice);
    //    jsonPropertyInfoValues2.Converter = null;
    //    jsonPropertyInfoValues2.Getter = (object obj) => ((StringSlice)obj).Text;
    //    jsonPropertyInfoValues2.Setter = delegate {
    //        throw new InvalidOperationException("The member 'StringSlice.Text' has been annotated with the JsonIncludeAttribute but is not visible to the source generator.");
    //    };
    //    jsonPropertyInfoValues2.IgnoreCondition = null;
    //    jsonPropertyInfoValues2.HasJsonInclude = true;
    //    jsonPropertyInfoValues2.IsExtensionData = false;
    //    jsonPropertyInfoValues2.NumberHandling = null;
    //    jsonPropertyInfoValues2.PropertyName = "Text";
    //    jsonPropertyInfoValues2.JsonPropertyName = null;
    //    JsonPropertyInfoValues<string> info1 = jsonPropertyInfoValues2;
    //    JsonPropertyInfo propertyInfo1 = (properties[1] = JsonMetadataServices.CreatePropertyInfo(options, info1));
    //    JsonPropertyInfoValues<Range> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<Range>();
    //    jsonPropertyInfoValues3.IsProperty = false;
    //    jsonPropertyInfoValues3.IsPublic = true;
    //    jsonPropertyInfoValues3.IsVirtual = false;
    //    jsonPropertyInfoValues3.DeclaringType = typeof(StringSlice);
    //    jsonPropertyInfoValues3.Converter = null;
    //    jsonPropertyInfoValues3.Getter = (object obj) => ((StringSlice)obj).Range;
    //    jsonPropertyInfoValues3.Setter = null;
    //    jsonPropertyInfoValues3.IgnoreCondition = null;
    //    jsonPropertyInfoValues3.HasJsonInclude = false;
    //    jsonPropertyInfoValues3.IsExtensionData = false;
    //    jsonPropertyInfoValues3.NumberHandling = null;
    //    jsonPropertyInfoValues3.PropertyName = "Range";
    //    jsonPropertyInfoValues3.JsonPropertyName = null;
    //    JsonPropertyInfoValues<Range> info2 = jsonPropertyInfoValues3;
    //    JsonPropertyInfo propertyInfo2 = (properties[2] = JsonMetadataServices.CreatePropertyInfo(options, info2));
    //    return properties;
    //}

    private void StringSliceSerializeHandler(Utf8JsonWriter writer, StringSlice value) {
        writer.WriteStringValue(value.Text);
    }

    public StringSliceJsonContext()
        : base(null) {
    }

    public StringSliceJsonContext(JsonSerializerOptions options)
        : base(options) {
    }

    private static JsonConverter? GetRuntimeProvidedCustomConverter(JsonSerializerOptions options, Type type) {
        IList<JsonConverter> converters = options.Converters;
        for (int i = 0; i < converters.Count; i++) {
            JsonConverter converter = converters[i];
            if (!converter.CanConvert(type)) {
                continue;
            }
            if (converter is JsonConverterFactory factory) {
                var converterForType = factory.CreateConverter(type, options);
                if (converterForType == null || converterForType is JsonConverterFactory) {
                    throw new InvalidOperationException($"The converter '{factory.GetType()}' cannot return null or a JsonConverterFactory instance.");
                }
                return converterForType;
            }
            return converter;
        }
        return null;
    }

    public override JsonTypeInfo? GetTypeInfo(Type type) {
        if (type == typeof(StringSlice)) {
            return StringSlice;
        }
        return null;
    }

    JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options) {
        if (type == typeof(StringSlice)) {
            return Create_StringSlice(options, makeReadOnly: false);
        }
        return null;
    }
}

#endif