using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The identifiers of the stored queries that use a parameter, read with one
/// <see cref="QueriesDeclaringParameterStatement"/>.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run on; an <see cref="IAsyncSession"/> runs the statement
/// as an auto-commit query.
/// </param>
/// <param name="Param">The name of the parameter.</param>
public sealed record QueriesDeclaringParameter(IAsyncQueryRunner Runner, string Param)
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
            new QueriesDeclaringParameterStatement(Param).Value
        ).ValueAsync(cancellationToken);
    }
}
