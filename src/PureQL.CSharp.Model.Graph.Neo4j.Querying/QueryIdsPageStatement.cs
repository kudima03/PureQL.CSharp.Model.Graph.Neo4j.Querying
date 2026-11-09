using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The statement reading one page of the identifiers of the stored queries, in
/// ascending order, after a given one: <c>MATCH (q:Query) WHERE q.id &gt; $after RETURN
/// q.id AS id ORDER BY id LIMIT $size</c>.
/// </summary>
/// <param name="After">
/// The last identifier of the previous page, or <see langword="null"/> for the first page.
/// </param>
/// <param name="Size">The number of identifiers in a page.</param>
public sealed record QueryIdsPageStatement(Guid? After, int Size)
{
    private const string Text = """
        MATCH (q:Query)
        WHERE q.id > $after
        RETURN q.id AS id
        ORDER BY id
        LIMIT $size
        """;

    /// <summary>The Cypher text and its <c>after</c> and <c>size</c> parameters.</summary>
    public DriverQuery Value =>
        new DriverQuery(
            Text,
            new Dictionary<string, object>
            {
                ["after"] = After?.ToString("D") ?? "",
                ["size"] = (long)Size,
            }
        );
}
