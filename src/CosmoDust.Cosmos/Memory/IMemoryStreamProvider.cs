namespace CosmoDust.Cosmos.Memory;

public interface IMemoryStreamProvider
{
    MemoryStream GetMemoryStream(string tag = "");
}
