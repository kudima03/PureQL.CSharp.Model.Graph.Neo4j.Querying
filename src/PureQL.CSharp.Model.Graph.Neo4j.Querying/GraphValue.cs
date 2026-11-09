using System.Collections;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// A property value as the Neo4j driver returns it, in the store-neutral representation
/// <see cref="QueryGraphs.Read(IReadNode)"/> reads: a scalar stays as it is (the driver
/// hands integers over as <see cref="long"/>), and a list, which the driver hands over as
/// a <see cref="List{T}"/> of <see cref="object"/>, becomes a <see cref="string"/>,
/// <see cref="long"/> or <see cref="bool"/> array by the type of its elements. An empty
/// list becomes an empty <see cref="string"/> array, as the graph writes an empty list.
/// </summary>
/// <param name="DriverValue">The value as the driver returns it.</param>
internal sealed record GraphValue(object? DriverValue)
{
    /// <summary>The value in the store-neutral representation.</summary>
    public object? Value => Converted(DriverValue);

    private static object? Converted(object? value)
    {
        return value is IEnumerable items and not string ? Array(items) : value;
    }

    private static object Array(IEnumerable items)
    {
        object[] values = [.. items.Cast<object>()];
        return values.All(item => item is string) ? values.Cast<string>().ToArray()
            : values.All(item => item is long) ? values.Cast<long>().ToArray()
            : values.All(item => item is bool) ? values.Cast<bool>().ToArray()
            : values;
    }
}
