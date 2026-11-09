using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The statement reading every node of one stored query with its outgoing
/// relationships: <c>MATCH (n:Node {queryId: $id}) RETURN n, [(n)-[r]-&gt;(m) |
/// [type(r), r.position, elementId(m)]] AS out</c>, one row per node.
/// </summary>
/// <param name="Id">The identifier of the query.</param>
public sealed record QueryByIdStatement(Guid Id)
{
    private const string Text = """
        MATCH (n:Node {queryId: $id})
        RETURN n, [(n)-[r]->(m) | [type(r), r.position, elementId(m)]] AS out
        """;

    /// <summary>The Cypher text and its <c>id</c> parameter.</summary>
    public DriverQuery Value =>
        new DriverQuery(
            Text,
            new Dictionary<string, object> { ["id"] = Id.ToString("D") }
        );
}
