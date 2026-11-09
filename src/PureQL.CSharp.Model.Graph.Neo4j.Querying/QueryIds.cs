using Neo4j.Driver;
using DriverQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The query identifiers a statement returns in its <c>queryId</c> column.
/// </summary>
/// <param name="Runner">The session or transaction to run on.</param>
/// <param name="Statement">The statement.</param>
internal sealed record QueryIds(IAsyncQueryRunner Runner, DriverQuery Statement)
{
    /// <summary>Runs the statement and reads the identifiers.</summary>
    /// <param name="cancellationToken">Cancels reading the rows.</param>
    /// <returns>The identifiers, in the order the statement returns them.</returns>
    public async Task<IReadOnlyList<Guid>> ValueAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IResultCursor cursor = await Runner.RunAsync(Statement);
        List<IRecord> records = await cursor.ToListAsync(cancellationToken);
        return [.. records.Select(record => Guid.Parse((string)record["queryId"]))];
    }
}
