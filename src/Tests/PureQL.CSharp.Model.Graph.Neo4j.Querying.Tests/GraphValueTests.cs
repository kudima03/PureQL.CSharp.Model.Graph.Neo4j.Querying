namespace PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests;

public sealed record GraphValueTests
{
    [Fact]
    public void KeepsAnInteger()
    {
        Assert.Equal(42L, new GraphValue(42L).Value);
    }

    [Fact]
    public void KeepsAString()
    {
        Assert.Equal("1.5", new GraphValue("1.5").Value);
    }

    [Fact]
    public void KeepsABoolean()
    {
        Assert.Equal(true, new GraphValue(true).Value);
    }

    [Fact]
    public void KeepsNull()
    {
        Assert.Null(new GraphValue(null).Value);
    }

    [Fact]
    public void TurnsAListOfStringsIntoAStringArray()
    {
        object? value = new GraphValue(new List<object> { "a", "b" }).Value;

        Assert.Equal(["a", "b"], Assert.IsType<string[]>(value));
    }

    [Fact]
    public void TurnsAListOfIntegersIntoALongArray()
    {
        object? value = new GraphValue(new List<object> { 1L, -2L }).Value;

        Assert.Equal([1L, -2L], Assert.IsType<long[]>(value));
    }

    [Fact]
    public void TurnsAListOfBooleansIntoABoolArray()
    {
        object? value = new GraphValue(new List<object> { true, false }).Value;

        Assert.Equal([true, false], Assert.IsType<bool[]>(value));
    }

    [Fact]
    public void TurnsAnEmptyListIntoAnEmptyStringArray()
    {
        object? value = new GraphValue(new List<object>()).Value;

        Assert.Empty(Assert.IsType<string[]>(value));
    }

    [Fact]
    public void KeepsTheItemsOfAMixedList()
    {
        object? value = new GraphValue(new List<object> { 1L, "a" }).Value;

        Assert.Equal([1L, "a"], Assert.IsType<object[]>(value));
    }
}
