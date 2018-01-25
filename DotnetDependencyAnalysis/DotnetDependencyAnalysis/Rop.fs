namespace DotnetDependencyAnalysis

module Rop =

    type Result<'a, 'b> =
        | Ok of 'a
        | Fail of 'b list

    let id = Ok []

    let bind binder = function
        | Ok x -> binder x
        | Fail messages -> Fail messages

    let bind2 binder a b  =
        match a, b with
        | Ok a, Ok b -> Ok (binder a b)
        | Ok _, Fail messages -> Fail messages
        | Fail messages, Ok _ -> Fail messages
        | Fail a, Fail b -> Fail (b @ a)

    let map mapping result = 
        result |> bind (fun x -> x |> mapping |> Ok)