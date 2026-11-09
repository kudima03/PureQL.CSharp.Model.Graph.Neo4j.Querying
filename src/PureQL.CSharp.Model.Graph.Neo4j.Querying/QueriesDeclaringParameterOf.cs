using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The identifiers of the stored queries that use a parameter, read as
/// <see cref="QueriesDeclaringParameter"/> does, in a session of its own that runs the
/// statement as an auto-commit query.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Param">The name of the parameter.</param>
public sealed record QueriesDeclaringParameterOf(INeo4jStoredQueries Store, string Param)
{
    /// <summary>Reads the identifiers.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The identifiers, each once, in no particular order.</returns>
    public async Task<IReadOnlyList<Guid>> ValueAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using IAsyncSession session = new ReadSession(Store).Value;
        return await new QueriesDeclaringParameter(session, Param).ValueAsync(
            cancellationToken
        );
    }
}
