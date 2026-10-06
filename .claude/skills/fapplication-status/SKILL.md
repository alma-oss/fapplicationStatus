---
name: fapplication-status
description: Use whenever generating or reviewing F# code that builds an application/health status payload via ApplicationStatus.create, implements ApplicationStatusFeature interfaces (ICurrentApplication, IAssemblyInformation, IDockerApplication), or wraps git/docker metadata in GitBranch, GitCommit, GitRepository, DockerImageVersion. Trigger also on mentions of Alma.ApplicationStatus, an appStatus XML endpoint, exposing instance/environment/tier/version/hostName, or serializing the ApplicationStatus record.
---

# F-Application-Status

Library: [alma-oss/fapplicationStatus](https://github.com/alma-oss/fapplicationStatus)
NuGet: `Alma.ApplicationStatus`

## Purpose

`Alma.ApplicationStatus` provides F# types and interfaces for a common application status representation. A consuming application implements a subset of feature interfaces, and `ApplicationStatus.create` projects whatever is implemented into a flat, XML-serializable `ApplicationStatus` record (instance name, environment, tier, version, git metadata, host name).

## When to Use

- Exposing a structured status/health payload (e.g. an `appStatus` endpoint) from a service.
- Reporting instance, environment, tier, git branch/commit/repository, docker image version, and host name in one record.
- Serializing that status to XML.

## When NOT to Use

- For business/domain health logic or dependency checks — this library only models static identity/build metadata.
- When you need a custom payload shape — the `ApplicationStatus` record and its XML element names are fixed.

## Main Concepts

- `GitBranch`, `GitCommit`, `GitRepository`, `DockerImageVersion` — single-case unions wrapping a `string`.
- `GitBranch` / `GitCommit` / `GitRepository` / `DockerImageVersion` modules — each exposes `value` (unwrap) and `empty`.
- `ApplicationStatusFeature.ICurrentApplication` — required feature: exposes `Instance` and `Environment`.
- `ApplicationStatusFeature.IAssemblyInformation` — optional feature: exposes git branch/commit/repository.
- `ApplicationStatusFeature.IDockerApplication` — optional feature: exposes the docker image version.
- `ApplicationStatus` — `[<CLIMutable>]` `[<XmlRoot("appStatus")>]` record with fixed XML element names; the serialized output shape.
- `ApplicationStatus.create` — factory constrained to `ICurrentApplication`; fills fields from whatever optional interfaces are implemented, defaults missing ones to empty strings, and resolves `HostName` automatically.

## Related Libraries

- `Alma.ServiceIdentification` — supplies `Instance`, `Environment`, `Tier` used by `ICurrentApplication` and `create`.
- `Alma.EnvironmentModel` — environment/tier model backing `Environment`.

## Keywords for Search

Alma.ApplicationStatus, ApplicationStatus, ApplicationStatus.create, ApplicationStatusFeature, ICurrentApplication, IAssemblyInformation, IDockerApplication, GitBranch, GitCommit, GitRepository, DockerImageVersion, appStatus, XmlRoot, CLIMutable, XML serialization, instance, environment, tier, version, hostName, health status, F#, Alma.ServiceIdentification, Alma.EnvironmentModel

## Reference Files

- For composition principles and recommended API usage, read `references/preferred-patterns.md`.
- For known pitfalls and incorrect assumptions, read `references/anti-patterns.md`.
- For worked code examples, read `references/examples.md`.
