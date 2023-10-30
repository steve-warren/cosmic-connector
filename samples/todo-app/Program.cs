using System.Text.Json;
using Cosmodust.Cosmos;
using Cosmodust.Json;
using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Samples.TodoApp.Infra;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Store;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCosmodust(
    connectionString: builder.Configuration["ConnectionStrings:CosmosDB"],
    database: "reminderdb");

builder.Services.AddSingleton<DocumentStore>(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    var database = new CosmosDatabase(client.GetDatabase("reminderdb"));
    var options = sp.GetRequiredService<CosmodustJsonSerializer>().Options;

    var store = new DocumentStore(
        database,
        options,
        sp.GetRequiredService<EntityConfigurationProvider>(),
        sqlParameterCache: sp.GetRequiredService<SqlParameterCache>(),
        shadowPropertyStore: sp.GetRequiredService<ShadowPropertyStore>());

    store.BuildModel(modelBuilder =>
    {
        modelBuilder.HasEntity<Account>()
            .HasId(e => e.Id)
            .HasPartitionKey(
                e => e.Id,
                "ownerId")
            .ToContainer("todo");

        modelBuilder.HasEntity<TodoList>()
            .HasId(e => e.Id)
            .HasPartitionKey(e => e.OwnerId)
            .HasProperty("Items")
            .HasShadowProperty<string>("_etag")
            .HasShadowProperty<long>("_ts")
            .ToContainer("todo");

        modelBuilder.HasEntity<TodoItem>()
            .HasId(e => e.Id)
            .HasPartitionKey(e => e.OwnerId)
            .ToContainer("todo");

        modelBuilder.HasValueObject<ArchiveState>()
            .HasValueObject<TodoItemCompletedState>()
            .HasValueObject<TodoItemPriority>();
    });

    return store;
});

builder.Services.AddScoped<ITodoListRepository, CosmodustTodoListRepository>();
builder.Services.AddScoped<IAccountRepository, CosmodustAccountRepository>();
builder.Services.AddScoped<ITodoItemRepository, CosmodustTodoItemRepository>();
builder.Services.AddScoped<IUnitOfWork, CosmodustUnitOfWork>();

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
