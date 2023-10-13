using CosmoDust.Linq;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmoDust.Cosmos.Tests;

public class CosmosDatabaseTests : IClassFixture<CosmosTextFixture>
{
    public class AccountPlan
    {
        public AccountPlan(string id)
        {
            Id = id;
        }

        public string Id { get; set; } = "";
        public string Name { get; set; } = "Test Plan";
    }

    public class BlogPost
    {
        private int _likes = 0;

        public string Id { get; set; } = "";
        public string Title { get; set; } = "Test Post";
        public string PostId { get; set; } = "";

        public int GetLikes()
        {
            return _likes;
        }

        public int Like()
        {
            return ++_likes;
        }
    }

    public class BlogPostComment
    {
        public string Id { get; set; } = "";
        public string PostId { get; set; } = "";
        public string Content { get; set; } = "Test Comment";
    }

    private readonly CosmosDatabase _db;

    public CosmosDatabaseTests(CosmosTextFixture configurationTextFixture)
    {
        var client = configurationTextFixture.Client;
        var db = client.GetDatabase("reminderdb");
        _db = new CosmosDatabase(db);
    }

    [Fact]
    public async Task Can_Add_And_Find_Entity_In_Separate_Sessions()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");
    }

    [Fact]
    public async Task Can_Add_Find_Delete_Entity_In_Separate_Sessions()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var deleteSession = store.CreateSession();
        var deleteEntity = await deleteSession.FindAsync<AccountPlan>(entity.Id);

        deleteEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");
        deleteEntity.Should().BeEquivalentTo(entity, because: "we should be able to find the entity we just created");

        deleteSession.Remove(deleteEntity!);
        await deleteSession.CommitAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().BeNull(because: "we should not be able to find the entity we just deleted");
    }

    [Fact]
    public async Task Can_Add_Find_Update_Entity_In_Separate_Session()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entity = new AccountPlan(Guid.NewGuid().ToString());

        var writeSession = store.CreateSession();

        writeSession.Store(entity);
        await writeSession.CommitAsync();

        var updateSession = store.CreateSession();
        var updatedEntity = await updateSession.FindAsync<AccountPlan>(entity.Id);

        updatedEntity.Should().NotBeSameAs(entity, because: "the entity should be a different instance since it is from a different session");
        updatedEntity.Should().NotBeNull(because: "we should be able to find the entity we just created");

        updatedEntity!.Name = "Updated Plan";
        updateSession.Update(updatedEntity);

        await updateSession.CommitAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.FindAsync<AccountPlan>(entity.Id);

        readEntity.Should().NotBeSameAs(updatedEntity, because: "the entity should be a different instance since it is from a different session");
        readEntity.Should().NotBeNull(because: "we should be able to find the entity we just updated");
        readEntity!.Name.Should().Be(updatedEntity.Name, because: "we should have updated the entity name");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_List()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entities = new[] { new AccountPlan(Guid.NewGuid().ToString()), new AccountPlan(Guid.NewGuid().ToString()) };

        var writeSession = store.CreateSession();

        foreach (var entity in entities)
            writeSession.Store(entity);

        await writeSession.CommitAsync();

        var readSession = store.CreateSession();
        var readEntities = await readSession.Query<AccountPlan>()
                                .Where(p => p.Id == entities[0].Id || p.Id == entities[1].Id)
                                .ToListAsync();

        readEntities.Should().HaveCount(2, because: "we should have found the two entities we just created");
        readEntities.Should().BeEquivalentTo(entities, because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Can_Execute_Linq_Query_As_AsyncEnumerable()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entities = new[] { new AccountPlan(Guid.NewGuid().ToString()), new AccountPlan(Guid.NewGuid().ToString()) };

        var writeSession = store.CreateSession();

        foreach (var entity in entities)
            writeSession.Store(entity);

        await writeSession.CommitAsync();

        var readSession = store.CreateSession();
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
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<AccountPlan>()
                       .HasId(e => e.Id)
                       .ToContainer("accountPlans");
            });

        var entities = new[] { new AccountPlan(Guid.NewGuid().ToString()), new AccountPlan(Guid.NewGuid().ToString()) };

        var writeSession = store.CreateSession();

        foreach (var entity in entities)
            writeSession.Store(entity);

        await writeSession.CommitAsync();

        var readSession = store.CreateSession();
        var readEntity = await readSession.Query<AccountPlan>()
                             .Where(p => p.Id == entities[0].Id || p.Id == entities[1].Id)
                             .FirstOrDefaultAsync();

        readEntity.Should().NotBeNull(because: "we should have found the entity we just created");
        readEntity.Should().BeEquivalentTo(entities[0], because: "we should be able to query the entities we just created");
    }

    [Fact]
    public async Task Transactional_Batch()
    {
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<BlogPost>()
                       .HasId(e => e.Id)
                       .HasPartitionKey(e => e.PostId)
                       .ToContainer("blogPosts");

                builder.Entity<BlogPostComment>()
                       .HasId(e => e.Id)
                       .HasPartitionKey(e => e.PostId)
                       .ToContainer("blogPosts");
            });

        var session = store.CreateSession();

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
        var store = new DocumentStore(_db)
            .ConfigureModel(builder =>
            {
                builder.Entity<BlogPost>()
                       .HasId(e => e.Id)
                       .HasPartitionKey(e => e.PostId)
                       .HasField("_commentIdSeed")
                       .ToContainer("blogPosts");
            });

        var writeSession = store.CreateSession();

        var postId = Guid.NewGuid().ToString();
        var post = new BlogPost { Id = postId, PostId = postId };

        post.GetLikes().Should().Be(0, because: "the post should have zero likes");

        post.Like();

        post.GetLikes().Should().Be(1, because: "the post should have one like");

        writeSession.Store(post);

        await writeSession.CommitAsync();

        var readSession = store.CreateSession();

        var readPost = await readSession.FindAsync<BlogPost>(postId);

        readPost.Should().NotBeNull(because: "we should be able to find the post we just created");
        readPost!.GetLikes().Should().Be(1, because: "the post should have one like");
    }
}
