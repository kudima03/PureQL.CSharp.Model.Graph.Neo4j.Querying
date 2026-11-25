# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Querying` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Follows `PureQL.CSharp.Model` `0.1.0-preview.15.0.0` (PureQL specification
`0.1.0-preview.3.0.0`). The public API is unchanged; the queries read back are those of the
new model, so combined, window, inline-rows and recursive queries are read too.

### Changed

- **Breaking:** depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.2.0.0` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.2.0.0`, on
  `PureQL.CSharp.Model` `0.1.0-preview.15.0.0`.
- A node labelled `List` and `Node` (one row of the inline rows of a `SubqueryRows`) is
  read with `List` as its label, as `PureQL.CSharp.Model.Graph` expects.
- Graphs written with `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.x` are not read back
  (`GraphMappingException`): drop them and write the queries again. There is no
  migration.
- The integration tests store and read back all 151 queries of
  `PureQL.CSharp.Model.Samples` `0.1.0-preview.4.0.0`, compared as JSON through
  `PureQL.CSharp.Model.Serialization` `0.1.0-preview.7.0.0`.

## [0.1.0-preview.0.2.0] - 2026-11-11

### Changed

- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.1.0` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.1.0`, the
  .NET 11 releases.
- Builds on the .NET 11 GA SDK, installed by version instead of `global.json`.

## [0.1.0-preview.0.1.0] - 2026-11-09

First release: reads PureQL queries stored as graphs in Neo4j back into
`StoredQuery` values, for the `PureQL.CSharp.Model.Graph` family of Neo4j
operation packages.

### Added

- **Readers** on an `IAsyncQueryRunner` (an `IAsyncSession` or an
  `IAsyncTransaction`):
  - `QueryById(Runner, Guid Id)` → `Task<StoredQuery?> ValueAsync(ct)`,
    `null` when no query has the id.
  - `QueriesByIds(Runner, IReadOnlyCollection<Guid> Ids)` →
    `IAsyncEnumerable<StoredQuery> Values(ct)`, yielding each query as soon
    as its rows are read.
  - `AllQueries(Runner)` → `IAsyncEnumerable<StoredQuery> Values(ct)`,
    paging through the query ids so memory stays bounded.
  - `QueriesReferencingField(Runner, string Source, string Field)` and
    `QueriesDeclaringParameter(Runner, string Param)` →
    `Task<IReadOnlyList<Guid>> ValueAsync(ct)`: the ids of the queries
    that use a field or declare a parameter.
- **`...Of` twins** — `QueryByIdOf`, `QueriesByIdsOf`, `AllQueriesOf`,
  `QueriesReferencingFieldOf` and `QueriesDeclaringParameterOf` take an
  `INeo4jStoredQueries` instead of a runner, open a read session on its
  database and run the reader as auto-commit queries.
- **Statements** — `QueryByIdStatement`, `QueriesByIdsStatement`,
  `QueryIdsPageStatement`, `QueriesReferencingFieldStatement` and
  `QueriesDeclaringParameterStatement`, each exposing the Cypher
  `Neo4j.Driver.Query` it runs.
- A stored graph is rebuilt into a `StoredQuery` through
  `PureQL.CSharp.Model.Graph`; a malformed graph throws
  `GraphMappingException`.
- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`,
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and
  `Neo4j.Driver` `6.5.1`; targets .NET 11 and is AOT-compatible.
