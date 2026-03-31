# AGENTS.md — Alma.ApplicationStatus

## Project Purpose

`Alma.ApplicationStatus` is an F# NuGet library that provides types and interfaces for a common application status representation. It allows services to expose structured status information (instance name, environment, git metadata, Docker image version, hostname) via a standardized `ApplicationStatus` record with XML serialization support.

## Tech Stack

- **Language:** F# (.NET 10)
- **Package manager:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **Test framework:** Expecto
- **NuGet package:** `Alma.ApplicationStatus`
- **Repository:** <https://github.com/alma-oss/fapplicationStatus>

## Key Dependencies

- `FSharp.Core ~> 10.0`
- `FSharp.Data ~> 6.0`
- `Alma.EnvironmentModel ~> 10.0` — environment/tier model types
- `Alma.ServiceIdentification ~> 11.0` — `Instance`, `Service`, `Environment` types

## Commands

```bash
# Install dependencies
dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests

# Lint (fsharplint config in fsharplint.json)
# Linting is integrated into the build pipeline
```

## Project Structure

```
├── ApplicationStatus.fsproj      # Project file (version, package metadata)
├── AssemblyInfo.fs               # Auto-generated assembly info
├── src/
│   └── ApplicationStatus.fs      # All types and logic (single-file library)
├── tests/
│   ├── tests.fsproj              # Test project
│   ├── Tests.fs                  # Test runner entry point
│   ├── CreationTest.fs           # Tests for ApplicationStatus.create
│   └── SerializationTest.fs      # Tests for XML serialization
├── build/
│   ├── Build.fs                  # FAKE build entry point
│   ├── Targets.fs                # Build targets (build, tests, publish)
│   └── Utils.fs                  # Build utilities
├── paket.dependencies            # Dependency definitions
├── paket.references              # References for main project
└── fsharplint.json               # Lint config (disables genericTypesNames)
```

## Architecture

The library defines:

1. **Single-case union types** — `GitBranch`, `GitCommit`, `GitRepository`, `DockerImageVersion` with companion modules for `value`/`empty`
2. **Feature interfaces** in `ApplicationStatusFeature` module:
   - `ICurrentApplication` — `Instance` + `Environment`
   - `IAssemblyInformation` — git branch/commit/repository
   - `IDockerApplication` — Docker image version
3. **`ApplicationStatus` record** — XML-serializable (`[<CLIMutable>]`, `[<XmlRoot>]`) flat representation
4. **`ApplicationStatus.create`** — factory function using active patterns to match implemented interfaces

### Pattern: Interface-based feature composition

Consumers implement a subset of `ApplicationStatusFeature` interfaces on their application type. `ApplicationStatus.create` uses active pattern matching (`IsCurrentApplication`, `IsAssemblyInfo`, `IsDockerApplication`) to extract data from whichever interfaces are implemented, defaulting to empty strings for missing features.

## Conventions

- **Single-case discriminated unions** for type safety (e.g., `GitBranch of string`)
- **`[<RequireQualifiedAccess>]`** on all modules — always use qualified names
- **Module companion pattern** — each type `Foo` has a `Foo` module with `value`/`empty` functions
- Lint config disables `genericTypesNames` rule

## CI/CD

| Workflow | Trigger | What it does |
|---|---|---|
| `tests.yaml` | PR, daily at 03:00 UTC | `./build.sh -t tests` on ubuntu-latest with .NET 10 |
| `publish.yaml` | Tag push (`X.Y.Z`) | `./build.sh -t publish` → NuGet.org |
| `pr-check.yaml` | PR | Blocks fixup commits, runs ShellCheck on scripts |

## Release Process

1. Increment `<Version>` in `ApplicationStatus.fsproj`
2. Update `CHANGELOG.md`
3. Commit and push a git tag matching the version (e.g., `8.0.0`)

## Pitfalls

- **No docker-compose / no local environment** — this is a pure library, no runtime services
- **XML serialization** — the `ApplicationStatus` record uses `[<CLIMutable>]` and XML attributes; do not convert to an immutable record without preserving XML serialization
- **Interface matching** — `ApplicationStatus.create` uses `obj` boxing for pattern matching; changing interface hierarchy may break detection
- **fsharplint.json** — minimal config; only disables `genericTypesNames`
