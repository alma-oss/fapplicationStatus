module Alma.ApplicationStatus.SerializationTest

open Expecto
open System.IO
open Alma.ApplicationStatus

[<RequireQualifiedAccess>]
module XML =
    open System.Xml
    open System.Xml.Serialization

    let private encoding = System.Text.Encoding.ASCII

    let private toString (v: byte array) = encoding.GetString v

    let serialize<'a> (value: 'a) =
        let xmlSerializer = XmlSerializer(typeof<'a>)
        use stream = new MemoryStream()

        use writer = new XmlTextWriter(stream, encoding)
        writer.Formatting <- Formatting.Indented
        writer.Indentation <- 2

        xmlSerializer.Serialize(writer, value)
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
            }

            let actual = XML.serialize<ApplicationStatus> status

            let expected =
                """<?xml version="1.0" encoding="us-ascii"?>
<appStatus xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <name>lmc-service-common-stable</name>
  <environment>dev1-services</environment>
  <tier>dev</tier>
  <version>DockerImageVersion</version>
  <buildBranch>GitBranch</buildBranch>
  <sourceRevision>GitCommit</sourceRevision>
  <repository />
  <hostName>HostName</hostName>
</appStatus>"""

            Expect.equal actual expected "Application status should be serialized to XML"
    ]
