using Cosmodust.Extensions;
using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Samples.TodoApp.Infra;
using KsuidDotNet;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddLogging(configure =>
{
    configure.AddConsole();
});

services.AddCosmodust(
    options =>
    {
        options.WithConnectionString(builder.Configuration["ConnectionStrings:CosmosDB"])
            .WithDatabase("reminderdb")
            .WithModel(modelBuilder =>
            {
                modelBuilder.DefineEntity<Account>()
                    .WithId(e => e.Id)
                    .WithPartitionKey(
                        e => e.Id,
                        "ownerId")
                    .WithDomainEvents("_domainEvents",
                        () => Ksuid.NewKsuid("event_"))
                    .ToContainer("todo");

                modelBuilder.DefineEntity<TodoList>()
                    .WithId(e => e.Id)
                    .WithPartitionKey(e => e.OwnerId)
                    .ToContainer("todo");

                modelBuilder.DefineEntity<TodoItem>()
                    .WithId(e => e.Id)
                    .WithPartitionKey(e => e.OwnerId)
                    .ToContainer("todo");

                modelBuilder.DefineEnumeration<ArchiveState>()
                    .DefineEnumeration<TodoItemCompletedState>()
                    .DefineEnumeration<TodoItemPriority>();
            })
            .WithJsonOptions(jsonOptions =>
            {
                jsonOptions.CamelCase()
                    .SerializePrivateProperties()
                    .SerializePrivateFields()
                    .SerializeEntityTypeInfo();
            })
            .WithQueryOptions(queryOptions =>
            {
                queryOptions.ExcludeCosmosMetadata = true;
                queryOptions.IncludeETag = true;
                queryOptions.IndentJsonOutput = false;
                queryOptions.RenameDocumentCollectionProperties = true;
            });
    });

services.AddScoped<ITodoListRepository, CosmodustTodoListRepository>()
    .AddScoped<IAccountRepository, CosmodustAccountRepository>()
    .AddScoped<ITodoItemRepository, CosmodustTodoItemRepository>()
    .AddScoped<IUnitOfWork, CosmodustUnitOfWork>()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

services.AddControllers()
    .AddJsonOptions(configure =>
    {
        
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
