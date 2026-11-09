using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

[Collection(Neo4jServer.Name)]
public sealed class LookupTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task FindsTheQueriesReferencingEachField()
    {
        Dictionary<(string Source, string Field), HashSet<Guid>> expected = Fields();
        await using IAsyncSession session = fixture.Session();

        Assert.NotEmpty(expected);
        foreach (
            KeyValuePair<(string Source, string Field), HashSet<Guid>> pair in expected
        )
        {
            IReadOnlyList<Guid> ids = await new QueriesReferencingField(
                session,
                pair.Key.Source,
                pair.Key.Field
            ).ValueAsync();

            Assert.Equal(ids.Count, ids.Distinct().Count());
            Assert.Equal(pair.Value.Order(), ids.Order());
        }
    }

    [Fact]
    public async Task FindsTheQueriesDeclaringEachParameter()
    {
        Dictionary<string, HashSet<Guid>> expected = Parameters();
        await using IAsyncSession session = fixture.Session();

        Assert.NotEmpty(expected);
        foreach (KeyValuePair<string, HashSet<Guid>> param in expected)
        {
            IReadOnlyList<Guid> ids = await new QueriesDeclaringParameter(
                session,
                param.Key
            ).ValueAsync();

            Assert.Equal(ids.Count, ids.Distinct().Count());
            Assert.Equal(param.Value.Order(), ids.Order());
        }
    }

    [Fact]
    public async Task FindsNothingForAnUnknownField()
    {
        await using IAsyncSession session = fixture.Session();

        Assert.Empty(
            await new QueriesReferencingField(session, "nowhere", "nothing").ValueAsync()
        );
    }

    [Fact]
    public async Task FindsNothingForAnUnknownParameter()
    {
        await using IAsyncSession session = fixture.Session();

        Assert.Empty(
            await new QueriesDeclaringParameter(session, "nothing").ValueAsync()
        );
    }

    [Fact]
    public async Task FindsThroughTheStore()
    {
        Dictionary<(string Source, string Field), HashSet<Guid>> fields = Fields();
        Dictionary<string, HashSet<Guid>> parameters = Parameters();
        KeyValuePair<(string Source, string Field), HashSet<Guid>> field = fields.First();
        KeyValuePair<string, HashSet<Guid>> parameter = parameters.First();

        IReadOnlyList<Guid> byField = await new QueriesReferencingFieldOf(
            fixture.Store,
            field.Key.Source,
            field.Key.Field
        ).ValueAsync();
        IReadOnlyList<Guid> byParameter = await new QueriesDeclaringParameterOf(
            fixture.Store,
            parameter.Key
        ).ValueAsync();

        Assert.Equal(field.Value.Order(), byField.Order());
        Assert.Equal(parameter.Value.Order(), byParameter.Order());
    }

    [Fact]
    public async Task StopsWhenCancelled()
    {
        await using IAsyncSession session = fixture.Session();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new QueriesDeclaringParameter(session, "id").ValueAsync(
                new CancellationToken(true)
            )
        );
    }

    /// <summary>
    /// The identifiers of the stored queries by the source and field their nodes
    /// reference, computed from the graphs the fixture wrote.
    /// </summary>
    private Dictionary<(string Source, string Field), HashSet<Guid>> Fields()
    {
        Dictionary<(string Source, string Field), HashSet<Guid>> expected = [];
        foreach (KeyValuePair<Guid, QueryGraph> graph in fixture.Graphs)
        {
            foreach (WrittenNode node in graph.Value.Nodes)
            {
                if (
                    node.Properties.TryGetValue("source", out object? source)
                    && node.Properties.TryGetValue("field", out object? field)
                    && source is string sourceName
                    && field is string fieldName
                )
                {
                    Add(expected, (sourceName, fieldName), graph.Key);
                }
            }
        }

        return expected;
    }

    /// <summary>
    /// The identifiers of the stored queries by the parameters their nodes use, computed
    /// from the graphs the fixture wrote.
    /// </summary>
    private Dictionary<string, HashSet<Guid>> Parameters()
    {
        Dictionary<string, HashSet<Guid>> expected = [];
        foreach (KeyValuePair<Guid, QueryGraph> graph in fixture.Graphs)
        {
            foreach (WrittenNode node in graph.Value.Nodes)
            {
                if (
                    node.Properties.TryGetValue("param", out object? param)
                    && param is string name
                )
                {
                    Add(expected, name, graph.Key);
                }
            }
        }

        return expected;
    }

    private static void Add<TKey>(
        Dictionary<TKey, HashSet<Guid>> expected,
        TKey key,
        Guid id
    )
        where TKey : notnull
    {
        if (!expected.TryGetValue(key, out HashSet<Guid>? ids))
        {
            ids = [];
            expected[key] = ids;
        }

        _ = ids.Add(id);
    }
}
