using System.Runtime.CompilerServices;
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The stored queries of several identifiers, such as those visualizations hold, read with
/// one <see cref="QueriesByIdsStatement"/> and streamed one query at a time in ascending
/// order of identifier. Identifiers no query has are skipped, and each query is read once
/// however often its identifier is given.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run on; an <see cref="IAsyncSession"/> runs the statement
/// as an auto-commit query.
/// </param>
/// <param name="Ids">The identifiers of the queries.</param>
public sealed record QueriesByIds(IAsyncQueryRunner Runner, IReadOnlyCollection<Guid> Ids)
{
    /// <summary>Reads the queries.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The queries that exist, each once.</returns>
    /// <exception cref="GraphMappingException">
    /// The stored nodes of a query do not form a query graph.
    /// </exception>
    public async IAsyncEnumerable<StoredQuery> Values(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Ids.Count == 0)
        {
            yield break;
        }

        IResultCursor cursor = await Runner.RunAsync(
            new QueriesByIdsStatement(Ids).Value
        );
        List<IRecord> records = [];
        string? current = null;
        while (await cursor.FetchAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            string queryId = (string)cursor.Current["queryId"];
            if ((current is not null) && (queryId != current))
            {
                yield return new RecordsQuery(Guid.Parse(current), records).Value;
                records = [];
            }

            current = queryId;
            records.Add(cursor.Current);
        }

        if (current is not null)
        {
            yield return new RecordsQuery(Guid.Parse(current), records).Value;
        }
    }
}
