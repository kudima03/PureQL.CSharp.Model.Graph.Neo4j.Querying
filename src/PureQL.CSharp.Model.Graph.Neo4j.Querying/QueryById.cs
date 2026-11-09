using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// One stored query read by its identifier with one <see cref="QueryByIdStatement"/>.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run on; an <see cref="IAsyncSession"/> runs the statement
/// as an auto-commit query.
/// </param>
/// <param name="Id">The identifier of the query.</param>
public sealed record QueryById(IAsyncQueryRunner Runner, Guid Id)
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
        cancellationToken.ThrowIfCancellationRequested();
        IResultCursor cursor = await Runner.RunAsync(new QueryByIdStatement(Id).Value);
        List<IRecord> records = await cursor.ToListAsync(cancellationToken);
        return records.Count == 0 ? null : new RecordsQuery(Id, records).Value;
    }
}
