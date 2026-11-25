using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

/// <summary>
/// The seeded store holds every shape of graph the readers must rebuild, so that reading
/// every sample back covers them: inline rows, recursive subqueries, combined queries
/// (main or subquery), windows, the constant <c>All</c> and an optional <c>skip</c> left
/// out.
/// </summary>
[Collection(Neo4jServer.Name)]
public sealed class StoredShapesTests(Neo4jFixture fixture)
{
    [Theory]
    [InlineData("MATCH (n:List:Node) RETURN count(n) AS found")]
    [InlineData("MATCH (:SubqueryRows)-[:Rows]->(n:List) RETURN count(n) AS found")]
    [InlineData("MATCH (n:SubqueryRecursive) RETURN count(n) AS found")]
    [InlineData(
        "MATCH (n) WHERE n:MainCombinedQueryUnionAll OR n:CombinedQueryUnionAll "
            + "RETURN count(n) AS found"
    )]
    [InlineData(
        "MATCH (n) WHERE n:MainCombinedQueryUnion OR n:CombinedQueryUnion "
            + "RETURN count(n) AS found"
    )]
    [InlineData(
        "MATCH (n) WHERE n:MainCombinedQueryIntersect OR n:CombinedQueryIntersect "
            + "RETURN count(n) AS found"
    )]
    [InlineData(
        "MATCH (n) WHERE n:MainCombinedQueryExcept OR n:CombinedQueryExcept "
            + "RETURN count(n) AS found"
    )]
    [InlineData("MATCH ()-[r:Over]->() RETURN count(r) AS found")]
    [InlineData("MATCH (n:All) RETURN count(n) AS found")]
    [InlineData(
        "MATCH (n:Pagination) WHERE n.skip IS NULL AND NOT (n)-[:Skip]->() "
            + "RETURN count(n) AS found"
    )]
    public async Task HoldsTheShape(string cypher)
    {
        await using IAsyncSession session = fixture.Session();

        IResultCursor cursor = await session.RunAsync(cypher);
        IRecord record = await cursor.SingleAsync();

        Assert.True(record["found"].As<long>() > 0, cypher);
    }
}
