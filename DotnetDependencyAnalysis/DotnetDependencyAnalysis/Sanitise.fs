namespace DotnetDependencyAnalysis

module Sanitise =

  open DotnetDependencyAnalysis.DomainTypes

  type SanitisationResult = {
    solutions: Solution list;
    messages: Message list;
  }

  let sanitise solutions =
    let fold (result: SanitisationResult) (solution: Solution) =
      let duplicateSolution = result.solutions |> List.exists (fun s -> s.name = solution.name) 
      if duplicateSolution then
        { result with messages = result.messages @ [DuplicateSolution solution.name] }
      else 
        { result with solutions = result.solutions @ [solution] }

    let acc = { solutions = []; messages = [] }
    List.fold fold acc solutions