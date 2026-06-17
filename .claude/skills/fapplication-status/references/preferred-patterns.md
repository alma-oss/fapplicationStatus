# Preferred Patterns

## Core Principles

- Implement only the feature interfaces you can actually supply. `ApplicationStatus.create` detects implemented interfaces and leaves the rest as empty strings — partial implementation is the intended design, not an error.
- Keep the `ApplicationStatus` record as the boundary type for serialization. Build it through `create`; do not assemble it by hand unless you are writing a test fixture.
- Treat the single-case unions (`GitBranch`, `GitCommit`, `GitRepository`, `DockerImageVersion`) as the typed carriers for raw strings; wrap at the edge, unwrap with the module `value` function only when you need the underlying string.

## Recommended API Usage

- Wrapping and unwrapping metadata strings: see `examples.md` → Basic.
- Producing an `ApplicationStatus` from an application that implements several feature interfaces: see `examples.md` → Realistic.
- Sourcing git metadata from generated assembly attributes: see `examples.md` → Integration.
- Serializing the result to XML: see `examples.md` → XML serialization.

## Error Handling

- `ApplicationStatus.create` never throws for missing optional data: any feature interface that is not implemented yields an empty string in the corresponding field.
- `HostName` is resolved internally and its lookup failure is swallowed, falling back to an empty string — callers do not handle host resolution themselves.

## Composition

- An application type can implement the feature interfaces directly, or you can supply them inline through an object expression at the point you call `create`. Prefer the object expression when the interface values are derived (e.g. from assembly attributes) rather than stored on the type.
- `ICurrentApplication` is the only required interface (it is the generic constraint on `create`); `IAssemblyInformation` and `IDockerApplication` are independently optional and can be mixed in any combination.

## Integration with Other Libraries

- `ICurrentApplication.Instance` and `ICurrentApplication.Environment` come from `Alma.ServiceIdentification`; construct them with that library's parsers rather than ad-hoc strings.
- `create` derives `Tier` from the `Environment` via `Alma.ServiceIdentification` / `Alma.EnvironmentModel`; you do not set `Tier` yourself.

## Naming Conventions

- All companion modules are `[<RequireQualifiedAccess>]`; always call them qualified (`GitBranch.value`, `DockerImageVersion.empty`).
- Each single-case union `Foo` has a matching `Foo` module exposing `value` and `empty`.

## Testing Recommendations

- Test `create` by implementing the feature interfaces through an object expression and asserting on the resulting `ApplicationStatus` record: see `examples.md` → Test.
- When asserting `HostName`, resolve the expected host name the same way the library does rather than hard-coding it, since it is environment-dependent.
- Cover the partial-implementation cases (only `ICurrentApplication`, or some interfaces present) to confirm missing fields default to empty strings.
