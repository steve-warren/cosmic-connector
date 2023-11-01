namespace Cosmodust.Cosmos.Tests.Domain.Blogs;

public class BlogPost
{
    private int _likes = 0;

    public string Id { get; init; } = "";
    public string Title { get; set; } = "Test Post";
    public string Url { get; set; } = "";
    public int CommentCount { get; set; }
    public DateTimeOffset PublishedOn { get; set; }

    public int GetLikes()
    {
        return _likes;
    }

    public int Like()
    {
        return ++_likes;
    }

    public void PublishOn(DateTimeOffset date)
    {
        PublishedOn = date;
    }
}
