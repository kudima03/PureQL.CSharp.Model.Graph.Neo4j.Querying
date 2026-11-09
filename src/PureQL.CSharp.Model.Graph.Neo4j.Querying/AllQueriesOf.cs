using System.Runtime.CompilerServices;
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// Every stored query read as <see cref="AllQueries"/> does, in a session of its own, open
/// while the queries are enumerated, that runs each statement as an auto-commit query.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
public sealed record AllQueriesOf(INeo4jStoredQueries Store)
{
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
        await using IAsyncSession session = new ReadSession(Store).Value;
        await foreach (
            StoredQuery query in new AllQueries(session).Values(cancellationToken)
        )
        {
            yield return query;
        }
    }
}
