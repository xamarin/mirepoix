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
    // MSBuild is crashing with a StackOverflowException on .NET Core
    // when calling ProjectCollection.LoadProject, so this test project
    // is actually net471, which means 'dotnet xunit' won't run it,
    // since the dotnet toolchain won't resolve the framework.
    // So just run the tests manually for now, because I hate dealing
    // with xunit.runner.console. Sad.
    static class SadDriver
    {
        public static async Task Main ()
        {
            MSBuildLocator.RegisterMSBuildPath ();

            await new DependencyGraphTests ().ProcessMirepoixSolution ();
        }
    }

    public class DependencyGraphTests
    {
        [Fact (Skip = "Does not run under .NET Core xunit")]
        public async Task ProcessMirepoixSolution ()
        {
            var solutionPath = Path.Combine (
                Git.FindRepositoryRootPathFromAssembly (),
                "mirepoix.sln");

            var dependencyGraph = await DependencyGraph
                .Create (solutionPath, ("Configuration", "Debug"))
                .LoadGraphAsync ();

            Assert.Collection (
                dependencyGraph
                    .ProjectCollection
                    .LoadedProjects
                    .Select (p => Path.GetFileNameWithoutExtension (p.FullPath)),
                p => Assert.Equal ("Xamarin.ProcessControl", p),
                p => Assert.Equal ("Xamarin.ProcessControl.Tests", p),
                p => Assert.Equal ("Xamarin.Downloader", p),
                p => Assert.Equal ("Xamarin.Downloader.Tests", p),
                p => Assert.Equal ("Xamarin.XunitHelpers", p),
                p => Assert.Equal ("Xamarin.Security.Keychain", p),
                p => Assert.Equal ("Xamarin.Security.Keychain.Tests", p),
                p => Assert.Equal ("Xamarin.NativeHelpers", p),
                p => Assert.Equal ("Xamarin.NativeHelpers.Tests", p),
                p => Assert.Equal ("Xamarin.MSBuild.Tooling", p),
                p => Assert.Equal ("Xamarin.Helpers", p),
                p => Assert.Equal ("Xamarin.Helpers.Tests", p));

            Assert.Collection (
                dependencyGraph
                    .TopologicallySortedProjects
                    .Select (p => Path.GetFileNameWithoutExtension (p.FullPath)),
                p => Assert.Equal ("Xamarin.ProcessControl", p),
                p => Assert.Equal ("Xamarin.ProcessControl.Tests", p),
                p => Assert.Equal ("Xamarin.Downloader", p),
                p => Assert.Equal ("Xamarin.XunitHelpers", p),
                p => Assert.Equal ("Xamarin.Downloader.Tests", p),
                p => Assert.Equal ("Xamarin.Security.Keychain", p),
                p => Assert.Equal ("Xamarin.Security.Keychain.Tests", p),
                p => Assert.Equal ("Xamarin.NativeHelpers", p),
                p => Assert.Equal ("Xamarin.NativeHelpers.Tests", p),
                p => Assert.Equal ("Xamarin.Helpers", p),
                p => Assert.Equal ("Xamarin.MSBuild.Tooling", p),
                p => Assert.Equal ("Xamarin.Helpers.Tests", p));
        }
    }
}