# RsBase

<One sentence: what this is and who uses it.>

## Layout

- `RsBase.slnx` at the root. Production code in `src/`, test projects in `tests/`.
- Build settings shared by all projects: `Directory.Build.props`. Never repeat them in a project file.
- Package versions: `Directory.Packages.props`. A project file names a package without a version.
- Tools the project needs: `.config/dotnet-tools.json`, installed with `dotnet tool restore`.

## Commands

- Build: `dotnet build`
- Test: `dotnet test`
- Run: <command>
