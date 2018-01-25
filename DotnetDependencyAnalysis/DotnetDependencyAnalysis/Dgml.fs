namespace DotnetDependencyAnalysis

module Dgml = 

    open System

    open DomainTypes

    type ProjectVersionGroup = {
        name: string
        versions: string list
    }

    let private buildProjectVersionGroups solutions =
        let projects = solutions |> List.collect (fun s -> s.projects)
        let projectNames = projects |> List.map (fun p -> p.name)

        let dependencies = projects |> List.collect (fun p -> p.dependencies) |> List.distinct
        let dependencyNames = 
            dependencies |> List.map (function | ProjectDependency p -> p | NugetDependency n -> n.name)

        let names = (projectNames @ dependencyNames) |> List.distinct

        names
        |> List.map (fun name ->
            let versions = 
                dependencies 
                |> List.choose (function | NugetDependency n -> Some n | _ -> None)
                |> List.filter (fun n -> n.name = name)
                |> List.map (fun n -> n.version)
            { name = name; versions = versions }
        )

    let private projectMultipleVersionsGroupCategoryId = "ProjectMultipleVersionsGroupCategoryId"
    let private solutionCategoryId = "SolutionCategoryId"

    let private solutionId (solution: Solution) =
        sprintf "Sln-%s" solution.name

    let private buildSolutionNodes solutions =
        solutions
        |> List.map(fun solution ->
            let id = solutionId solution
            let s = sprintf "<Node Id=\"%s\" Label=\"%s\" Group=\"Expanded\" Category=\"%s\" />"
            s id solution.name solutionCategoryId
        )
        |> List.fold (+) ""

    let private projectVersionId project version =
        sprintf "%s[%s]" project version

    let private buildProjectNodes solutions =
        buildProjectVersionGroups solutions
        |> List.collect (fun group ->
            match group.versions with
            | [] -> 
                [ sprintf "<Node Id=\"%s\" />" group.name ]
            | versions ->
                let groupNode =
                    let groupNodeCategory = 
                        if versions.Length > 1 then projectMultipleVersionsGroupCategoryId else ""
                    sprintf "<Node Id=\"%s\" Group=\"Expanded\" Category=\"%s\" />" group.name groupNodeCategory
                let versionNodes = versions |> List.map (fun version ->
                    sprintf "<Node Id=\"%s\" />" (projectVersionId group.name version)
                )
                groupNode::versionNodes
        )
        |> List.fold (+) ""

    let private buildNodes solutions =
        let solutionNodes = buildSolutionNodes solutions
        let projectNodes = buildProjectNodes solutions
        sprintf "<Nodes>%s%s</Nodes>" solutionNodes projectNodes

    let private buildSolutionToProjectLinks solutions =
        solutions 
        |> List.collect (fun solution ->
            solution.projects 
            |> List.map (fun project ->
                let solutionId = solutionId solution
                sprintf "<Link Source=\"%s\" Target=\"%s\" Category=\"Contains\" />" solutionId project.name
            )
        )
        |> List.fold (+) ""

    let private buildProjectToVersionLinks solutions =
        buildProjectVersionGroups solutions
        |> List.collect (fun group ->
            match group.versions with
            | [] -> []
            | versions -> versions |> List.map(fun version ->
                let source = group.name
                let target = projectVersionId group.name version
                sprintf "<Link Source=\"%s\" Target=\"%s\" Category=\"Contains\" />" source target
            )
        )
        |> List.fold (+) ""

    let private buildProjectToDependencyLinks solutions =
        solutions 
        |> List.collect (fun solution ->
            solution.projects |> List.collect (fun project ->
                project.dependencies 
                |> List.choose (function | NugetDependency n -> Some n | _ -> None)
                |> List.map (fun dependency ->
                    let source = project.name
                    let target = projectVersionId dependency.name dependency.version
                    sprintf "<Link Source=\"%s\" Target=\"%s\" />" source target
                )
            )
        )
        |> List.fold (+) ""

    let private buildLinks solutions = 
        let a = buildSolutionToProjectLinks solutions
        let b = buildProjectToVersionLinks solutions
        let c = buildProjectToDependencyLinks solutions
        sprintf "<Links>%s%s%s</Links>" a b c

    let build solutions =
        let nodes = buildNodes solutions
        let links = buildLinks solutions
        let s = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <DirectedGraph Layout=""Sugiyama"" ZoomLevel=""-1"" xmlns=""http://schemas.microsoft.com/vs/2009/dgml"">
            {0}
            {1}
            <Categories>  
                <Category Id=""{2}"" Background=""Orange"" />  
                <Category Id=""{3}"" Background=""#2c89cc"" />  
            </Categories>  
        </DirectedGraph>"
        String.Format(s, nodes, links, projectMultipleVersionsGroupCategoryId, solutionCategoryId)
