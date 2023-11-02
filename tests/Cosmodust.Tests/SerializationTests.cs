using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Json;
using Cosmodust.Serialization;
using Cosmodust.Store;

namespace Cosmodust.Tests;

public class SerializationTests
{
    public class BackingFieldEntity
    {
        private string _firstName;
        private string _lastName;
        
        public BackingFieldEntity(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
        }

        public override string ToString()
        {
            return $"{_firstName} {_lastName}";
        }
    }

    [Fact]
    public void Should_Serialize_Private_Mutable_Fields()
    {
        using var stream = new MemoryStream();
        var entity = new BackingFieldEntity(firstName: "Michael", lastName: "Scott");
        var configuration = new EntityConfigurationProvider();
        configuration.AddEntityConfiguration(new EntityConfiguration(typeof(BackingFieldEntity))
        {
            Fields = new[]
            {
                FieldAccessor.Create("_firstName", typeof(BackingFieldEntity)),
                FieldAccessor.Create("_lastName", typeof(BackingFieldEntity)), 
            }
        });
        
        JsonSerializer.Serialize(stream, entity, typeof(BackingFieldEntity),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                {
                    Modifiers = { new BackingFieldJsonTypeModifier(configuration).Modify }
                }
            });

        stream.Position = 0;

        var reader = new StreamReader(stream);
        var json = reader.ReadLine();

