using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

[Collection(Neo4jServer.Name)]
public sealed class QueriesByIdsTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task ReadsExactlyTheExistingQueries()
    {
        Guid[] existing = [.. fixture.Ids.Values.Take(20)];
        Guid[] ids = [.. existing, Guid.NewGuid(), Guid.NewGuid()];
        await using IAsyncSession session = fixture.Session();

        List<StoredQuery> stored = await ReadAsync(
            new QueriesByIds(session, ids).Values()
        );

        Assert.Equal(existing.Order(), stored.Select(query => query.Id).Order());
        AssertSamples(stored);
    }

    [Fact]
    public async Task ReadsEveryStoredQuery()
    {
        await using IAsyncSession session = fixture.Session();

        List<StoredQuery> stored = await ReadAsync(
            new QueriesByIds(session, [.. fixture.Ids.Values]).Values()
        );

        Assert.Equal(86, stored.Count);
        AssertSamples(stored);
    }

    [Fact]
    public async Task ReadsARepeatedIdOnce()
    {
        Guid id = fixture.Ids.First().Value;
        await using IAsyncSession session = fixture.Session();

        List<StoredQuery> stored = await ReadAsync(
            new QueriesByIds(session, [id, id]).Values()
        );

        Assert.Equal([id], stored.Select(query => query.Id));
    }

    [Fact]
    public async Task ReadsNothingForNoIds()
    {
        await using IAsyncSession session = fixture.Session();

        Assert.Empty(await ReadAsync(new QueriesByIds(session, []).Values()));
    }

    [Fact]
    public async Task ReadsNothingForUnknownIds()
    {
        await using IAsyncSession session = fixture.Session();

        Assert.Empty(
            await ReadAsync(new QueriesByIds(session, [Guid.NewGuid()]).Values())
        );
    }

    [Fact]
    public async Task ReadsThroughTheStore()
    {
        Guid[] ids = [.. fixture.Ids.Values.Skip(40).Take(10)];

        List<StoredQuery> stored = await ReadAsync(
            new QueriesByIdsOf(fixture.Store, [.. ids, Guid.NewGuid()]).Values()
        );

        Assert.Equal(ids.Order(), stored.Select(query => query.Id));
        AssertSamples(stored);
    }

    [Fact]
    public async Task StopsWhenCancelled()
    {
        await using IAsyncSession session = fixture.Session();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(
                new QueriesByIds(session, [.. fixture.Ids.Values]).Values(
                    new CancellationToken(true)
                )
            )
        );
    }

    [Fact]
    public async Task StopsWhenCancelledWhileReading()
    {
        using CancellationTokenSource cancellation = new CancellationTokenSource();
        await using IAsyncSession session = fixture.Session();
        List<StoredQuery> stored = [];

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (
                StoredQuery query in new QueriesByIds(
                    session,
                    [.. fixture.Ids.Values]
                ).Values(cancellation.Token)
            )
            {
                stored.Add(query);
                await cancellation.CancelAsync();
            }
        });
        _ = Assert.Single(stored);
    }

    internal static async Task<List<StoredQuery>> ReadAsync(
        IAsyncEnumerable<StoredQuery> queries
    )
    {
        List<StoredQuery> stored = [];
        await foreach (StoredQuery query in queries)
        {
            stored.Add(query);
        }

        return stored;
    }

    private void AssertSamples(IEnumerable<StoredQuery> stored)
    {
        Dictionary<Guid, string> names = fixture.Ids.ToDictionary(
            sample => sample.Value,
            sample => sample.Key
        );
        foreach (StoredQuery query in stored)
        {
            Assert.Equal(
                PureQLJson.Serialize(
                    QuerySamples.All[names[query.Id]],
                    typeof(PureQLQuery)
                ),
                PureQLJson.Serialize(query.Query, typeof(PureQLQuery))
            );
        }
    }
}
