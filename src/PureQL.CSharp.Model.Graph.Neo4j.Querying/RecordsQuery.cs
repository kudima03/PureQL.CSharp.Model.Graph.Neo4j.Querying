using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Querying;

/// <summary>
/// A stored query read back from the rows of <see cref="QueryByIdStatement"/> or
/// <see cref="QueriesByIdsStatement"/> for that query: one <see cref="ReadNode"/> per
/// node, labelled with its record label and holding its converted properties, its
/// children wired by element id, grouped by relationship type and ordered by position,
/// and the node labelled <see cref="GraphLabels.Query"/> read as the root.
/// </summary>
/// <param name="Id">The identifier of the query.</param>
/// <param name="Records">The rows of the query's nodes, each with <c>n</c> and <c>out</c>.</param>
internal sealed record RecordsQuery(Guid Id, IReadOnlyList<IRecord> Records)
{
    /// <summary>The query read back.</summary>
    /// <exception cref="GraphMappingException">The rows do not form a query graph.</exception>
    public StoredQuery Value => new StoredQuery(Id, QueryGraphs.Read(Root(Records)));

    private static ReadNode Root(IReadOnlyList<IRecord> records)
    {
        Dictionary<string, ReadNode> nodes = new Dictionary<string, ReadNode>(
            records.Count,
            StringComparer.Ordinal
        );
        Dictionary<string, Dictionary<string, IReadOnlyList<IReadNode>>> children =
            new Dictionary<string, Dictionary<string, IReadOnlyList<IReadNode>>>(
                records.Count,
                StringComparer.Ordinal
            );
        ReadNode? root = null;
        foreach (IRecord record in records)
        {
            INode node = (INode)record["n"];
            Dictionary<string, IReadOnlyList<IReadNode>> byType =
            [
                with(StringComparer.Ordinal),
            ];
            ReadNode read = new ReadNode(
                Label(node),
                new NodeProperties(node.Properties).Value,
                byType
            );
            nodes[node.ElementId] = read;
            children[node.ElementId] = byType;
            if (node.Labels.Contains(GraphLabels.Query))
            {
                if (root is not null)
                {
                    throw new GraphMappingException(
                        "The query has more than one node labelled Query."
                    );
                }

                root = read;
            }
        }

        foreach (IRecord record in records)
        {
            Dictionary<string, IReadOnlyList<IReadNode>> byType = children[
                ((INode)record["n"]).ElementId
            ];
            foreach (
                IGrouping<string, IList<object>> type in Relationships(record)
                    .GroupBy(
                        relationship => (string)relationship[0],
                        StringComparer.Ordinal
                    )
            )
            {
                byType[type.Key] =
                [
                    .. type.OrderBy(relationship => relationship[1] as long? ?? 0L)
                        .Select(relationship => Target(nodes, (string)relationship[2])),
                ];
            }
        }

        return root
            ?? throw new GraphMappingException("The query has no node labelled Query.");
    }

    private static string Label(INode node)
    {
        foreach (string label in node.Labels)
        {
            if (label is not GraphLabels.Node and not GraphLabels.Query)
            {
                return label;
            }
        }

        throw new GraphMappingException(
            $"The node {node.ElementId} has no record label."
        );
    }

    private static IEnumerable<IList<object>> Relationships(IRecord record)
    {
        return ((IEnumerable<object>)record["out"]).Cast<IList<object>>();
    }

    private static ReadNode Target(Dictionary<string, ReadNode> nodes, string elementId)
    {
        return nodes.TryGetValue(elementId, out ReadNode? target)
            ? target
            : throw new GraphMappingException(
                $"A relationship leads to the node {elementId} outside the query."
            );
    }
}
