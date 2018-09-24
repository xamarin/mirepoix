// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using static Xamarin.PathHelpers;

using Xamarin.MSBuild.Tooling.Solution;

namespace Xamarin.MSBuild.Tooling.Tests
{
    public class SolutionBuilderTests : MSBuildTestBase
    {
        static readonly string solutionsDirectory = Path.Combine (
            Git.FindRepositoryRootPathFromAssembly (),
            "src",
            "Xamarin.MSBuild.Tooling.Tests",
            "Solutions");

        [Theory]
        [InlineData ("DisabledProjectsInConfig.proj")]
        [InlineData ("UnsupportedProjectDependency.proj")]
        public void GenerateSolution (string projectFile)
        {
            var projectPath = Path.Combine (solutionsDirectory, projectFile);
            var referenceSolutionPath = Path.ChangeExtension (projectPath, ".sln");
            var testSolutionPath = referenceSolutionPath + ".test";

            SolutionBuilder
                .FromTraversalProject (projectPath, testSolutionPath)
                .Write ();

            Assert.Equal (
                File.ReadAllText (referenceSolutionPath),
                File.ReadAllText (testSolutionPath));
        }
    }
}