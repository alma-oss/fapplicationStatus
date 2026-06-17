# Examples

Worked examples ordered by increasing complexity. Each is self-contained.

## Basic — wrapping and unwrapping metadata

```fsharp
open Alma.ApplicationStatus

let branch = GitBranch "main"
let branchValue = GitBranch.value branch     // "main"

// Use the explicit empty value for absent metadata
let repository = GitRepository.empty          // GitRepository ""
let version = DockerImageVersion "1.4.2"
```

## Realistic — building a status from an application

`ApplicationStatus.create` requires `ICurrentApplication` and picks up any optional
feature interfaces supplied through the object expression.

```fsharp
open Alma.ApplicationStatus
open Alma.ServiceIdentification
open Alma.EnvironmentModel

type ServiceA = {
    Instance: Instance
    Environment: Environment
    DockerImageVersion: DockerImageVersion
}

let createStatus (app: ServiceA) =
    ApplicationStatus.create {
        new ApplicationStatusFeature.ICurrentApplication with
            member __.Instance = app.Instance
            member __.Environment = app.Environment

        interface ApplicationStatusFeature.IDockerApplication with
            member __.DockerImageVersion = app.DockerImageVersion
    }
```

## Integration — git metadata from generated assembly attributes

Source git branch/commit from the assembly metadata attributes generated at build time,
and fall back to `empty` where a value is not available.

```fsharp
open Alma.ApplicationStatus

let createStatusWithAssemblyInfo (app: ServiceA) =
    ApplicationStatus.create {
        new ApplicationStatusFeature.ICurrentApplication with
            member __.Instance = app.Instance
            member __.Environment = app.Environment

        interface ApplicationStatusFeature.IAssemblyInformation with
            member __.GitBranch = GitBranch AssemblyVersionInformation.AssemblyMetadata_gitbranch
            member __.GitCommit = GitCommit AssemblyVersionInformation.AssemblyMetadata_gitcommit
            member __.GitRepository = GitRepository.empty

        interface ApplicationStatusFeature.IDockerApplication with
            member __.DockerImageVersion = app.DockerImageVersion
    }
```

## XML serialization

The `ApplicationStatus` record serializes to an `appStatus` document with fixed element names.

```fsharp
open System.IO
open System.Xml
open System.Xml.Serialization
open Alma.ApplicationStatus

let serialize (status: ApplicationStatus) =
    let encoding = System.Text.Encoding.ASCII
    let serializer = XmlSerializer(typeof<ApplicationStatus>)
    use stream = new MemoryStream()
    use writer = new XmlTextWriter(stream, encoding)
    writer.Formatting <- Formatting.Indented
    writer.Indentation <- 2
    serializer.Serialize(writer, status)
    encoding.GetString(stream.ToArray())

// Produces:
// <appStatus ...>
//   <name>service-a-stable</name>
//   <environment>dev1-services</environment>
//   <tier>dev</tier>
//   <version>1.4.2</version>
//   <buildBranch>main</buildBranch>
//   <sourceRevision>abc1234</sourceRevision>
//   <repository />
//   <hostName>...</hostName>
// </appStatus>
```

## Test — verifying `ApplicationStatus.create`

Implement the feature interfaces through an object expression and assert on the record.
Resolve the expected host name the same way the library does, since it is environment-dependent.

```fsharp
module ServiceA.StatusTest

open Expecto
open System.Net
open Alma.EnvironmentModel
open Alma.ServiceIdentification
open Alma.ApplicationStatus

let orFail = function
    | Ok success -> success
    | Error e -> failtestf "%A" e

let instance = Instance.parseStrict "-" >> orFail
let environment = Environment.parse >> orFail

[<Tests>]
let createTest =
    testCase "create fills implemented features, defaults the rest" <| fun _ ->
        let app =
            { new ApplicationStatusFeature.ICurrentApplication with
                member __.Instance = instance "service-a-stable"
                member __.Environment = environment "dev1-services"

              interface ApplicationStatusFeature.IAssemblyInformation with
                member __.GitBranch = GitBranch "main"
                member __.GitCommit = GitCommit "abc1234"
                member __.GitRepository = GitRepository "repo" }

        let expected = {
            Name = "service-a-stable"
            Environment = "dev1-services"
            Tier = "dev"
            Version = ""                  // IDockerApplication not implemented
            BuildBranch = "main"
            SourceRevision = "abc1234"
            Repository = "repo"
            HostName = try Dns.GetHostName() with _ -> ""
        }

        Expect.equal (ApplicationStatus.create app) expected "status should match"
```
