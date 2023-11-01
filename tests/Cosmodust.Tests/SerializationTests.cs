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
}
