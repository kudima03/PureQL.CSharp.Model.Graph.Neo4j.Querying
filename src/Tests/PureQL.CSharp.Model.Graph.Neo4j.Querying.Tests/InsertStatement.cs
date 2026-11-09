using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

/// <summary>
/// The statement that writes a query graph, duplicated from
/// <c>PureQL.CSharp.Model.Graph.Neo4j.Insertion</c> so that the tests seed the store
/// without depending on that package.
/// </summary>
internal sealed record InsertStatement(QueryGraph Graph)
{
    private const string Text = """
        UNWIND $nodes AS n
        CREATE (x:$(n.labels))
        SET x = n.props
        WITH collect(x) AS xs
        UNWIND $rels AS r
        WITH xs[r.f] AS a, xs[r.t] AS b, r
        CREATE (a)-[e:$(r.type)]->(b)
        SET e.position = r.position
        """;

    public DriverQuery Value =>
        new DriverQuery(
            Text,
            new Dictionary<string, object>
            {
                ["nodes"] = Graph
                    .Nodes.Select(node => new Dictionary<string, object>
                    {
                        ["labels"] = node.Labels.ToArray(),
                        ["props"] = new Dictionary<string, object>(node.Properties),
                    })
                    .ToList(),
                ["rels"] = Graph
                    .Relationships.Select(relationship => new Dictionary<string, object?>
                    {
                        ["f"] = (long)relationship.From,
                        ["t"] = (long)relationship.To,
                        ["type"] = relationship.Type,
                        ["position"] = relationship.Position,
                    })
                    .ToList(),
            }
        );
}
