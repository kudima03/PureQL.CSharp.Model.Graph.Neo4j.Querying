namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// The properties of a node as the Neo4j driver returns them, in the store-neutral
/// representation <see cref="QueryGraphs.Read(IReadNode)"/> reads: each value converted by
/// <see cref="GraphValue"/>, and a <see langword="null"/> value left out, as the graph
/// counts it as absent.
/// </summary>
/// <param name="DriverProperties">The properties as the driver returns them.</param>
internal sealed record NodeProperties(
    IReadOnlyDictionary<string, object> DriverProperties
)
{
    /// <summary>The properties in the store-neutral representation.</summary>
    public IReadOnlyDictionary<string, object?> Value => Converted(DriverProperties);

    private static Dictionary<string, object?> Converted(
        IReadOnlyDictionary<string, object> properties
    )
    {
        Dictionary<string, object?> converted = new Dictionary<string, object?>(
            properties.Count,
            StringComparer.Ordinal
        );
        foreach (KeyValuePair<string, object> property in properties)
        {
            object? value = new GraphValue(property.Value).Value;
            if (value is not null)
            {
                converted[property.Key] = value;
            }
        }

        return converted;
    }
}
