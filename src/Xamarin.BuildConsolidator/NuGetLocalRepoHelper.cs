
//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Repositories;
using NuGet.Versioning;

namespace Xamarin.BuildConsolidator
{
    static class NuGetLocalRepoHelper
    {
        static readonly NuGetv3LocalRepository repo = new NuGetv3LocalRepository (
            Path.Combine (
                NuGetEnvironment.GetFolderPath (NuGetFolderPath.NuGetHome),
                "packages"));

        public static LocalPackageInfo GetPackage (
            string id,
            VersionRange versionRange)
        {
            var packageCandidates = repo
                .FindPackagesById (id)
                .ToList ();

            if (packageCandidates.Count == 0)
                return null;

            var bestVersion = versionRange.FindBestMatch (
                packageCandidates.Select (p => p.Version));

            return packageCandidates.FirstOrDefault (
                p => p.Version == bestVersion);
        }

        public static string GetPackageAssemblySearchPath (
            string id,
            VersionRange versionRange,
            NuGetFramework framework)
            => GetPackageAssemblySearchPath (
                GetPackage (id, versionRange),
                framework);

        public static string GetPackageAssemblySearchPath (
            LocalPackageInfo packageInfo,
            NuGetFramework framework)
        {
            if (packageInfo == null)
                return null;

            if (framework == null)
                throw new ArgumentNullException (nameof (framework));

            var possibleFrameworks = packageInfo.Files
                .Select (path => path.Split (new [] { '/', '\\' }))
                .Where (parts => string.Equals (parts [0], "lib", StringComparison.OrdinalIgnoreCase))
                .Select (parts => NuGetFramework.ParseFolder (parts [1].ToLowerInvariant ()))
                .Distinct ();

            var bestFramework = new FrameworkReducer ()
                .GetNearest (framework, possibleFrameworks);

            return Path.Combine (
                packageInfo.ExpandedPath,
                "lib",
                bestFramework.GetShortFolderName ());
        }
    }
}