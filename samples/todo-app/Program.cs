using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Samples.TodoApp.Infra;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddCosmodust(
    options =>
    {
        options.WithConnectionString(builder.Configuration["ConnectionStrings:CosmosDB"])
            .WithDatabase("reminderdb")
            .WithModel(modelBuilder =>
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
    });

services.AddScoped<ITodoListRepository, CosmodustTodoListRepository>()
    .AddScoped<IAccountRepository, CosmodustAccountRepository>()
    .AddScoped<ITodoItemRepository, CosmodustTodoItemRepository>()
    .AddScoped<IUnitOfWork, CosmodustUnitOfWork>()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddControllers();

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
