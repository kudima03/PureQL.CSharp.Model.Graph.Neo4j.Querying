using System.Runtime.CompilerServices;
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// Every stored query, streamed in ascending order of identifier. The identifiers are read
/// a page at a time with <see cref="QueryIdsPageStatement"/> and the queries of each page
/// with <see cref="QueriesByIds"/>, so memory holds one page of identifiers and one query
/// whatever the number of stored queries.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run on; an <see cref="IAsyncSession"/> runs each
/// statement as an auto-commit query.
/// </param>
public sealed record AllQueries(IAsyncQueryRunner Runner)
{
    /// <summary>The number of queries read with one statement.</summary>
    private const int PageSize = 50;

    /// <summary>Reads the queries.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>Every stored query.</returns>
    /// <exception cref="GraphMappingException">
    /// The stored nodes of a query do not form a query graph.
    /// </exception>
    public async IAsyncEnumerable<StoredQuery> Values(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Guid? after = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IResultCursor cursor = await Runner.RunAsync(
                new QueryIdsPageStatement(after, PageSize).Value
            );
            List<IRecord> page = await cursor.ToListAsync(cancellationToken);
            Guid[] ids = [.. page.Select(record => Guid.Parse((string)record["id"]))];
            await foreach (
                StoredQuery query in new QueriesByIds(Runner, ids).Values(
                    cancellationToken
                )
            )
            {
                yield return query;
            }

            if (ids.Length < PageSize)
            {
                yield break;
            }

            after = ids[^1];
        }
    }
}
