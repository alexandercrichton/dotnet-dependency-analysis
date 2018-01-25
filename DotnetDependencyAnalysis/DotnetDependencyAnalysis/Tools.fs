namespace DotnetDependencyAnalysis

module Tools = 

    let trimFromEnd (trim: string) (str: string) =
        str.Substring(0, str.Length - trim.Length)