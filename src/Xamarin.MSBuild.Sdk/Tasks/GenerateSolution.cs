// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Xamarin.MSBuild.Sdk.Solution;

namespace Xamarin.MSBuild.Sdk.Tasks
{
    public sealed class GenerateSolution : Task
    {
        [Required]
        public string TraversalProjectFile { get; set; }

        public string SolutionFile { get; set; }

        public override bool Execute ()
        {
            SolutionBuilder
                .FromTraversalProject (
                    TraversalProjectFile,
                    SolutionFile,
                    log: Log)
                .Write ();

            return true;
        }
    }
}