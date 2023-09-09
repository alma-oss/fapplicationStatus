namespace Alma.ApplicationStatus

open Alma.ServiceIdentification
open Alma.EnvironmentModel

//
// Common types
//

type GitBranch = GitBranch of string
type GitCommit = GitCommit of string
type GitRepository = GitRepository of string

type DockerImageVersion = DockerImageVersion of string

type NomadJobName = NomadJobName of string
type NomadAllocationId = NomadAllocationId of string

[<RequireQualifiedAccess>]
module GitBranch =
    let value (GitBranch value) = value
    let empty = GitBranch ""

[<RequireQualifiedAccess>]
module GitCommit =
    let value (GitCommit value) = value
    let empty = GitCommit ""

[<RequireQualifiedAccess>]
module GitRepository =
    let value (GitRepository value) = value
    let empty = GitRepository ""

[<RequireQualifiedAccess>]
module DockerImageVersion =
    let value (DockerImageVersion value) = value
    let empty = DockerImageVersion ""

[<RequireQualifiedAccess>]
module NomadJobName =
    let value (NomadJobName value) = value
    let empty = NomadJobName ""

[<RequireQualifiedAccess>]
module NomadAllocationId =
    let value (NomadAllocationId value) = value
    let empty = NomadAllocationId ""

//
// Application Status Interfaces
//

[<RequireQualifiedAccess>]
module ApplicationStatusFeature =
    type ICurrentApplication =
        abstract member Instance: Instance
        abstract member Environment: Environment

    type IAssemblyInformation =
        abstract member GitBranch: GitBranch
        abstract member GitCommit: GitCommit
        abstract member GitRepository: GitRepository

    type IDockerApplication =
        abstract member DockerImageVersion: DockerImageVersion

    type INomadApplication =
        abstract member NomadJobName: NomadJobName
        abstract member NomadAllocationId: NomadAllocationId

    module internal Matching =
        let (|IsCurrentApplication|_|): obj -> ICurrentApplication option = box >> function
            | :? ICurrentApplication as currentApplication -> Some currentApplication
            | _ -> None

        let (|IsAssemblyInfo|_|): obj -> IAssemblyInformation option = box >> function
            | :? IAssemblyInformation as assemblyInfo -> Some assemblyInfo
            | _ -> None

        let (|IsDockerApplication|_|): obj -> IDockerApplication option = box >> function
            | :? IDockerApplication as dockerAppliation -> Some dockerAppliation
            | _ -> None

        let (|IsNomadApplication|_|): obj -> INomadApplication option = box >> function
            | :? INomadApplication as nomadApplication -> Some nomadApplication
            | _ -> None

//
// Application Status
//

open System.Xml.Serialization

[<CLIMutable>]
[<XmlRoot("appStatus")>]
type ApplicationStatus = {
    [<XmlElement("name")>] Name: string
    [<XmlElement("environment")>] Environment: string
    [<XmlElement("tier")>] Tier: string
    [<XmlElement("version")>] Version: string
    [<XmlElement("buildBranch")>] BuildBranch: string
    [<XmlElement("sourceRevision")>] SourceRevision: string
    [<XmlElement("repository")>] Repository: string
    [<XmlElement("hostName")>] HostName: string
    [<XmlElement("nomadJobName")>] NomadJobName: string
    [<XmlElement("nomadAllocId")>] NomadAllocId: string
}

[<RequireQualifiedAccess>]
module ApplicationStatus =
    open System.Net
    open ApplicationStatusFeature.Matching

    let create<'Application when 'Application :> ApplicationStatusFeature.ICurrentApplication> (application: 'Application) =
        {
            Name =
                match application with
                | IsCurrentApplication currentApplication -> currentApplication.Instance |> Instance.concat "-"
                | _ -> ""
            Environment =
                match application with
                | IsCurrentApplication currentApplication -> currentApplication.Environment |> Environment.value
                | _ -> ""
            Tier =
                match application with
                | IsCurrentApplication currentApplication ->
                    match currentApplication.Environment |> Environment.toTuple with
                    | Ok (tier, _, _) -> tier |> Tier.value
                    | _ -> ""
                | _ -> ""

            BuildBranch =
                match application with
                | IsAssemblyInfo assemblyInformation -> assemblyInformation.GitBranch |> GitBranch.value
                | _ -> ""
            SourceRevision =
                match application with
                | IsAssemblyInfo assemblyInformation -> assemblyInformation.GitCommit |> GitCommit.value
                | _ -> ""
            Repository =
                match application with
                | IsAssemblyInfo assemblyInformation -> assemblyInformation.GitRepository |> GitRepository.value
                | _ -> ""

            Version =
                match application with
                | IsDockerApplication dockerApplication -> dockerApplication.DockerImageVersion |> DockerImageVersion.value
                | _ -> ""

            HostName = try Dns.GetHostName() with _ -> ""

            NomadJobName =
                match application with
                | IsNomadApplication nomadApplication -> nomadApplication.NomadJobName |> NomadJobName.value
                | _ -> ""
            NomadAllocId =
                match application with
                | IsNomadApplication nomadApplication -> nomadApplication.NomadAllocationId |> NomadAllocationId.value
                | _ -> ""
        }
