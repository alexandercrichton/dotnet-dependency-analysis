namespace DotnetDependencyAnalysis

module Analysis =

    open Rop

    let analyse directory =
        directory
            |> Loading.loadSolutions
            |> Rop.bind (Dgml.build >> Ok)