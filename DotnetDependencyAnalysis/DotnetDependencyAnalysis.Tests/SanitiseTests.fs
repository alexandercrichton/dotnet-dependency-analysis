namespace DotnetDependencyAnalysis.Tests

open Xunit
open DotnetDependencyAnalysis
open DotnetDependencyAnalysis.DomainTypes
open DotnetDependencyAnalysis.Sanitise

module SanitiseTests =
    
  [<Fact>]
  let ``Multiple solutions`` () =
    let input = [
      {
        name = "Solution1";
        projects = [];
      };
      {
        name = "Solution2";
        projects = [];
      };
    ]

    let expected = {
      solutions = input;
      messages = [];
    }

    let result = Sanitise.sanitise input

    Assert.Equal(expected, result)
    
  [<Fact>]
  let ``Duplicate solution`` () =
    let solution = {
      name = "Solution1";
      projects = [];
    }
    let input = [solution; solution]

    let expected = {
      solutions = [solution];
      messages = [DuplicateSolution solution.name];
    }

    let result = Sanitise.sanitise input

    Assert.Equal(expected, result)