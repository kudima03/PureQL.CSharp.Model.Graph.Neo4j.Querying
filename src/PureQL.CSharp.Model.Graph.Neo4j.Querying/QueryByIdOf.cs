using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// One stored query read by its identifier as <see cref="QueryById"/> does, in a session
/// of its own that runs the statement as an auto-commit query.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Id">The identifier of the query.</param>
public sealed record QueryByIdOf(INeo4jStoredQueries Store, Guid Id)
{
    /// <summary>Reads the query.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The query, or <see langword="null"/> when no query has the identifier.</returns>
    /// <exception cref="GraphMappingException">
    /// The stored nodes do not form a query graph.
    /// </exception>
    public async Task<StoredQuery?> ValueAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using IAsyncSession session = new ReadSession(Store).Value;
        return await new QueryById(session, Id).ValueAsync(cancellationToken);
    }
}
