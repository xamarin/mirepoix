// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using static Xamarin.PathHelpers;

using Xamarin.MSBuild.Sdk.Solution;

namespace Xamarin.MSBuild.Sdk.Tests
{
    public class SolutionBuilderTests : MSBuildTestBase
    {
        static readonly string solutionsDirectory = Path.Combine (
            Git.FindRepositoryRootPathFromAssembly (),
            "src",
            "Xamarin.MSBuild.Sdk.Tests",
            "Solutions");

        [Theory]
        [InlineData ("Folders.proj", true)]
        [InlineData ("Folders.proj", false)]
        [InlineData ("DisabledProjectsInConfig.proj", true)]
        [InlineData ("DisabledProjectsInConfig.proj", false)]
        [InlineData ("UnsupportedProjectDependency.proj", true)]
        [InlineData ("UnsupportedProjectDependency.proj", false)]
        [InlineData ("FallbackToXmlForProjectGuid.proj", true)]
        [InlineData ("FallbackToXmlForProjectGuid.proj", false)]
        [InlineData ("SharedProjectViaExplicitShprojReference.proj", true)]
        [InlineData ("SharedProjectViaExplicitShprojReference.proj", false)]
        [InlineData ("SharedProjectViaTransitiveProjitemsReference.proj", true)]
        [InlineData ("SharedProjectViaTransitiveProjitemsReference.proj", false)]
        [InlineData ("OmitProjectFromSolution.proj", true)]
        [InlineData ("OmitProjectFromSolution.proj", false)]
        public void GenerateSolution (string projectFile, bool updateExistingSolution)
        {
            var projectPath = Path.Combine (solutionsDirectory, projectFile);
            var referenceSolutionPath = Path.ChangeExtension (projectPath, ".sln");
            var testSolutionPath = referenceSolutionPath + ".test";

            File.Delete (testSolutionPath);

            if (updateExistingSolution)
                File.Copy (referenceSolutionPath, testSolutionPath, true);

            SolutionBuilder
                .FromTraversalProject (projectPath, testSolutionPath)
                .Write ();

            Assert.Equal (
                File.ReadAllText (referenceSolutionPath),
                File.ReadAllText (testSolutionPath));
        }
    }
}