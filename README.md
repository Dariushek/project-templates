# Project templates

Install once, and again after a change:

```
dotnet new install dotnet/base --force
dotnet new install dotnet/webapi --force
dotnet new install dotnet/tests --force
```

## rs-base

A new solution. Run in an empty folder; the folder name becomes the solution name.

```
dotnet new rs-base
```

- `<Name>.slnx`, `src/`, `tests/`.
- `Directory.Build.props`: newest C#, nullable, all analyzers, warnings as errors, lock files, NuGet audit. High and critical vulnerabilities fail the build.
- `Directory.Packages.props`: central package versions, the threading analyzers and the versions the other templates need.
- `tests/Directory.Build.props`: settings for test projects only.
- `nuget.config`: nuget.org only, with source mapping.
- `.editorconfig`, `.gitattributes`, `.gitignore`, `global.json`.
- `.agents/validation.json`: check whitespace and code style of the changed files, build, test.
- `.agents/after-edit.json`: whitespace format of every C# file an agent edits. When it finishes, `dotnet format style`, then ReSharper Full Cleanup on the changed files.
- `.editorconfig`: ReSharper braces settings match Roslyn's `csharp_prefer_braces`, so the two formatters agree.
- `AGENTS.md`: skeleton for project facts.
- `CLAUDE.md`: imports `AGENTS.md`, so Claude Code reads it too.
- `.config/dotnet-tools.json`: local tools, ReSharper command line tools (`jb`) for the cleanup. Run `dotnet tool restore` after cloning.
- `.github/`: Dependabot and a workflow that runs the validation gate.

The git hooks, the gate and the after-edit hook come from the harness: `bin/harness sync <project>` with at least the `validation` and `after-edit` bundles.

Lock files are committed. After changing a package run `dotnet restore`; CI restores in locked mode and fails when they are stale.

## rs-webapi

An empty web API. Run in `src/`.

```
dotnet new rs-webapi -n Shop.Api
dotnet sln add src/Shop.Api
```

- Errors as problem details, `/health`, OpenAPI at `/openapi/v1.json` in development.
- No endpoints, no database, no auth, no folders.

Container image, no Dockerfile needed:

```
dotnet publish src/Shop.Api -t:PublishContainer
```

## rs-tests

A test project for a web API: TUnit, the app in-process, the database in a container, one database per test. Run in `tests/`.

```
dotnet new rs-tests -n Shop.Api.Tests --api Shop.Api
dotnet sln add tests/Shop.Api.Tests --include-references false
```

| Option | Default | Meaning |
|--------|---------|---------|
| `--api` | required | Web API project in `src/` |
| `--database` | `postgres` | `postgres` or `mssql` |
| `--connectionString` | `Default` | Name of the connection string the app reads |

It ships no test. Derive test classes from `ApiFixture`, override `PrepareDatabaseAsync` to create the schema and `ReplaceServices` to register fakes. Needs a running container engine.

`--include-references false` matters: without it `dotnet sln add` fails when the API project is already in the solution.
