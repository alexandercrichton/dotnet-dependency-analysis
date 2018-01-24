module Tests

open System
open Xunit

[<Fact>]
let ``My test`` () =
    Assert.True(true)
    
[<Fact>]
let ``My test 2`` () =
    Assert.True(false)
