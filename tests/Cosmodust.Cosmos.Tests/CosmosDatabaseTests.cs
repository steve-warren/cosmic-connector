using System.Configuration.Internal;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Cosmos.Json;
using Cosmodust.Cosmos.Tests.Domain.Accounts;
using Cosmodust.Cosmos.Tests.Domain.Blogs;
using Cosmodust.Json;
using Cosmodust.Linq;
using Cosmodust.Store;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Cosmodust.Cosmos.Tests;

public class CosmosDatabaseTests : IClassFixture<CosmosTextFixture>
{
    private readonly IConfiguration _configuration;
    private readonly IDocumentStore _store;

    public CosmosDatabaseTests(CosmosTextFixture configurationTextFixture)
    {
        _configuration = configurationTextFixture.Configuration;

        var entityConfiguration = new EntityConfigurationHolder();

        var jsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();

        foreach (var action in new IJsonTypeModifier[]
                 {
                     new BackingFieldJsonTypeModifier(entityConfiguration),
                     new PropertyJsonTypeModifier(entityConfiguration),
                     new TypeMetadataJsonTypeModifier()
                 })
        {
            jsonTypeInfoResolver.Modifiers.Add(action.Modify);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = jsonTypeInfoResolver
        };

        var cosmosClient = new CosmosClient(_configuration["COSMOSDB_CONNECTIONSTRING"], new CosmosClientOptions()
        {
            Serializer = new CosmosJsonSerializer(options)
        });

        var db = cosmosClient.GetDatabase("reminderdb");

        _store = new DocumentStore(new CosmosDatabase(db), options, entityConfiguration)
                    .BuildModel(builder =>
                    {
                        builder.HasEntity<AccountPlan>()
                            .HasId(e => e.Id)
                            .ToContainer("accountPlans");

                        builder.HasEntity<BlogPost>()
                            .HasId(e => e.Id)
                            .HasPartitionKey(e => e.PostId)
                            .HasField("_likes")
                            .ToContainer("blogPosts");

                        builder.HasEntity<BlogPostComment>()
                            .HasId(e => e.Id)
                            .HasPartitionKey(e => e.PostId)
                            .ToContainer("blogPosts");
                    });
    }

    [Fact]
    public async Task Can_Add_And_Find_Entity_In_Separate_Sessions()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");
    }

    [Fact]
    public async Task Can_Add_Find_Delete_Entity_In_Separate_Sessions()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var deleteSession = _store.CreateSession();
        var deleteEntity = await deleteSession.FindAsync<AccountPlan>(entity.Id);

        deleteEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");
        deleteEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");

        deleteSession.Remove(deleteEntity!);
        await deleteSession.CommitAsync();

        var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeNull(because: "we should not be able to find the entity we just deleted");
    }

    [Fact]
    public async Task Can_Add_Find_Update_Entity_In_Separate_Session()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var updateSession = _store.CreateSession();
        var updatedEntity = await updateSession.FindAsync<AccountPlan>(entity.Id);

        updatedEntity.Should().NotBeSameAs(entity, because: "the entity should be a different instance since it is from a different session");
        updatedEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");

        updatedEntity!.Name = "Updated Plan";
        updateSession.Update(updatedEntity);

        await updateSession.CommitAsync();

        var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().NotBeSameAs(updatedEntity, because: "the entity should be a different instance since it is from a different session");
        readEntity.Should().NotBeNull(because: "we should be able to find the entity we just updated");
        readEntity!.Name.Should().Be(updatedEntity.Name, because: "we should have updated the entity name");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_List()
    {
        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        var comments = new[]
        {
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 1" },
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 2" }
        };
        
        var writeSession = _store.CreateSession();

        writeSession.Store(post);
        writeSession.Store(comments[0]);
        writeSession.Store(comments[1]);
        
        await writeSession.CommitTransactionAsync();

        var readSession = _store.CreateSession();
        var readEntities = await readSession.Query<BlogPostComment>(partitionKey: postId)
            .Where(comment => comment.PostId == postId)
            .ToListAsync();

        readEntities.Should().HaveCount(2, because: "we should have found the two entities we just created");
        readEntities.Should().BeEquivalentTo(comments, because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_AsyncEnumerable()
    {
        var entities = new[] { new AccountPlan(Guid.NewGuid().ToString()), new AccountPlan(Guid.NewGuid().ToString()) };

        var writeSession = _store.CreateSession();

        foreach (var entity in entities)
            writeSession.Store(entity);

        await writeSession.CommitAsync();

        var readSession = _store.CreateSession();
        var readEntities = readSession.Query<AccountPlan>()
                           .Where(p => p.Id == entities[0].Id || p.Id == entities[1].Id)
                           .ToAsyncEnumerable();

        var list = new List<AccountPlan>();

        await foreach (var entity in readEntities)
            list.Add(entity);

        list.Should().HaveCount(2, because: "we should have found the two entities we just created");
        list.Should().BeEquivalentTo(entities, because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_FirstOrDefault()
    {
        var entities = new[] { new AccountPlan(Guid.NewGuid().ToString()), new AccountPlan(Guid.NewGuid().ToString()) };

        var writeSession = _store.CreateSession();

        foreach (var entity in entities)
            writeSession.Store(entity);

        await writeSession.CommitAsync();

        var readSession = _store.CreateSession();
        var readEntity = await readSession.Query<AccountPlan>()
                             .Where(p => p.Id == entities[0].Id || p.Id == entities[1].Id)
                             .FirstOrDefaultAsync();

        readEntity.Should().NotBeNull(because: "we should have found the entity we just created");
        readEntity.Should().BeEquivalentTo(entities[0], because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Transactional_Batch()
    {
        var session = _store.CreateSession();

        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };
        var comment = new BlogPostComment { Id = Guid.NewGuid().ToString(), PostId = post.Id };

        session.Store(post);
        session.Store(comment);

        await session.CommitTransactionAsync();
    }

    [Fact]
    public async Task Backing_Field()
    {
        var writeSession = _store.CreateSession();

        var postId = DateTime.Now.ToFileTime().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        post.Like();
        post.GetLikes().Should().Be(1, because: "the post should have one like");
        post.PublishOn(DateTimeOffset.Now);

        writeSession.Store(post);

        await writeSession.CommitAsync();

        var readSession = _store.CreateSession();

        var readPost = await readSession.FindAsync<BlogPost>(postId);

        readPost.Should().NotBeNull(because: "we should be able to find the post we just created");
        readPost!.GetLikes().Should().Be(1, because: "the post should have one like");
    }
}
