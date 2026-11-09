using System.Runtime.CompilerServices;
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The stored queries of several identifiers read as <see cref="QueriesByIds"/> does, in a
/// session of its own, open while the queries are enumerated, that runs the statement as
/// an auto-commit query.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Ids">The identifiers of the queries.</param>
public sealed record QueriesByIdsOf(
    INeo4jStoredQueries Store,
    IReadOnlyCollection<Guid> Ids
)
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
        await using IAsyncSession session = new ReadSession(Store).Value;
        await foreach (
            StoredQuery query in new QueriesByIds(session, Ids).Values(cancellationToken)
        )
        {
            yield return query;
        }
    }
}
