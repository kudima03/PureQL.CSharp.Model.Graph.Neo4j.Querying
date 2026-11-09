using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The statement reading the identifiers of the stored queries that use a parameter:
/// <c>MATCH (n:Node) WHERE n.param = $param RETURN DISTINCT n.queryId AS queryId</c>,
/// served by the <c>Node(param)</c> index when it exists.
/// </summary>
/// <param name="Param">The name of the parameter.</param>
public sealed record QueriesDeclaringParameterStatement(string Param)
{
    private const string Text = """
        MATCH (n:Node)
        WHERE n.param = $param
        RETURN DISTINCT n.queryId AS queryId
        """;

    /// <summary>The Cypher text and its <c>param</c> parameter.</summary>
    public DriverQuery Value =>
        new DriverQuery(Text, new Dictionary<string, object> { ["param"] = Param });
}
