namespace DotnetDependencyAnalysis.ConsoleApp

open DotnetDependencyAnalysis
open DotnetDependencyAnalysis.DomainTypes
open System.IO

module Program =

  open System.Text.RegularExpressions

  type Arguments = {
    directory: DirectoryInfo;
    filter: Regex option;
  }
    
  let private parseArguments = function
    | [] -> 
      Rop.Fail [IncorrectArguments]

    | directory::xs -> 
      if not(Directory.Exists directory) then
        Rop.Fail [DirectoryDoesNotExist]
      else 
        let directory = new DirectoryInfo(directory)

        let filter = 
          match xs with
          | [] -> None |> Rop.Ok
          | filter::_ ->
            try
              new Regex(filter) |> Some |> Rop.Ok
            with
            | _ -> Rop.Fail [InvalidFilterRegex]

        filter
        |> Rop.map (fun filter ->
          { directory = directory; filter = filter }
        )

  let private writeDgmlToFile (directory: DirectoryInfo) dgml =
    let fileName = sprintf "%s/output.dgml" directory.FullName
    File.WriteAllText(fileName, dgml)
    fileName

  let private handleResult = function
    | Rop.Ok fileName ->
      printfn "Successfully created %A" fileName
    | Rop.Fail messages ->
      printfn "Errors: "
      for message in messages do
        match message with 
        | IncorrectArguments ->
          printfn "Incorrect arguments"
        | InvalidFilterRegex ->
          printfn "Invalid filter regex"
        | DirectoryDoesNotExist ->
          printfn "Directory does not exist"
        | NoSolutionsFound ->
          printfn "No solutions found in directory"
        | InvalidProjectXml projectFile ->
          printfn "Invalid XML in project file: %s" projectFile
        | MultipleNugetFilesInProject project ->
          printfn "Multiple nuget files found in project: %s" project
        | InvalidNugetsXml nugetsFile ->
          printfn "Invalid XML in nuget packages file: %s" nugetsFile
      printfn "Usage: <exe> <solution-directory> [<filter-regex>]"
            
  [<EntryPoint>]
  let main argv = 
    argv 
    |> List.ofArray
    |> parseArguments
    |> Rop.bind (fun args ->
      Loading.loadSolutions args.directory
      |> Rop.map (fun solutions -> 
        match args.filter with
        | None -> solutions
        | Some filter -> Filtering.filter filter solutions
      )
      |> Rop.map Dgml.build
      |> Rop.map (writeDgmlToFile args.directory)
    )
    |> handleResult
    0