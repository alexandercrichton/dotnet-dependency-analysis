

namespace DotnetDependencyAnalysis.Tests

open Xunit
open DotnetDependencyAnalysis
open DomainTypes

module DgmlTests =
    open System.Xml.Linq

    [<Fact>]
    let ``No solutions`` () =
        let solutions = [] 
        let result = Dgml.build solutions |> XDocument.Parse

        let expected = XDocument.Parse @"<?xml version=""1.0"" encoding=""utf-8""?>
        <DirectedGraph Layout=""Sugiyama"" ZoomLevel=""-1"" xmlns=""http://schemas.microsoft.com/vs/2009/dgml"">
            <Nodes></Nodes>
            <Links></Links>
            <Categories>  
                <Category Id=""ProjectMultipleVersionsGroupCategoryId"" Background=""Orange"" />  
                <Category Id=""SolutionCategoryId"" Background=""#2c89cc"" />  
            </Categories>  
        </DirectedGraph>"

        XNode.DeepEquals(result, expected) |> Assert.True 

    [<Fact>]
    let ``Basic solution`` () =
        let solutions = [ 
            { 
                name = "TestSolution"
                projects = [
                    {
                        name = "TestSolution.Project1"
                        frameworks = [NetFramework]
                        dependencies = []
                    };
                    {
                        name = "TestSolution.Project2"
                        frameworks = [NetFramework]
                        dependencies = [
                            ProjectDependency "TestSolution.Project1";
                            NugetDependency {
                                name = "NugetDependency"
                                version = "1.0"
                            }
                        ]
                    }
                ] 
            }
        ] 

        let result = Dgml.build solutions |> XDocument.Parse

        let expected = XDocument.Parse @"<?xml version=""1.0"" encoding=""utf-8""?>
            <DirectedGraph Layout=""Sugiyama"" ZoomLevel=""-1"" xmlns=""http://schemas.microsoft.com/vs/2009/dgml"">
                <Nodes>
                    <Node Id=""Sln-TestSolution"" Label=""TestSolution"" Group=""Expanded"" Category=""SolutionCategoryId"" />
                    <Node Id=""TestSolution.Project1"" />
                    <Node Id=""TestSolution.Project2"" />
                    <Node Id=""NugetDependency"" Group=""Expanded"" Category="""" />
                    <Node Id=""NugetDependency[1.0]"" />
                </Nodes>
                <Links>
                    <Link Source=""Sln-TestSolution"" Target=""TestSolution.Project1"" Category=""Contains"" />
                    <Link Source=""Sln-TestSolution"" Target=""TestSolution.Project2"" Category=""Contains"" />
                    <Link Source=""NugetDependency"" Target=""NugetDependency[1.0]"" Category=""Contains"" />
                    <Link Source=""TestSolution.Project2"" Target=""NugetDependency[1.0]"" />
                </Links>
                <Categories>
                    <Category Id=""ProjectMultipleVersionsGroupCategoryId"" Background=""Orange"" />
                    <Category Id=""SolutionCategoryId"" Background=""#2c89cc"" />
                </Categories>
            </DirectedGraph>"

        XNode.DeepEquals(result, expected) |> Assert.True 

    [<Fact>]
    let ``Multiple solutions`` () =
        let solutions = [ 
            { 
                name = "MainSolution"
                projects = [
                    {
                        name = "MainSolution.Project"
                        frameworks = [NetFramework]
                        dependencies = [
                            NugetDependency {
                                name = "NugetSolution.Project"
                                version = "1.0"
                            }
                        ]
                    }
                ] 
            };
            { 
                name = "NugetSolution"
                projects = [
                    {
                        name = "NugetSolution.Project"
                        frameworks = [NetFramework]
                        dependencies = []
                    }
                ] 
            }
        ] 

        let result = Dgml.build solutions |> XDocument.Parse

        let expected = XDocument.Parse @"<?xml version=""1.0"" encoding=""utf-8""?>
            <DirectedGraph Layout=""Sugiyama"" ZoomLevel=""-1"" xmlns=""http://schemas.microsoft.com/vs/2009/dgml"">
                <Nodes>
                    <Node Id=""Sln-MainSolution"" Label=""MainSolution"" Group=""Expanded"" Category=""SolutionCategoryId"" />
                    <Node Id=""Sln-NugetSolution"" Label=""NugetSolution"" Group=""Expanded"" Category=""SolutionCategoryId"" />
                    <Node Id=""MainSolution.Project"" />
                    <Node Id=""NugetSolution.Project"" Group=""Expanded"" Category="""" />
                    <Node Id=""NugetSolution.Project[1.0]"" />
                </Nodes>
                <Links>
                    <Link Source=""Sln-MainSolution"" Target=""MainSolution.Project"" Category=""Contains"" />
                    <Link Source=""Sln-NugetSolution"" Target=""NugetSolution.Project"" Category=""Contains"" />
                    <Link Source=""NugetSolution.Project"" Target=""NugetSolution.Project[1.0]"" Category=""Contains"" />
                    <Link Source=""MainSolution.Project"" Target=""NugetSolution.Project[1.0]"" />
                </Links>
                <Categories>
                    <Category Id=""ProjectMultipleVersionsGroupCategoryId"" Background=""Orange"" />
                    <Category Id=""SolutionCategoryId"" Background=""#2c89cc"" />
                </Categories>
            </DirectedGraph>"

        XNode.DeepEquals(result, expected) |> Assert.True 

