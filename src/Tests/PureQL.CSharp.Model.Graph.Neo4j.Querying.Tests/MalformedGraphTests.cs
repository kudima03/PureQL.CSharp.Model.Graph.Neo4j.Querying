using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

/// <summary>
/// Graphs no insertion writes, created in a transaction that is rolled back, so that the
/// stored samples stay as they are.
/// </summary>
[Collection(Neo4jServer.Name)]
public sealed class MalformedGraphTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task RejectsAGraphWithoutRoot()
    {
        await AssertRejectedAsync(
            "CREATE (:PaginationSkip:Node {queryId: $id})",
            "no node"
        );
    }

    [Fact]
    public async Task RejectsAGraphWithTwoRoots()
    {
        await AssertRejectedAsync(
            """
            CREATE (:MainPlainQuery:Node:Query {queryId: $id}),
                   (:MainPlainQuery:Node:Query {queryId: $id})
            """,
            "more than one"
        );
    }

    [Fact]
    public async Task RejectsANodeWithoutRecordLabel()
    {
        await AssertRejectedAsync("CREATE (:Node:Query {queryId: $id})", "record label");
    }

    [Fact]
    public async Task RejectsARelationshipLeavingTheQuery()
    {
        await AssertRejectedAsync(
            """
            CREATE (:MainPlainQuery:Node:Query {queryId: $id})
                   -[:Where]->(:TrueLiteral:Node {queryId: $other})
            """,
            "outside the query"
        );
    }

    [Fact]
    public async Task RejectsAMalformedGraphAmongSeveralQueries()
    {
        Guid id = Guid.NewGuid();
        await using IAsyncSession session = fixture.Session();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        await CreateAsync(transaction, "CREATE (:Node:Query {queryId: $id})", id);

        _ = await Assert.ThrowsAsync<GraphMappingException>(() =>
            QueriesByIdsTests.ReadAsync(
                new QueriesByIds(transaction, [id, fixture.Ids.First().Value]).Values()
            )
        );

        await transaction.RollbackAsync();
    }

    private async Task AssertRejectedAsync(string cypher, string message)
    {
        Guid id = Guid.NewGuid();
        await using IAsyncSession session = fixture.Session();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        await CreateAsync(transaction, cypher, id);

        GraphMappingException exception = await Assert.ThrowsAsync<GraphMappingException>(
            () =>
                new QueryById(transaction, id).ValueAsync()
        );

        Assert.Contains(message, exception.Message, StringComparison.Ordinal);
        await transaction.RollbackAsync();
    }

    private static async Task CreateAsync(
        IAsyncQueryRunner runner,
        string cypher,
        Guid id
    )
    {
        IResultCursor cursor = await runner.RunAsync(
            cypher,
            new Dictionary<string, object>
            {
                ["id"] = id.ToString("D"),
                ["other"] = Guid.NewGuid().ToString("D"),
            }
        );
        _ = await cursor.ConsumeAsync();
    }
}
