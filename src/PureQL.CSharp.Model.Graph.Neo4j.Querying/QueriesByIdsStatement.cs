using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The statement reading every node of several stored queries with its outgoing
/// relationships, one row per node, ordered by query so that the rows of a query are
/// adjacent: <c>MATCH (n:Node) WHERE n.queryId IN $ids RETURN n.queryId AS queryId, n,
/// [(n)-[r]-&gt;(m) | [type(r), r.position, elementId(m)]] AS out ORDER BY queryId</c>.
/// </summary>
/// <param name="Ids">The identifiers of the queries.</param>
public sealed record QueriesByIdsStatement(IReadOnlyCollection<Guid> Ids)
{
    private const string Text = """
        MATCH (n:Node)
        WHERE n.queryId IN $ids
        RETURN n.queryId AS queryId, n,
               [(n)-[r]->(m) | [type(r), r.position, elementId(m)]] AS out
        ORDER BY queryId
        """;

    /// <summary>The Cypher text and its <c>ids</c> parameter.</summary>
    public DriverQuery Value =>
        new DriverQuery(
            Text,
            new Dictionary<string, object>
            {
                ["ids"] = Ids.Select(id => id.ToString("D")).ToArray(),
            }
        );
}
