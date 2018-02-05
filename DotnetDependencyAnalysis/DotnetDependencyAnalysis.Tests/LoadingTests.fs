namespace DotnetDependencyAnalysis.Tests

open System
open System.IO
open DotnetDependencyAnalysis
open DotnetDependencyAnalysis.DomainTypes
open DotnetDependencyAnalysis.Rop
open Xunit

module LoadingTests =

    let absoluteDirectoryPath directory = 
        AppDomain.CurrentDomain.BaseDirectory + "/LoadingTestFiles/" + directory

    let loadSolutionsFromTestDirectory directory =
        absoluteDirectoryPath directory
        |> (fun directory -> new DirectoryInfo(directory))
        |> Loading.loadSolutions
    
    [<Fact>]
    let ``Empty directory`` () =
        let result = loadSolutionsFromTestDirectory "EmptyDirectory"

        let expected =  Fail [NoSolutionsFound]

        Assert.Equal(expected, result)
    
    [<Fact>]
    let ``Csharp NET framework solution`` () =
        let result = loadSolutionsFromTestDirectory "DotnetFrameworkSolution"

        let expected =  Ok [
            {
                name = "DotnetFrameworkSolution";
                projects = [
                    {
                        name = "DotnetFrameworkSolution.DependencyProject";
                        frameworks = [NetFramework];
                        dependencies = [];
                    };
                    {
                        name = "DotnetFrameworkSolution";
                        frameworks = [NetFramework];
                        dependencies = [
                            ProjectDependency "DotnetFrameworkSolution.DependencyProject";
                            NugetDependency {
                                name = "Newtonsoft.Json";
                                version = "10.0.3";
                            }
                        ];
                    }
                ];
            }
        ]

        Assert.Equal(expected, result)
    
    [<Fact>]
    let ``Csharp NET core solution`` () =
        let result = loadSolutionsFromTestDirectory "DotnetCoreSolution"

        let expected =  Ok [
            {
                name = "DotnetCoreSolution";
                projects = [
                    {
                        name = "DotnetCoreSolution.DependencyProject";
                        frameworks = [NetStandard];
                        dependencies = [];
                    };
                    {
                        name = "DotnetCoreSolution";
                        frameworks = [NetCore];
                        dependencies = [
                            ProjectDependency "DotnetCoreSolution.DependencyProject";
                            NugetDependency {
                                name = "Newtonsoft.Json";
                                version = "10.0.3";
                            }
                        ];
                    }
                ];
            }
        ]

        Assert.Equal(expected, result)
    
    [<Fact>]
    let ``Fsharp NET core solution`` () =
        let result = loadSolutionsFromTestDirectory "FsharpDotnetCoreSolution"

        let expected =  Ok [
            {
                name = "FsharpDotnetCoreSolution";
                projects = [
                    {
                        name = "FsharpDotnetCoreSolution.Dependency";
                        frameworks = [NetStandard];
                        dependencies = [];
                    };
                    {
                        name = "FsharpDotnetCoreSolution";
                        frameworks = [NetCore];
                        dependencies = [
                            ProjectDependency "FsharpDotnetCoreSolution.Dependency";
                            NugetDependency {
                                name = "Newtonsoft.Json";
                                version = "10.0.3";
                            }
                        ];
                    }
                ];
            }
        ]

        Assert.Equal(expected, result)
        