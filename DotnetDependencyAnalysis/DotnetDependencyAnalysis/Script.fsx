#r "System.Xml"
#r "System.Xml.Linq"
#r "System.Linq"

#load "Rop.fs"
#load "DomainTypes.fs"
#load "Loading.fs"

open DotnetDependencyAnalysis
open System.IO

let solutionDirectoy = new DirectoryInfo(@"C:\Dev")

Loading.loadSolutions solutionDirectoy



