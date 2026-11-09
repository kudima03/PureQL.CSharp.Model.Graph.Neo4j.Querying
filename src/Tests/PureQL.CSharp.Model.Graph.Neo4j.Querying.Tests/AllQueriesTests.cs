using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

[Collection(Neo4jServer.Name)]
public sealed class AllQueriesTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task ReadsEveryStoredQueryInOrderOfId()
    {
        await using IAsyncSession session = fixture.Session();

        List<StoredQuery> stored = await QueriesByIdsTests.ReadAsync(
            new AllQueries(session).Values()
        );

        Assert.Equal(
            fixture
                .Ids.Values.Select(id => id.ToString("D"))
                .Order(StringComparer.Ordinal),
            stored.Select(query => query.Id.ToString("D"))
        );
        AssertSamples(stored);
    }

    [Fact]
    public async Task ReadsInATransaction()
    {
        await using IAsyncSession session = fixture.Session();

        List<StoredQuery> stored = await session.ExecuteReadAsync(transaction =>
            QueriesByIdsTests.ReadAsync(new AllQueries(transaction).Values())
        );

        Assert.Equal(86, stored.Count);
    }

    [Fact]
    public async Task ReadsThroughTheStore()
    {
        List<StoredQuery> stored = await QueriesByIdsTests.ReadAsync(
            new AllQueriesOf(fixture.Store).Values()
        );

        Assert.Equal(
            fixture.Ids.Values.Order(),
            stored.Select(query => query.Id).Order()
        );
        AssertSamples(stored);
    }

    [Fact]
    public async Task StopsWhenCancelled()
    {
        await using IAsyncSession session = fixture.Session();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            QueriesByIdsTests.ReadAsync(
                new AllQueries(session).Values(new CancellationToken(true))
            )
        );
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
