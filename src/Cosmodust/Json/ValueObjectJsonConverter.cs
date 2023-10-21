using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosmodust.Json;

public sealed class ValueObjectJsonConverter<TValueObject> : JsonConverter<TValueObject> where TValueObject : class
{
    private readonly Func<string, TValueObject> _parseFunc;

    public ValueObjectJsonConverter()
    {
        var parseMethod = typeof(TValueObject).GetMethod(
            name: "Parse",
            bindingAttr: BindingFlags.Static | BindingFlags.Public,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null);

        if (parseMethod == null)
            throw new InvalidOperationException($"The type {typeof(TValueObject).Name} does not have a suitable static Parse method.");

        var input = Expression.Parameter(typeof(string), "input");
        var parseCall = Expression.Call(null, parseMethod, input);

        _parseFunc = Expression.Lambda<Func<string, TValueObject>>(parseCall, input).Compile();
    }

    public override TValueObject? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();

        return stringValue is null
            ? default
            : _parseFunc(stringValue);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TValueObject value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}
