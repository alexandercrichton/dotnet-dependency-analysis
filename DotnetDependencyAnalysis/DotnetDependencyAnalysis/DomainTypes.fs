﻿namespace DotnetDependencyAnalysis

module DomainTypes =

  type Nuget = {
    name: string
    version: string
  }

  type Dependency =
    | ProjectDependency of string
    | NugetDependency of Nuget

  type Framework =
    | NetFramework
    | NetStandard
    | NetCore

  type Project = {
    name: string
    frameworks: Framework list
    dependencies: Dependency list
  }

  type Solution = {
    name: string
    projects: Project list
  }

  type Error =
    | IncorrectArguments
    | InvalidFilterRegex
    | DirectoryDoesNotExist
    | NoSolutionsFound
    | InvalidProjectXml of string
    | MultipleNugetFilesInProject of string
    | InvalidNugetsXml of string

  type Message =
    | DuplicateSolution of string