# dotnet-dependency-analysis
This tool can be used to graph dependencies in and between .NET solutions and projects. 
To use it just specify a directory and it will:
1. Find all solutions in the directory (recursively)
2. Detect all project and Nuget package references per solution
3. Output a graph to a file

The file format is DGML, which means it requires Visual Studio (as far as I'm aware). This was just the easiest format that had lots of interactive features.

## Examples
![Whole Graph](https://github.com/alexandercrichton/dotnet-dependency-analysis/blob/master/img/whole-graph.PNG)
Above is the output of running the tool on its own repo. Notice the test solutions are included.
* Blue boxes are solutions
* White boxes inside blue boxes are projects
* White boxes outside blue boxes are Nuget packages
* Orange boxes are Nuget packages that have multiple versions referenced

The interactive features of DGML allow you to do useful things like seeing what projects reference a particular package version:
![Upstream](https://github.com/alexandercrichton/dotnet-dependency-analysis/blob/master/img/upstream.PNG)

You can then right-click to select all incoming dependencies and then hide everything not selected, leaving:
![Upstream 2](https://github.com/alexandercrichton/dotnet-dependency-analysis/blob/master/img/upstream-2.PNG)

The tool supports arbitrary levels of dependencies. For example if there was another solution that depended on any of the projects in this solution, if that solution was included in the directory that's analysed, that solution would appear in the graph, and it would show its projects referencing the projects in this solution.

The tool is particularly useful for finding multiple Nuget package versions referenced across multiple solutions, and finding what references them all the way up the dependency tree. When updating a Nuget package, this makes it easy to see all the solutions and projects that probably need to be updated.
