using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The identifiers of the stored queries that reference a field of a source, read as
/// <see cref="QueriesReferencingField"/> does, in a session of its own that runs the
/// statement as an auto-commit query.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Source">The source, such as an entity name or a <c>from</c> alias.</param>
/// <param name="Field">The field of the source.</param>
public sealed record QueriesReferencingFieldOf(
    INeo4jStoredQueries Store,
    string Source,
    string Field
)
{
    /// <summary>Reads the identifiers.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The identifiers, each once, in no particular order.</returns>
    public async Task<IReadOnlyList<Guid>> ValueAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using IAsyncSession session = new ReadSession(Store).Value;
        return await new QueriesReferencingField(session, Source, Field).ValueAsync(
            cancellationToken
        );
    }
}
