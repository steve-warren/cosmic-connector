using System.Configuration.Internal;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Cosmodust.Cosmos.Json;
using Cosmodust.Cosmos.Tests.Domain.Accounts;
using Cosmodust.Cosmos.Tests.Domain.Blogs;
using Cosmodust.Json;
using Cosmodust.Linq;
using Cosmodust.Query;
using Cosmodust.Serialization;
using Cosmodust.Session;
using Cosmodust.Store;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Cosmodust.Cosmos.Tests;

public class CosmosDatabaseTests : IClassFixture<CosmosTextFixture>
{
    private readonly IConfiguration _configuration;
    private readonly DocumentStore _store;
    private readonly QueryFacade _queryFacade;

    public CosmosDatabaseTests(CosmosTextFixture configurationTextFixture)
    {
        _configuration = configurationTextFixture.Configuration;

        var entityConfiguration = new EntityConfigurationHolder();

        var jsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();

        var shadowPropertyCache = new ShadowPropertyCache();
        
        foreach (var action in new IJsonTypeModifier[]
                 {
                     new BackingFieldJsonTypeModifier(entityConfiguration),
                     new PropertyJsonTypeModifier(entityConfiguration),
                     new ShadowPropertyJsonTypeModifier(entityConfiguration, shadowPropertyCache),
                     new TypeMetadataJsonTypeModifier(),
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

        _store = new DocumentStore(
                new CosmosDatabase(db),
                options,
                entityConfiguration,
                shadowPropertyCache: shadowPropertyCache)
                    .BuildModel(builder =>
                    {
                        builder.HasEntity<AccountPlan>()
                            .HasId(e => e.Id)
                            .HasPartitionKey(
                                e => e.Id,
                                "ownerId")
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

        _queryFacade = new QueryFacade(
            client: cosmosClient,
            databaseName: "reminderdb",
            sqlParameterCache: new SqlParameterCache());
    }

    [Fact]
    public async Task Can_Add_And_Find_Entity_In_Separate_Sessions()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        using var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        using var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id, entity.Id);

        readEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");
    }

    [Fact]
    public async Task Can_Add_Find_Delete_Entity_In_Separate_Sessions()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        using var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        using var deleteSession = _store.CreateSession();
        var deleteEntity = await deleteSession.FindAsync<AccountPlan>(entity.Id, entity.Id);

        deleteEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");
        deleteEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");

        deleteSession.Remove(deleteEntity!);
        await deleteSession.CommitAsync();

        using var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id, entity.Id);

        readEntity.Should().BeNull(because: "we should not be able to find the entity we just deleted");
    }

    [Fact]
    public async Task Can_Add_Find_Update_Entity_In_Separate_Session()
    {
        var entity = new AccountPlan(Guid.NewGuid().ToString());

        using var writeSession = _store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        using var updateSession = _store.CreateSession();
        var updatedEntity = await updateSession.FindAsync<AccountPlan>(entity.Id, entity.Id);

        updatedEntity.Should().NotBeSameAs(entity, because: "the entity should be a different instance since it is from a different session");
        updatedEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");

        updatedEntity!.Name = "Updated Plan";
        updateSession.Update(updatedEntity);

        await updateSession.CommitAsync();

        using var readSession = _store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id, entity.Id);

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
        
        using var writeSession = _store.CreateSession();

        writeSession.Store(post);
        writeSession.Store(comments[0]);
        writeSession.Store(comments[1]);
        
        await writeSession.CommitTransactionAsync();

        using var readSession = _store.CreateSession();
        var readEntities = await readSession.Query<BlogPostComment>(partitionKey: postId)
            .Where(comment => comment.PostId == postId)
            .ToListAsync();

        readEntities.Should().HaveCount(2, because: "we should have found the two entities we just created");
        readEntities.Should().BeEquivalentTo(comments, because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_AsyncEnumerable()
    {
        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        var comments = new[]
        {
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 1" },
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 2" }
        };
        
        using var writeSession = _store.CreateSession();

        writeSession.Store(post);
        writeSession.Store(comments[0]);
        writeSession.Store(comments[1]);
        
        await writeSession.CommitTransactionAsync();

        using var readSession = _store.CreateSession();
        var readEntities = readSession.Query<BlogPostComment>(postId)
                           .Where(c => c.PostId == postId)
                           .ToAsyncEnumerable();

        var list = new List<BlogPostComment>();

        await foreach (var entity in readEntities)
            list.Add(entity);

        list.Should().HaveCount(2, because: "we should have found the two entities we just created");
        list.Should().BeEquivalentTo(comments, because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_FirstOrDefault()
    {
        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        var comments = new[]
        {
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 1" },
            new BlogPostComment { PostId = postId, Id = Guid.NewGuid().ToString(), Content = "Comment 2" }
        };

        using var writeSession = _store.CreateSession();

        writeSession.Store(post);
        writeSession.Store(comments[0]);
        writeSession.Store(comments[1]);

        await writeSession.CommitTransactionAsync();

        using var readSession = _store.CreateSession();
        var readEntity = await readSession.Query<BlogPostComment>(postId)
            .Where(c => c.PostId == postId)
            .FirstOrDefaultAsync();

        readEntity.Should().NotBeNull(because: "we should have found the entity we just created");
        readEntity.Should().BeEquivalentTo(comments[0], because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Transactional_Batch()
    {
        using var session = _store.CreateSession();

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
        using var writeSession = _store.CreateSession();

        var postId = DateTime.Now.ToFileTime().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        post.Like();
        post.GetLikes().Should().Be(1, because: "the post should have one like");
        post.PublishOn(DateTimeOffset.Now);

        writeSession.Store(post);

        await writeSession.CommitAsync();

        using var readSession = _store.CreateSession();

        var readPost = await readSession.FindAsync<BlogPost>(postId, postId);

        readPost.Should().NotBeNull(because: "we should be able to find the post we just created");
        readPost!.GetLikes().Should().Be(1, because: "the post should have one like");
    }

    [Fact]
    public async Task Can_Query_Using_Raw_Sql_With_Parameters()
    {
        using var writeSession = _store.CreateSession();

        var postId = Guid.NewGuid().ToString();
        var blogPost = new BlogPost
        {
            Id = postId,
            PostId = postId,
            Title = "🦖Rawr SQL!",
            PublishedOn = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-5))
        };

        writeSession.Store(blogPost);
        await writeSession.CommitAsync();

        using var readSession = _store.CreateSession();

        var query = readSession.Query<BlogPost>(
            partitionKey: postId,
            sql: "select * from c where c.id = @id",
            parameters: new { id = postId });

        var result = await query.FirstOrDefaultAsync();

        result.Should().BeEquivalentTo(expectation: blogPost, because: "the query should return an object by its id.");
    }

    [Fact]
    public async Task Can_Use_Query_Facade()
    {
        var pipe = new Pipe();

        await _queryFacade.ExecuteQueryAsync(
            writer: pipe.Writer,
            containerName: "todo",
            partitionKey: "a_2X011ldw0dogcauAbw0oExAv21H",
            sql: "select * from c where c.ownerId = @ownerId",
            parameters: new { ownerId = "a_2X011ldw0dogcauAbw0oExAv21H" });
    }

    [Fact]
    public void Foo()
    {
        
    }
}
