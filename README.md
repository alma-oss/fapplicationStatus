Application Status
==================

> Types for an common Application status representation used in LMC.

## Install

Add following into `paket.dependencies`
```
git ssh://git@bitbucket.lmc.cz:7999/archi/nuget-server.git master Packages: /nuget/
# LMC Nuget dependencies:
nuget Lmc.ApplicationStatus
```

Add following into `paket.references`
```
Lmc.ApplicationStatus
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
    NomadJobName: NomadJobName
    NomadAllocationId: NomadAllocationId
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

        interface ApplicationStatusFeature.INomadApplication with
            member __.NomadJobName = currentApplication.NomadJobName
            member __.NomadAllocationId = currentApplication.NomadAllocationId
    }
```

**NOTE**: You can implement as many interfaces as you can. You can skip the rest if you don't provide the functionality.

## Release
1. Increment version in `ApplicationStatus.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it
4. Run `$ ./build.sh -t release`
5. Go to `nuget-server` repo, run `./build.sh -t copyAll` and push new versions

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)
- [FAKE](https://fake.build/fake-gettingstarted.html)

### Build
```bash
./build.sh
```
