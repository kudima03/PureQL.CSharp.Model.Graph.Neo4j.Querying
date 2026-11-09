# PureQL.CSharp.Model.Graph.Neo4j.Querying

Reads **PureQL** queries stored as property graphs in Neo4j back into `PureQLQuery` values.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Querying)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Querying)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Querying` is the read operation package of the `PureQL.CSharp.Model.Graph` family: it reads the query graphs that `PureQL.CSharp.Model.Graph.Neo4j.Insertion` writes back into `StoredQuery` values and answers structural lookups over them.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Querying`. Every reader runs on an `IAsyncQueryRunner` (an `IAsyncSession`, whose `RunAsync` runs an auto-commit query, or an `IAsyncTransaction`), so reads compose with the other operation packages inside one transaction. Each has an `...Of` twin over `INeo4jStoredQueries` that opens a read session on `Store.Driver` against `Store.Database` and runs auto-commit queries.

| Reader | Twin | Result |
|--------|------|--------|
| `QueryById(IAsyncQueryRunner Runner, Guid Id)` | `QueryByIdOf(INeo4jStoredQueries Store, Guid Id)` | `Task<StoredQuery?> ValueAsync(ct)`: the query, or `null` when no query has the id |
| `QueriesByIds(IAsyncQueryRunner Runner, IReadOnlyCollection<Guid> Ids)` | `QueriesByIdsOf(INeo4jStoredQueries Store, IReadOnlyCollection<Guid> Ids)` | `IAsyncEnumerable<StoredQuery> Values(ct)`: the queries that exist, each once, in ascending order of id; missing ids are skipped |
| `AllQueries(IAsyncQueryRunner Runner)` | `AllQueriesOf(INeo4jStoredQueries Store)` | `IAsyncEnumerable<StoredQuery> Values(ct)`: every stored query, streamed in ascending order of id |
| `QueriesReferencingField(IAsyncQueryRunner Runner, string Source, string Field)` | `QueriesReferencingFieldOf(INeo4jStoredQueries Store, string Source, string Field)` | `Task<IReadOnlyList<Guid>> ValueAsync(ct)`: the ids of the queries with a node referencing the field of the source |
| `QueriesDeclaringParameter(IAsyncQueryRunner Runner, string Param)` | `QueriesDeclaringParameterOf(INeo4jStoredQueries Store, string Param)` | `Task<IReadOnlyList<Guid>> ValueAsync(ct)`: the ids of the queries using the parameter |

A stored graph that does not map back to a query throws `GraphMappingException` from `PureQL.CSharp.Model.Graph`.

The statements are public too, each a record whose `Query Value` is a `Neo4j.Driver.Query` (Cypher text and parameters):

| Statement | Cypher |
|-----------|--------|
| `QueryByIdStatement(Guid Id)` | `MATCH (n:Node {queryId: $id}) RETURN n, [(n)-[r]->(m) \| [type(r), r.position, elementId(m)]] AS out` |
| `QueriesByIdsStatement(IReadOnlyCollection<Guid> Ids)` | `MATCH (n:Node) WHERE n.queryId IN $ids RETURN n.queryId AS queryId, n, [(n)-[r]->(m) \| [type(r), r.position, elementId(m)]] AS out ORDER BY queryId` |
| `QueryIdsPageStatement(Guid? After, int Size)` | `MATCH (q:Query) WHERE q.id > $after RETURN q.id AS id ORDER BY id LIMIT $size` |
| `QueriesReferencingFieldStatement(string Source, string Field)` | `MATCH (n:Node) WHERE n.field = $field AND n.source = $source RETURN DISTINCT n.queryId AS queryId` |
| `QueriesDeclaringParameterStatement(string Param)` | `MATCH (n:Node) WHERE n.param = $param RETURN DISTINCT n.queryId AS queryId` |