        json.Should().Be("""{"_firstName":"Michael","_lastName":"Scott"}""", because: "we should be able to serialize private fields");
    }

    public record EmptyType();

    [Fact]
    public void Should_Serialize_Object_Type_To_Json()
    {
        using var stream = new MemoryStream();
        var entity = new EmptyType();

        JsonSerializer.Serialize(stream, entity, typeof(EmptyType),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                {
                    Modifiers = { new TypeMetadataJsonTypeModifier().Modify }
                }
            });

        stream.Position = 0;

        var reader = new StreamReader(stream);
        var json = reader.ReadLine();

        json.Should().Be("""{"__type":"EmptyType"}""", because: "we should be able to serialize the object's type.");
    }

    public record ArchiveState
    {
        public static readonly ArchiveState NotArchived = new() { Name = nameof(NotArchived) };
        public static readonly ArchiveState Archived = new() { Name = nameof(Archived) };

        public static ArchiveState Parse(string name)
        {
            return name switch
            {
                nameof(NotArchived) => NotArchived,
                nameof(Archived) => Archived,
                _ => throw new ArgumentException("invalid state", nameof(name))
            };
        }

        private ArchiveState() { }

        public string Name { get; private init; } = "";

        public override string ToString() =>
            Name;
    }

    public class FooEntity
    {
        public string Id { get; set; } = "123";
        public ArchiveState State { get; set; } = ArchiveState.Archived;
    }

    [Fact]
    public void Can_Serialize_ValueObject()
    {
        using var stream = new MemoryStream();
        var entity = new FooEntity();

        var options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new ValueObjectJsonConverter<ArchiveState>()
                }
            };

        JsonSerializer.Serialize(stream, entity, typeof(FooEntity), options);

        stream.Position = 0;

        var reader = new StreamReader(stream);
        var json = reader.ReadLine();

        json.Should().Be("""{"id":"123","state":"Archived"}""", because: "we should be able to serialize the value type.");
    }
    
    [Fact]
    public void Can_Deserialize_ValueObject()
    {
        var options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new ValueObjectJsonConverter<ArchiveState>()
                }
            };

        var entity = new FooEntity();

        var json = """{"id":"123","state":"Archived"}""";
        var deserializedEntity = JsonSerializer.Deserialize<FooEntity>(json, options);

        deserializedEntity.Should().BeEquivalentTo(entity, because: "we should be able to deserialize the value type.");
    }

    [Fact]
    public void Can_Extract_Parameters_From_Anonymous_Types()
    {
        var type = new
        {
            Id = "123",
            NumberOfItems = 10,
            TimeStamp = new DateTime(year: 2000, month: 01, day: 01)
        };

        var cache = new SqlParameterObjectTypeCache();
        var parameters = cache.ExtractParametersFromObject(type).ToList();

        parameters[0].Should().Be(("@" + nameof(type.Id), type.Id), because: "the id property name and value must match.");
        parameters[1].Should().Be(("@" + nameof(type.NumberOfItems), type.NumberOfItems), because: "the id property name and value must match.");
        parameters[2].Should().Be(("@" + nameof(type.TimeStamp), type.TimeStamp), because: "the id property name and value must match.");
    }

    [Fact]
    public void TestTransformAndCopyJson()
    {
        // Arrange
        const string InputJson =
"""
{
"_rid": "EmYyALEelj8=",
"Documents": [
  {
    "id": "l_2XcnZ1q9VjJJSEzlsU1aTShFYTG",
    "ownerId": "a_2XcnUpI87sTHRKbeDBEqov2Qekd",
    "name": "super-fast list",
    "count": 0,
    "items": [],
    "archiveState": "NotArchived",
    "__type": "TodoList",
    "_etag": "\u00229801e7d6-0000-0400-0000-6543b9c80000\u0022"
  },
  {
    "id": "l_2Xcna2S7OSh5fPZGtXRs2x3tCfc",
    "ownerId": "a_2XcnUpI87sTHRKbeDBEqov2Qekd",
    "name": "super-fast list",
    "count": 0,
    "items": [],
    "archiveState": "NotArchived",
    "__type": "TodoList",
    "_etag": "\u0022980150d7-0000-0400-0000-6543b9d00000\u0022"
  }
],
"_Count": 2
}
""";
        const string ExpectedOutputJson =
"""
{
  "items": [
    {
      "id": "l_2XcnZ1q9VjJJSEzlsU1aTShFYTG",
      "ownerId": "a_2XcnUpI87sTHRKbeDBEqov2Qekd",
      "name": "super-fast list",
      "count": 0,
      "items": [],
      "archiveState": "NotArchived",
      "__type": "TodoList",
      "_etag": "\u00229801e7d6-0000-0400-0000-6543b9c80000\u0022"
    },
    {
      "id": "l_2Xcna2S7OSh5fPZGtXRs2x3tCfc",
      "ownerId": "a_2XcnUpI87sTHRKbeDBEqov2Qekd",
      "name": "super-fast list",
      "count": 0,
      "items": [],
      "archiveState": "NotArchived",
      "__type": "TodoList",
      "_etag": "\u0022980150d7-0000-0400-0000-6543b9d00000\u0022"
    }
  ],
  "count": 2
}
""";
        
        var propertyMappings = new Dictionary<string, string> { { "Documents", "items" }, { "_Count", "count" } };
        var skip = new Dictionary<string, string?>
        {
            { "_rid", null },
            { "_self", null },
            { "_attachments", null },
            { "_ts", null }
        };
        
        using var inputMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(InputJson));
        using var outputMemoryStream = new MemoryStream();

        var readerOptions = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip };
        var reader = new Utf8JsonReader(inputMemoryStream.ToArray(), readerOptions);
        
        using var writer = new Utf8JsonWriter(outputMemoryStream, new JsonWriterOptions
        {
            Indented = true,
            SkipValidation = true
        });

        // Act
        TransformAndCopyJson(reader, writer, propertyMappings, skip);
        writer.Flush();
        
        var outputJson = Encoding.UTF8.GetString(outputMemoryStream.ToArray());

        // Assert
        Assert.Equal(ExpectedOutputJson, outputJson);
    }

    private static void TransformAndCopyJson(Utf8JsonReader reader, Utf8JsonWriter writer,
        Dictionary<string, string> propertyMappings,
        Dictionary<string, string?> skip)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var originalPropertyName = reader.GetString();

                    if (skip.ContainsKey(originalPropertyName))
                    {
                        reader.Skip();
                        break;
                    }
                    
                    else if (originalPropertyName == "_etag")
                    {
                        writer.WritePropertyName("_etag");

                        if (reader.Read() && reader.TokenType == JsonTokenType.String)
                        {
                            var etag = reader.GetString();
                            writer.WriteStringValue(etag);
                        }

                        break;
                    }

                    var newPropertyName = propertyMappings.GetValueOrDefault(originalPropertyName, originalPropertyName);
                    writer.WritePropertyName(newPropertyName);
                    continue;
                
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;
                
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;
                
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;

                case JsonTokenType.String:
                    writer.WriteStringValue(reader.ValueSpan);
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                case JsonTokenType.None:
                case JsonTokenType.Comment:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                default:
                    writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
                    break;
            }
        }
    }
}
