using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;

namespace Cosmodust.Cosmos.Tests;

public class SerializationTests
{
    public class BackingFieldEntity
    {
        private readonly string _firstName;
        private readonly string _lastName;

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
    public void Foo()
    {
        // var entity = new BackingFieldEntity(firstName: "Michael", lastName: "Scott");

        // var serializer = new CosmosJsonSerializer(new IJsonTypeModifier[] { new BackingFieldJsonTypeModifier(new EntityConfigurationHolder()) });
        // using var jsonStream = serializer.ToStream(entity);
        // var reader = new StreamReader(jsonStream);
        // var json = reader.ReadToEnd();

        // json.Should().Be("""{"_firstName":"Michael","_lastName":"Scott"}""", because: "we should be able to serialize private fields");
    }
}
