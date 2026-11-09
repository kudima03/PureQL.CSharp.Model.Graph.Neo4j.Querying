using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using Testcontainers.Neo4j;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

/// <summary>
/// A Neo4j server holding every sample query, each under an identifier of its own. The
/// tests only read it, so that every reader sees the same stored queries.
/// </summary>
public sealed class Neo4jFixture : IAsyncLifetime
{
    private readonly Neo4jContainer _container = new Neo4jBuilder(
        "neo4j:5-community"
    ).Build();

    private IDriver? _driver;

    public IDriver Driver =>
        _driver ?? throw new InvalidOperationException("Not started.");

    public INeo4jStoredQueries Store => new Neo4jStoredQueries(Driver);

    /// <summary>The identifier of each sample, by the sample's name.</summary>
    public IReadOnlyDictionary<string, Guid> Ids { get; } =
        QuerySamples.All.Keys.ToDictionary(name => name, _ => Guid.NewGuid());

    /// <summary>The graph of each stored query, by its identifier.</summary>
    public IReadOnlyDictionary<Guid, QueryGraph> Graphs =>
        Ids.ToDictionary(
            sample => sample.Value,
            sample => QueryGraphs.Of(QuerySamples.All[sample.Key], sample.Value)
        );

    public IAsyncSession Session()
    {
        return Driver.AsyncSession(builder => builder.WithDatabase(Store.Database));
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _driver = GraphDatabase.Driver(_container.GetConnectionString(), AuthTokens.None);
        await using IAsyncSession session = Session();
        foreach (QueryGraph graph in Graphs.Values)
        {
            _ = await session.ExecuteWriteAsync(async transaction =>
            {
                IResultCursor cursor = await transaction.RunAsync(
                    new InsertStatement(graph).Value
                );
                return await cursor.ConsumeAsync();
            });
        }
    }

    public async Task DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class Neo4jServer : ICollectionFixture<Neo4jFixture>
{
    public const string Name = "Neo4j";
}
