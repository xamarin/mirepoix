// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Evaluation;

using Mono.Options;

using Xamarin.MSBuild.Tooling;
using Xamarin.MSBuild.Tooling.Solution;

namespace Xamarin.MSBuild.Tool
{
    sealed class GenerateSolutionCommand : FancyCommand
    {
        const string commandName = "slngen";

        public GenerateSolutionCommand () : base (
            commandName,
            "Generate a solution from an MSBuild traversal project")
        {
            Options = new HelpOptionSet (
                $"Usage: {Program.Name} {commandName} PROJECT_FILE [SOLUTION_FILE]",
                "",
                "  PROJECT_FILE    Path to an MSBuild traversal project",
                "",
                "  SOLUTION_FILE   Path where the generated solution should",
                "                  be written. If not specified, the solution",
                "                  will be written alongside PROJECT_FILE.");
        }

        public override int Invoke (IEnumerable<string> arguments)
        {
            var projectPath = arguments.ElementAtOrDefault (0);
            var solutionPath = arguments.ElementAtOrDefault (1);

            if (projectPath == null)
                return Error ("PROJECT_FILE was not specified");

            if (!File.Exists (projectPath))
                return Error ($"Project file does not exist: {projectPath}");

            MSBuildLocator.RegisterMSBuildPath (Program.MSBuildExePath);

            SolutionBuilder
                .FromTraversalProject (projectPath, solutionPath)
                .Write ();

            return 0;
        }
    }
}