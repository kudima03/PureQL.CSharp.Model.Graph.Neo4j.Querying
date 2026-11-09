# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory. The SDK is pinned by `global.json` (.NET 11, for the model's C# 15 unions); where the system SDK is older, use `~/.dotnet/dotnet` or put it first on `PATH` with `DOTNET_ROOT=$HOME/.dotnet`.

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet format --verify-no-changes          # check code style (CI enforces this)
dotnet format                              # auto-fix code style
dotnet test --no-build --verbosity normal  # run tests (the integration tests need Docker)
dotnet csharpier check .                   # check formatting (CI enforces this)
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is the **Neo4j querying package** of the `PureQL.CSharp.Model.Graph` family: it reads stored query graphs back into `StoredQuery` values. It depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`, `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and `Neo4j.Driver` `6.5.1`, and on no other operation package. Namespace `PureQL.CSharp.Model.Graph.Neo4j.Querying`.

Public API, one type per file:

- **Readers** on an `IAsyncQueryRunner Runner` (an `IAsyncSession` runs auto-commit queries; an `IAsyncTransaction` composes with the sibling packages): `QueryById(Runner, Guid Id)` → `Task<StoredQuery?> ValueAsync(ct)` (`null` when no node has the id), `QueriesByIds(Runner, IReadOnlyCollection<Guid> Ids)` and `AllQueries(Runner)` → `IAsyncEnumerable<StoredQuery> Values(ct)`, `QueriesReferencingField(Runner, string Source, string Field)` and `QueriesDeclaringParameter(Runner, string Param)` → `Task<IReadOnlyList<Guid>> ValueAsync(ct)`. The names and signatures are the family contract: do not reshape them.
- **`...Of` twins** (`QueryByIdOf`, `QueriesByIdsOf`, `AllQueriesOf`, `QueriesReferencingFieldOf`, `QueriesDeclaringParameterOf`) take `INeo4jStoredQueries Store` instead of the runner, open a read session through the internal `ReadSession` (`WithDatabase(Store.Database)`, `AccessMode.Read`) and run the reader on it, so the statements run as auto-commit queries (`session.RunAsync`, not a managed transaction: about 1 ms cheaper per read). The enumerating twins keep the session open while the caller enumerates.
- **Statements**, each a record with `Neo4j.Driver.Query Value`: `QueryByIdStatement`, `QueriesByIdsStatement` (`ORDER BY queryId`, so a query's rows are adjacent and `QueriesByIds` yields a query as soon as the next one starts), `QueryIdsPageStatement` (keyset paging over `Query.id`: `WHERE q.id > $after ... LIMIT $size`, `""` for the first page), `QueriesReferencingFieldStatement`, `QueriesDeclaringParameterStatement`. The namespace sits inside `PureQL.CSharp.Model`, whose `Query` shadows the driver's, so files alias it (`using DriverQuery = Neo4j.Driver.Query;`).

Internal: `RecordsQuery(Guid Id, IReadOnlyList<IRecord> Records)` builds one `ReadNode` per row (`n`): the label that is neither `Node` nor `Query`, the properties through `NodeProperties`, and a children dictionary filled afterwards from `out` (`[type(r), r.position, elementId(m)]`), grouped by type and ordered by position; the node labelled `Query` is the root for `QueryGraphs.Read`. No root, two roots, a node without record label or a relationship to a node outside the rows throw `GraphMappingException`. `GraphValue` turns a driver value into the store-neutral representation (a list becomes a `string[]`, `long[]` or `bool[]` by its elements, an empty list a `string[]`), `NodeProperties` converts a node's properties and leaves `null` out, `QueryIds` reads the `queryId` column of a lookup. `AllQueries` reads pages of 50 ids (`PageSize`).

**Test project:** `PureQL.CSharp.Model.Graph.Neo4j.Querying.Tests` (xunit, coverlet) sees the internals through `InternalsVisibleTo`. Unit tests cover the conversions and the statements' parameters. The integration tests share `Neo4jFixture` (collection `Neo4jServer`): a `Testcontainers.Neo4j` container on `neo4j:5-community` (random host port), seeded with all 86 queries of `PureQL.CSharp.Model.Samples` through `InsertStatement`, a test-only copy of the Insertion package's UNWIND statement built from `QueryGraphs.Of` (no other operation package is referenced). The tests only read the seeded store and compare queries as JSON through `PureQL.CSharp.Model.Serialization` (`PureQLJson`); `MalformedGraphTests` creates broken graphs in a transaction it rolls back. The lookups' expected ids are computed from the samples' `QueryGraph` properties. The tests need Docker.

**Target framework:** net11.0 only, as the model. `IsAotCompatible` is `true`.

**Package validation:** `EnablePackageValidation` is `true` without a baseline until the first release; the `nuget-publish` skill adds `PackageValidationBaselineVersion` after each release, and intentional breaks go to `CompatibilitySuppressions.xml` next to the csproj.

**Mutation testing:** the Stryker step of CI is commented out.

**Publishing:** triggered by pushing a semver tag (pattern `*.*.*`). The tag becomes the `PackageVersion`. The workflow publishes to both GitHub Packages and NuGet.org.

## Code Style

Enforced via `.editorconfig` and `dotnet format --verify-no-changes` in CI:

- No `var` — always use explicit types
- No expression-bodied methods or constructors — use block bodies
- Expression-bodied properties and accessors are required
- File-scoped namespace declarations (`namespace Foo.Bar;`)
- Private fields prefixed with `_` in camelCase (e.g., `_options`)
- Braces always required, even for single-statement blocks
- Max line length: 90 characters
- Allman brace style — opening brace on its own line for all blocks
- `System.*` using directives sorted first, no blank line between using groups
- Parentheses required in arithmetic and relational binary operators for clarity

## Commit Messages

Do not mention Claude or AI assistance in commit messages.
