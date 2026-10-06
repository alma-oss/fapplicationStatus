# Anti-Patterns

Format: **mistake → why it is wrong → fix**.

## Common Mistakes

- **Calling `ApplicationStatus.create` on a type that does not implement `ICurrentApplication`** → `ICurrentApplication` is the generic constraint on `create`, so the code will not compile → make the application type (or inline object expression) implement `ICurrentApplication` first; the other feature interfaces remain optional.

- **Expecting `create` to fail or return an error when an optional interface is missing** → it is designed to degrade silently, writing an empty string for any unimplemented feature → if a field must be present, ensure the corresponding interface (`IAssemblyInformation` or `IDockerApplication`) is actually implemented; do not add error handling around `create`.

- **Implementing unrelated interfaces and expecting them to influence the output** → `create` only inspects `ICurrentApplication`, `IAssemblyInformation`, and `IDockerApplication`; any other interface (e.g. `IDisposable`) is ignored and its members are never invoked → only implement the documented feature interfaces to affect the status.

- **Setting the `Tier` field manually** → `create` derives `Tier` from the `Environment` through `Alma.ServiceIdentification` / `Alma.EnvironmentModel`; a hand-set tier is overwritten or inconsistent → supply a correct `Environment` and let `create` compute the tier.

- **Resolving `HostName` yourself before calling `create`** → `create` already resolves the host name internally and swallows lookup failures → leave `HostName` to the library and do not pass it in.

## Do Not Use / Avoid

- **Pattern matching on the single-case unions to extract the string** (e.g. `let (GitBranch b) = ...`) → bypasses the intended accessor and is inconsistent with the qualified-module convention → use `GitBranch.value`, `GitCommit.value`, `GitRepository.value`, `DockerImageVersion.value`.

- **Passing real `null` or omitted metadata as a union value** → the unions wrap a `string` and the empty case is explicit → use the module `empty` value (e.g. `GitRepository.empty`) for absent metadata.

## Wrong Abstractions

- **Converting `ApplicationStatus` to a plain immutable F# record** → it is intentionally `[<CLIMutable>]` with `[<XmlRoot("appStatus")>]` and per-field `[<XmlElement>]` names; removing those breaks XML serialization and the `appStatus` element shape → keep the attributes intact and treat the record as the serialization contract.

- **Building a parallel custom status record to "extend" the payload** → the XML element names and structure are the shared contract consumers rely on → if you only need a subset, still produce the standard record via `create` and ignore the unused fields.

## Legacy Usage

- **Hardcoding instance/environment strings instead of constructing them via `Alma.ServiceIdentification`** → the `Instance`/`Environment` types carry parsing and tier-derivation rules that `create` depends on → construct them with that library's parsers so `Name`, `Environment`, and `Tier` are populated correctly.
