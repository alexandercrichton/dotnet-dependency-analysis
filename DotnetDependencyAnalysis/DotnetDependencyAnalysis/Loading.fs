namespace DotnetDependencyAnalysis

module Loading = 

    open System.IO
    open System.Xml.Linq
    open System.Xml.XPath

    open DomainTypes
    open System

    let trimLastDot (str: string) =
        let index = str.LastIndexOf('.')
        str.Substring(0, index)

    let private parseXml xml =
        try 
            xml |> XDocument.Parse |> Some
        with    
        | _ -> None

    let private projectFrameworks (projectXml: XDocument) =
        let singleFrameworks = 
            projectXml.XPathSelectElements("//*[local-name() = 'TargetFramework']|//*[local-name() = 'TargetFrameworkVersion']")
            |> Seq.map (fun element -> element.Value)
            |> List.ofSeq

        let multipleFrameworks = 
            projectXml.XPathSelectElements("//TargetFrameworks")
            |> Seq.map (fun element -> element.Value.Split(','))
            |> Seq.collect id
            |> List.ofSeq

        singleFrameworks @ multipleFrameworks
        |> List.map (fun s ->
            match s with
            | "netcoreapp2.0" -> NetCore
            | "netstandard2.0" -> NetStandard
            | _ -> NetFramework
        )

    let private loadProjectDependencies (projectXml: XDocument) =
        projectXml.XPathSelectElements("//*[local-name() = 'ProjectReference']")
        |> List.ofSeq
        |> List.map (fun element -> 
            element.Attributes() 
            |> Seq.find (fun a -> a.Name.LocalName = "Include")
            |> (fun a -> a.Value.Split('\\'))
            |> Array.last
            |> trimLastDot
            |> ProjectDependency
        )

    let private foldListOfResults<'a, 'b> : (Rop.Result<'a, 'b> list -> Rop.Result<'a list, 'b>) =
        List.fold (fun a b -> Rop.bind2 (fun a b -> a::b) b a) Rop.id

    let private nugetDependencyFromElement (element: XElement) nameAttribute versionAttribute =
        let attributeValue attributeName =
            element.Attributes() 
            |> Seq.find (fun a -> a.Name.LocalName = attributeName) 
            |> (fun a -> a.Value)
        let nugetName = attributeValue nameAttribute
        let nugetVersion = attributeValue versionAttribute
        NugetDependency { name = nugetName; version = nugetVersion }
        
    let private loadNugetDependencies projectName (projectFile: FileInfo) (projectXml: XDocument option) =
        let dependenciesFromPackagesConfig =
            let nugetFiles = 
                projectFile.Directory.GetFiles("packages.config", SearchOption.TopDirectoryOnly)
                |> List.ofArray
            match nugetFiles with
            | [] -> 
                Rop.id
            | [file] -> 
                file.FullName 
                |> File.ReadAllText 
                |> parseXml
                |> Option.map (fun nugetsXml -> 
                    nugetsXml.XPathSelectElements("//package")
                    |> List.ofSeq
                    |> List.map (fun element ->
                        nugetDependencyFromElement element "id" "version"
                    )
                    |> Rop.Ok
                )
                |> Option.defaultValue Rop.id
            | _ -> 
                Rop.Fail [MultipleNugetFilesInProject projectName]

        let dependenciesFromProjectXml =
            projectXml
            |> Option.map (fun projectXml ->
                projectXml.XPathSelectElements("//PackageReference")
                |> List.ofSeq
                |> List.map (fun element ->
                    nugetDependencyFromElement element "Include" "Version"
                )
            )
            |> Option.defaultValue []

        dependenciesFromPackagesConfig
        |> Rop.map (fun dependenciesFromPackagesConfig ->
            dependenciesFromPackagesConfig @ dependenciesFromProjectXml
        )

    let private loadProject (projectFile: FileInfo) =
        let projectName = projectFile.Name |> trimLastDot

        let projectXml = 
            projectFile.FullName 
            |> File.ReadAllText 
            |> parseXml

        let frameworks =
            projectXml
            |> Option.map projectFrameworks
            |> Option.defaultValue []
            
        let projectDependencies = 
            projectXml
            |> Option.map loadProjectDependencies
            |> Option.defaultValue []

        let nugetDependencies = loadNugetDependencies projectName projectFile projectXml

        nugetDependencies
        |> Rop.map (fun nugetDependencies -> 
            let allDependencies = projectDependencies @ nugetDependencies
            { name = projectName; frameworks = frameworks; dependencies = allDependencies }
        )
    
    let private loadProjects (solutionFile: FileInfo) =
        let allfiles =
            (solutionFile.Directory.GetFiles("*.csproj", SearchOption.AllDirectories) |> List.ofArray)
            @ (solutionFile.Directory.GetFiles("*.fsproj", SearchOption.AllDirectories) |> List.ofArray)
        allfiles
        |> List.map loadProject
        |> foldListOfResults

    let private loadSolution (solutionFile: FileInfo) =
        let solutionName = solutionFile.Name |> trimLastDot
        loadProjects solutionFile
        |> Rop.map (fun projects -> { name = solutionName; projects = projects })
        
    let loadSolutions (directory: DirectoryInfo) =
        let solutionFiles = 
            directory.GetFiles("*.sln", SearchOption.AllDirectories)
            |> List.ofArray
        match solutionFiles with
        | [] ->
            Rop.Fail [NoSolutionsFound]
        | solutionFiles ->
            solutionFiles 
            |> List.map loadSolution
            |> foldListOfResults