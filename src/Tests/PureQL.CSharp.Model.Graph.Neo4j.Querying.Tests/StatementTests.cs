namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

public sealed record StatementTests
{
    private static readonly Guid Id = Guid.Parse("8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37");

    [Fact]
    public void QueryByIdPassesTheIdAsText()
    {
        Assert.Equal(
            "8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37",
            new QueryByIdStatement(Id).Value.Parameters["id"]
        );
    }

    [Fact]
    public void QueriesByIdsPassesTheIdsAsText()
    {
        Assert.Equal(
            ["8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37"],
            Assert.IsType<string[]>(
                new QueriesByIdsStatement([Id]).Value.Parameters["ids"]
            )
        );
    }

    [Fact]
    public void TheFirstPageOfIdsStartsAfterTheEmptyText()
    {
        IDictionary<string, object> parameters = new QueryIdsPageStatement(null, 10)
            .Value
            .Parameters;

        Assert.Equal("", parameters["after"]);
        Assert.Equal(10L, parameters["size"]);
    }

    [Fact]
    public void ANextPageOfIdsStartsAfterTheLastId()
    {
        Assert.Equal(
            "8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37",
            new QueryIdsPageStatement(Id, 10).Value.Parameters["after"]
        );
    }

    [Fact]
    public void QueriesReferencingFieldPassesTheSourceAndTheField()
    {
        IDictionary<string, object> parameters = new QueriesReferencingFieldStatement(
            "orders",
            "id"
        )
            .Value
            .Parameters;

        Assert.Equal("orders", parameters["source"]);
        Assert.Equal("id", parameters["field"]);
    }

    [Fact]
    public void QueriesDeclaringParameterPassesTheParameter()
    {
        Assert.Equal(
            "from",
            new QueriesDeclaringParameterStatement("from").Value.Parameters["param"]
        );
    }
}
