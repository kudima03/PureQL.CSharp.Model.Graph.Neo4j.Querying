# PureQL.CSharp.Model.Graph.Neo4j.Querying

Reads **PureQL** queries stored as property graphs in Neo4j back into `PureQLQuery` values.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Querying)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Querying)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Querying` is the read operation package of the `PureQL.CSharp.Model.Graph` family: it reads the query graphs that `PureQL.CSharp.Model.Graph.Neo4j.Insertion` writes back into `StoredQuery` values and answers structural lookups over them.

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
