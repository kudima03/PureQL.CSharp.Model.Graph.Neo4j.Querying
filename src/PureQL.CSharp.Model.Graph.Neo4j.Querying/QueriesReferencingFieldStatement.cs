using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The statement reading the identifiers of the stored queries that reference a field
/// of a source: <c>MATCH (n:Node) WHERE n.field = $field AND n.source = $source RETURN
/// DISTINCT n.queryId AS queryId</c>, served by the <c>Node(field, source)</c> index when
/// it exists.
/// </summary>
/// <param name="Source">The source, such as an entity name or a <c>from</c> alias.</param>
/// <param name="Field">The field of the source.</param>
public sealed record QueriesReferencingFieldStatement(string Source, string Field)
{
    private const string Text = """
        MATCH (n:Node)
        WHERE n.field = $field AND n.source = $source
        RETURN DISTINCT n.queryId AS queryId
        """;

    /// <summary>The Cypher text and its <c>source</c> and <c>field</c> parameters.</summary>
    public DriverQuery Value =>
        new DriverQuery(
            Text,
            new Dictionary<string, object> { ["source"] = Source, ["field"] = Field }
        );
}
