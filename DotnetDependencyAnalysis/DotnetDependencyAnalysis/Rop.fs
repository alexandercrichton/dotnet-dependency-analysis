namespace DotnetDependencyAnalysis

module Rop =

  type Result<'a, 'b> =
    | Ok of 'a
    | Fail of 'b list

  let id = Ok []

  let bind f = function
    | Ok x -> f x
    | Fail messages -> Fail messages

  let bind2 f a b  =
    match a, b with
    | Ok a, Ok b -> Ok (f a b)
    | Ok _, Fail messages -> Fail messages
    | Fail messages, Ok _ -> Fail messages
    | Fail a, Fail b -> Fail (b @ a)

  let map f =
    bind (fun x -> x |> f |> Ok)