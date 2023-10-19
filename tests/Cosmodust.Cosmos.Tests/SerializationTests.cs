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
        public BackingFieldEntity(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }

    [Fact]
    public void Foo()
    {
        var entity = new BackingFieldEntity(firstName: "Michael", lastName: "Scott");

        var serializer = new CosmosJsonSerializer(new IJsonTypeModifier[] { new BackingFieldJsonTypeModifier(new EntityConfigurationHolder()) });
        using var jsonStream = serializer.ToStream(entity);
        var reader = new StreamReader(jsonStream);
        var json = reader.ReadToEnd();

        json.Should().Be("""{"firstName":"Michael","lastName":"Scott"}""", because: "we should be able to serialize private fields");
    }
}
