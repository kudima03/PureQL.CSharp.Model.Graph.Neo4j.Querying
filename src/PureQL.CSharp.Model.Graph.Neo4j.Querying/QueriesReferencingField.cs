using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The identifiers of the stored queries that reference a field of a source, read with
/// one <see cref="QueriesReferencingFieldStatement"/>.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run on; an <see cref="IAsyncSession"/> runs the statement
/// as an auto-commit query.
/// </param>
/// <param name="Source">The source, such as an entity name or a <c>from</c> alias.</param>
/// <param name="Field">The field of the source.</param>
public sealed record QueriesReferencingField(
    IAsyncQueryRunner Runner,
    string Source,
    string Field
)
{
    /// <summary>Reads the identifiers.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The identifiers, each once, in no particular order.</returns>
    public Task<IReadOnlyList<Guid>> ValueAsync(
        CancellationToken cancellationToken = default
    )
    {
        return new QueryIds(
            Runner,
            new QueriesReferencingFieldStatement(Source, Field).Value
        ).ValueAsync(cancellationToken);
    }
}
