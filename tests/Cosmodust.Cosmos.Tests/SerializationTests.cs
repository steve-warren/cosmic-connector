using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Cosmos.Json;
using Cosmodust.Store;
using FluentAssertions;

namespace Cosmodust.Cosmos.Tests;

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
        var configuration = new EntityConfigurationHolder();
        configuration.Add(new EntityConfiguration(typeof(BackingFieldEntity))
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
}
