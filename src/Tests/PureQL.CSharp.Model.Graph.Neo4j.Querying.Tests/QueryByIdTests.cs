using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

[Collection(Neo4jServer.Name)]
public sealed class QueryByIdTests(Neo4jFixture fixture)
{
    [Fact]
    public void EverySampleIsStored()
    {
        Assert.Equal(86, fixture.Ids.Count);
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public async Task ReadsEverySampleBackAsTheSameQuery(string name)
    {
        Guid id = fixture.Ids[name];
        await using IAsyncSession session = fixture.Session();

        StoredQuery? stored = await new QueryById(session, id).ValueAsync();

        Assert.NotNull(stored);
        Assert.Equal(id, stored.Id);
        Assert.Equal(
            PureQLJson.Serialize(QuerySamples.All[name], typeof(PureQLQuery)),
            PureQLJson.Serialize(stored.Query, typeof(PureQLQuery))
        );
    }

    [Fact]
    public async Task ReadsNothingForAnUnknownId()
    {
        await using IAsyncSession session = fixture.Session();

        Assert.Null(await new QueryById(session, Guid.NewGuid()).ValueAsync());
    }

    [Fact]
    public async Task ReadsInATransaction()
    {
        KeyValuePair<string, Guid> sample = fixture.Ids.First();
        await using IAsyncSession session = fixture.Session();

        StoredQuery? stored = await session.ExecuteReadAsync(transaction =>
            new QueryById(transaction, sample.Value).ValueAsync()
        );

        Assert.Equal(
            PureQLJson.Serialize(QuerySamples.All[sample.Key], typeof(PureQLQuery)),
            PureQLJson.Serialize(stored!.Query, typeof(PureQLQuery))
        );
    }

    [Fact]
    public async Task ReadsThroughTheStore()
    {
        KeyValuePair<string, Guid> sample = fixture.Ids.Last();

        StoredQuery? stored = await new QueryByIdOf(
            fixture.Store,
            sample.Value
        ).ValueAsync();

        Assert.Equal(sample.Value, stored!.Id);
        Assert.Equal(
            PureQLJson.Serialize(QuerySamples.All[sample.Key], typeof(PureQLQuery)),
            PureQLJson.Serialize(stored.Query, typeof(PureQLQuery))
        );
    }

    [Fact]
    public async Task ReadsNothingThroughTheStoreForAnUnknownId()
    {
        Assert.Null(await new QueryByIdOf(fixture.Store, Guid.NewGuid()).ValueAsync());
    }

    [Fact]
    public async Task StopsWhenCancelled()
    {
        await using IAsyncSession session = fixture.Session();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new QueryById(session, fixture.Ids.First().Value).ValueAsync(
                new CancellationToken(true)
            )
        );
    }

    public static TheoryData<string> Samples()
    {
        return QuerySamples.Names();
    }
}
