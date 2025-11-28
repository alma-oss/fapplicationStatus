Application Status
==================

[![NuGet](https://img.shields.io/nuget/v/Alma.ApplicationStatus.svg)](https://www.nuget.org/packages/Alma.ApplicationStatus)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Alma.ApplicationStatus.svg)](https://www.nuget.org/packages/Alma.ApplicationStatus)
[![Tests](https://github.com/alma-oss/fapplicationStatus/actions/workflows/tests.yaml/badge.svg)](https://github.com/alma-oss/fapplicationStatus/actions/workflows/tests.yaml)

> Types for an common Application status representation.

## Install

Add following into `paket.references`
```
Alma.ApplicationStatus
```

## Use

First of all you need to implement your `CurrentApplication` class/type.

```fs
type CurrentApplication = {
    Instance: Instance
    Environment: Environment
    Dependencies: Dependencies
    ServiceStatus: ServiceStatus
    LoggerFactory: ILoggerFactory
    DockerImageVersion: DockerImageVersion
}

and Dependencies = {
    ConsentsRouter: ConsentsRouter
}
```

This type can either implement `ApplicationStatusFeature` interfaces right away or just in the `createStatus` function
```fs
let createStatus (currentApplication: CurrentApplication) =
    ApplicationStatus.create {
        new ApplicationStatusFeature.ICurrentApplication with
            member __.Instance = currentApplication.Instance
            member __.Environment = currentApplication.Environment

        interface ApplicationStatusFeature.IAssemblyInformation with
            member __.GitBranch = GitBranch AssemblyVersionInformation.AssemblyMetadata_gitbranch
            member __.GitCommit = GitCommit AssemblyVersionInformation.AssemblyMetadata_gitcommit
            member __.GitRepository = GitRepository.empty

        interface ApplicationStatusFeature.IDockerApplication with
            member __.DockerImageVersion = currentApplication.DockerImageVersion
    }
```

**NOTE**: You can implement as many interfaces as you can. You can skip the rest if you don't provide the functionality.

## Release
1. Increment version in `ApplicationStatus.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
