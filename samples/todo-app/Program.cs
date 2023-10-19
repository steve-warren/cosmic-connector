using Cosmodust;
using Cosmodust.Cosmos;
using Cosmodust.Cosmos.Json;
using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Store;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new EntityConfigurationHolder());
builder.Services.AddSingleton(sp =>
{
    return new CosmosClient(sp.GetRequiredService<IConfiguration>()["ConnectionStrings:CosmosDB"], new CosmosClientOptions()
    {
        Serializer = new CosmosJsonSerializer(new IJsonTypeModifier[]
            {
                new BackingFieldJsonTypeModifier(sp.GetRequiredService<EntityConfigurationHolder>()),
                new PropertyJsonTypeModifier(sp.GetRequiredService<EntityConfigurationHolder>()),
                new TypeMetadataJsonTypeModifier()
            })
    });
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    var database = new CosmosDatabase(client.GetDatabase("reminderdb"));

    var store = new DocumentStore(database, sp.GetRequiredService<EntityConfigurationHolder>());

    store.ConfigureModel(modelBuilder =>
    {
        modelBuilder.Entity<TodoList>()
            .HasId(e => e.Id)
            .HasPartitionKey(e => e.Id)
            .HasProperty("Items")
            .ToContainer("todo");

        modelBuilder.Entity<TodoItem>()
            .HasId(e => e.Id)
            .HasPartitionKey(e => e.ListId)
            .ToContainer("todo");
    });

    return store;
});

builder.Services.AddScoped(sp =>
{
    var store = sp.GetRequiredService<DocumentStore>();
    return store.CreateSession();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
