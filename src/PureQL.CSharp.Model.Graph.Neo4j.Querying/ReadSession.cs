using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// A new read session on <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/>, whose <c>RunAsync</c> runs auto-commit
/// queries. Whoever takes <see cref="Value"/> disposes it.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
internal sealed record ReadSession(INeo4jStoredQueries Store)
{
    /// <summary>A new session; each read opens another.</summary>
    public IAsyncSession Value =>
        Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database).WithDefaultAccessMode(AccessMode.Read)
        );
}
