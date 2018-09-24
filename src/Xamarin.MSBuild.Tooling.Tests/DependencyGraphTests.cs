// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using static Xamarin.PathHelpers;

namespace Xamarin.MSBuild.Tooling.Tests
{
    public class DependencyGraphTests : MSBuildTestBase
    {
        [Fact]
        public async Task ProcessMirepoixSolution ()
        {
            var solutionPath = Path.Combine (
                Git.FindRepositoryRootPathFromAssembly (),
                "mirepoix.proj");

            var dependencyGraph = await DependencyGraph
                .Create (solutionPath, ("Configuration", "Debug"))
                .LoadGraphAsync ();

            Assert.Collection (
                dependencyGraph
                    .ProjectCollection
                    .LoadedProjects
                    .Select (
                        p => Path.GetFileNameWithoutExtension (p.FullPath)),
                        p => Assert.Equal ("Xamarin.ProcessControl", p),
                        p => Assert.Equal ("Xamarin.Downloader.Tests", p),
                        p => Assert.Equal ("Xamarin.Downloader", p),
                        p => Assert.Equal ("Xamarin.XunitHelpers", p),
                        p => Assert.Equal ("Xamarin.ProcessControl.Tests", p),
                        p => Assert.Equal ("Xamarin.Helpers", p),
                        p => Assert.Equal ("Xamarin.NativeHelpers", p),
                        p => Assert.Equal ("Xamarin.Security.Keychain.Tests", p),
                        p => Assert.Equal ("Xamarin.Security.Keychain", p),
                        p => Assert.Equal ("Xamarin.Helpers.Tests", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tooling", p),
                        p => Assert.Equal ("Xamarin.NativeHelpers.Tests", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tool", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tooling.NetFXTests", p));

            Assert.Collection (
                dependencyGraph
                    .TopologicallySortedProjects
                    .Select (
                        p => Path.GetFileNameWithoutExtension (p.Project.FullPath)),
                        p => Assert.Equal ("Xamarin.ProcessControl", p),
                        p => Assert.Equal ("Xamarin.Downloader", p),
                        p => Assert.Equal ("Xamarin.XunitHelpers", p),
                        p => Assert.Equal ("Xamarin.Downloader.Tests", p),
                        p => Assert.Equal ("Xamarin.ProcessControl.Tests", p),
                        p => Assert.Equal ("Xamarin.Helpers", p),
                        p => Assert.Equal ("Xamarin.NativeHelpers", p),
                        p => Assert.Equal ("Xamarin.Security.Keychain", p),
                        p => Assert.Equal ("Xamarin.Security.Keychain.Tests", p),
                        p => Assert.Equal ("Xamarin.Helpers.Tests", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tooling", p),
                        p => Assert.Equal ("Xamarin.NativeHelpers.Tests", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tool", p),
                        p => Assert.Equal ("Xamarin.MSBuild.Tooling.NetFXTests", p));
        }
    }
}