

namespace DotnetDependencyAnalysis

open System
open Xunit

module Tests =

    [<Fact>]
    let ``My test`` () =
        Assert.True(true)
    
    [<Fact>]
    let ``My test 2`` () =
        Assert.True(false)

    let test 
        a
        b
        c
        d
        e =
        a + b + c + d + e