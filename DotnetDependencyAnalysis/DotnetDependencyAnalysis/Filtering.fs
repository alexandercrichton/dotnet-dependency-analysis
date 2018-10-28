namespace DotnetDependencyAnalysis

module Filtering =
    
  open DomainTypes
  open System.Text.RegularExpressions

  let filter (regex: Regex) solutions =
    solutions |> List.choose (fun solution ->
      if regex.IsMatch(solution.name) |> not then
        None
      else
        let projects = solution.projects |> List.choose (fun project -> 
          if regex.IsMatch(project.name) |> not then
            None
          else
            let dependencyName = function
              | ProjectDependency p -> p
              | NugetDependency n -> n.name
            let dependencies = 
              project.dependencies 
              |> List.filter (fun d -> d |> dependencyName |> regex.IsMatch)
            Some { project with dependencies = dependencies }
        )
        Some { solution with projects = projects }
    )