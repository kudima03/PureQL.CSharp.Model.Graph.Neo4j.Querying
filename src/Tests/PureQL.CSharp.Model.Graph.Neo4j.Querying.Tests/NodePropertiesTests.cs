namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

public sealed record NodePropertiesTests
{
    [Fact]
    public void ConvertsEveryValue()
    {
        IReadOnlyDictionary<string, object?> properties = new NodeProperties(
            new Dictionary<string, object>
            {
                ["queryId"] = "8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37",
                ["value"] = 7L,
                ["distinctBy"] = new List<object> { "id" },
            }
        ).Value;

        Assert.Equal(3, properties.Count);
        Assert.Equal("8f2a5c1e-3b7d-4e9a-a1c2-6d4b8e0f2a37", properties["queryId"]);
        Assert.Equal(7L, properties["value"]);
        Assert.Equal(["id"], Assert.IsType<string[]>(properties["distinctBy"]));
    }

    [Fact]
    public void LeavesNullValuesOut()
    {
        IReadOnlyDictionary<string, object?> properties = new NodeProperties(
            new Dictionary<string, object> { ["alias"] = null!, ["field"] = "id" }
        ).Value;

        Assert.Equal(["field"], properties.Keys);
    }

    [Fact]
    public void KeepsAnEmptyListAsAnEmptyStringArray()
    {
        IReadOnlyDictionary<string, object?> properties = new NodeProperties(
            new Dictionary<string, object> { ["joins"] = new List<object>() }
        ).Value;

        Assert.Empty(Assert.IsType<string[]>(properties["joins"]));
    }
}