The lookups and the reads by id are served by the `Node(queryId)`, `Node(field, source)` and `Node(param)` indexes and the `Query.id` constraint that `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates; they also work without them, by scanning.

## How a query is read back

The rows of a query's nodes become `ReadNode`s of `PureQL.CSharp.Model.Graph`: the label is the node's record label (the one that is neither `Node` nor `Query`), the properties are converted to the graph's store-neutral representation (integers stay `long`; a list becomes a `string[]`, `long[]` or `bool[]` by its elements, an empty list a `string[]`; a `null` counts as absent), and the children are wired by element id, grouped by relationship type and ordered by `position`. `QueryGraphs.Read` then reads the node labelled `Query`. `QueriesByIds` reads all its queries with one statement and yields each query as soon as its rows are read; `AllQueries` pages through the ids of the `Query` nodes 50 at a time, so memory holds one page of ids and one query however many queries are stored.

## Usage

```csharp
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using PureQL.CSharp.Model.Graph.Neo4j.Querying;

await using IDriver driver = GraphDatabase.Driver(
    "neo4j://localhost:7687",
    AuthTokens.Basic("neo4j", "password")
);
INeo4jStoredQueries store = new Neo4jStoredQueries(driver);
Guid queryId = /* the id a visualization holds */;
IReadOnlyCollection<Guid> queryIds = /* the ids a dashboard's visualizations hold */;

// One query by the id a visualization holds:
StoredQuery? query = await new QueryByIdOf(store, queryId).ValueAsync();

// The queries of a dashboard, in one statement:
await foreach (StoredQuery stored in new QueriesByIdsOf(store, queryIds).Values())
{
    Console.WriteLine($"{stored.Id}: {stored.Query}");
}

// Which queries break if the orders.amount field is renamed?
IReadOnlyList<Guid> affected = await new QueriesReferencingFieldOf(
    store,
    "orders",
    "amount"
).ValueAsync();

// Or read inside a transaction of your own:
await using IAsyncSession session = driver.AsyncSession(
    builder => builder.WithDatabase(store.Database)
);
StoredQuery? same = await session.ExecuteReadAsync(
    transaction => new QueryById(transaction, queryId).ValueAsync()
);
```

## Design Principles

- **One statement per read** — a query, a set of queries and a lookup each take one round trip; `AllQueries` takes two per page
- **Composable** — readers run on any `IAsyncQueryRunner`, so they share a transaction with writes of the sibling packages
- **AOT-compatible** — `IsAotCompatible=true`, no reflection

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Querying
```

## Dependencies

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) `0.1.0-preview.1.0.0` — reads a query graph back into a `PureQLQuery`
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) `0.1.0-preview.1.0.0` — `INeo4jStoredQueries` and `StoredQuery`
- [`Neo4j.Driver`](https://www.nuget.org/packages/Neo4j.Driver) `6.5.1`

## Target Frameworks

- .NET 11

## Related packages

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) — maps a `PureQLQuery` to a property graph and back, without a driver
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) — the stored-queries contract
- [`PureQL.CSharp.Model.Graph.Neo4j.Creation`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation) — constraints and indexes
- [`PureQL.CSharp.Model.Graph.Neo4j.Insertion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Insertion) — writes a query
- [`PureQL.CSharp.Model.Graph.Neo4j.Deletion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion) — deletes queries
- [`PureQL.CSharp.Model.Graph.Neo4j.Updating`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Updating) — replaces a query in one transaction
- [`PureQL.CSharp.Model.Graph.Neo4j.Existence`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence) — checks the schema and queries exist
- [`PureQL.CSharp.Model.Graph.Neo4j.Destruction`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction) — drops the schema and every stored query
- [`PureQL.CSharp.Model.Graph.Neo4j.Migration`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Migration) — migrates stored graphs between model versions
- [`PureQL.CSharp.Model.Serialization`](https://github.com/kudima03/PureQL.CSharp.Model.Serialization) — JSON for the same model
