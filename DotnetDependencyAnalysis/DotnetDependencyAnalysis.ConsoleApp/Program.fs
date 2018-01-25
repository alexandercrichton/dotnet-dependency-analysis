namespace DotnetDependencyAnalysis.ConsoleApp

open DotnetDependencyAnalysis
open DotnetDependencyAnalysis.DomainTypes
open DotnetDependencyAnalysis.Analysis
open System.IO

module Program =
    
    let private directoryFromArguments = function
        | [directory] -> 
            if Directory.Exists directory then 
                Rop.Ok (new DirectoryInfo(directory))
            else
                Rop.Fail [DirectoryDoesNotExist]
        | [] -> 
            Rop.Fail [MissingDirectoryArgument]
        | _ -> 
            Rop.Fail [TooManyArguments]

    type FileName = FileName of string
    
    let private outputDgml (directory: DirectoryInfo) dgml =
        let fileName = sprintf "%s/output.dgml" directory.FullName
        File.WriteAllText(fileName, dgml)
        FileName fileName

    let private handleResult = function
        | Rop.Ok fileName ->
            printfn "Successfully created %A" fileName
        | Rop.Fail messages ->
            printfn "Errors: "
            for message in messages do
                match message with 
                | TooManyArguments _ -> 
                    printfn "Too many arguments"
                | MissingDirectoryArgument _ ->
                    printfn "Missing directory argument"
                | DirectoryDoesNotExist _ ->
                    printfn "Directory does not exist"
                | NoSolutionsFound _ ->
                    printfn "No solutions found in directory"
                | InvalidProjectXml projectFile ->
                    printfn "Invalid XML in project file: %s" projectFile
                | MultipleNugetFilesInProject project ->
                    printfn "Multiple nuget files found in project: %s" project
                | InvalidNugetsXml nugetsFile ->
                    printfn "Invalid XML in nuget packages file: %s" nugetsFile
            printfn "Usage: <exe> \"<solution-directory>\""
            
    [<EntryPoint>]
    let main argv = 
        argv 
        |> List.ofArray
        |> directoryFromArguments
        |> Rop.bind (fun directory ->
            directory
            |> analyse 
            |> Rop.bind (outputDgml directory >> Rop.Ok)
        )
        |> handleResult
        0