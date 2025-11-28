module Alma.ApplicationStatus.CreationTest

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

type ApplicationStatusTestCase<'Application when 'Application :> ApplicationStatusFeature.ICurrentApplication> = {
    CurrentApplication: 'Application
    ExpectedStatus: ApplicationStatus
    Description: string
}

let provideApplicationStatus: ApplicationStatusTestCase<_> list =
    let hostName = try Dns.GetHostName() with _ -> ""

    [
        {
            Description = "Application with all interfaces"
            CurrentApplication = {
                new ApplicationStatusFeature.ICurrentApplication with
                    member __.Instance = instance "lmc-service-common-stable"
                    member __.Environment = environment "dev1-services"

                interface ApplicationStatusFeature.IAssemblyInformation with
                    member __.GitBranch = GitBranch "GitBranch"
                    member __.GitCommit = GitCommit "GitCommit"
                    member __.GitRepository = GitRepository "GitRepository"

                interface ApplicationStatusFeature.IDockerApplication with
                    member __.DockerImageVersion = DockerImageVersion "DockerImageVersion"
            }
            ExpectedStatus = {
                Name = "lmc-service-common-stable"
                Environment = "dev1-services"
                Tier = "dev"
                Version = "DockerImageVersion"
                BuildBranch = "GitBranch"
                SourceRevision = "GitCommit"
                Repository = "GitRepository"
                HostName = hostName
            }
        }
        {
            Description = "Application with some interfaces and some other"
            CurrentApplication = {
                new ApplicationStatusFeature.ICurrentApplication with
                    member __.Instance = instance "lmc-service-common-stable"
                    member __.Environment = environment "dev1-services"

                interface ApplicationStatusFeature.IAssemblyInformation with
                    member __.GitBranch = GitBranch "GitBranch"
                    member __.GitCommit = GitCommit "GitCommit"
                    member __.GitRepository = GitRepository "GitRepository"

                interface System.IDisposable with
                    member __.Dispose() = failtestf "This method should not be called."
            }
            ExpectedStatus = {
                Name = "lmc-service-common-stable"
                Environment = "dev1-services"
                Tier = "dev"
                Version = ""
                BuildBranch = "GitBranch"
                SourceRevision = "GitCommit"
                Repository = "GitRepository"
                HostName = hostName
            }
        }
        {
            Description = "Application with current application only"
            CurrentApplication = {
                new ApplicationStatusFeature.ICurrentApplication with
                    member __.Instance = instance "lmc-service-common-stable"
                    member __.Environment = environment "dev1-services"
            }
            ExpectedStatus = {
                Name = "lmc-service-common-stable"
                Environment = "dev1-services"
                Tier = "dev"
                Version = ""
                BuildBranch = ""
                SourceRevision = ""
                Repository = ""
                HostName = hostName
            }
        }
    ]

[<Tests>]
let creationTest =
    testList "ApplicationStatus - create" [
        yield!
            provideApplicationStatus
            |> List.map (fun { CurrentApplication = currentApplication; ExpectedStatus = expectedStatus; Description = description } ->
                testCase description <| fun _ ->
                    let actual = currentApplication |> ApplicationStatus.create

                    Expect.equal actual expectedStatus description
            )
    ]
