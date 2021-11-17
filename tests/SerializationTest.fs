module Lmc.ApplicationStatus.SerializationTest

open Expecto
open System.IO
open Lmc.ApplicationStatus

[<RequireQualifiedAccess>]
module XML =
    open System.Xml.Serialization

    let private toString (v: byte array) = System.Text.Encoding.ASCII.GetString v

    let serialize<'a> (value: 'a) =
        let xmlSerializer = XmlSerializer(typeof<'a>)
        use stream = new MemoryStream()
        xmlSerializer.Serialize(stream, value)
        toString <| stream.ToArray()

[<Tests>]
let serializationTest =
    testList "ApplicationStatus - serialization" [
        testCase "to xml" <| fun _ ->
            let status = {
                Name = "lmc-service-common-stable"
                Environment = "dev1-services"
                Tier = "dev"
                Version = "DockerImageVersion"
                BuildBranch = "GitBranch"
                SourceRevision = "GitCommit"
                Repository = ""
                HostName = "HostName"
                NomadJobName = "NomadJobName"
                NomadAllocId = "NomadAllocationId"
            }

            let actual = XML.serialize status

            let expected =
                """<?xml version="1.0"?>
<appStatus xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <name>lmc-service-common-stable</name>
  <environment>dev1-services</environment>
  <tier>dev</tier>
  <version>DockerImageVersion</version>
  <buildBranch>GitBranch</buildBranch>
  <sourceRevision>GitCommit</sourceRevision>
  <repository />
  <hostName>HostName</hostName>
  <nomadJobName>NomadJobName</nomadJobName>
  <nomadAllocId>NomadAllocationId</nomadAllocId>
</appStatus>"""

            Expect.equal actual expected "Application status should be serialized to XML"
    ]
