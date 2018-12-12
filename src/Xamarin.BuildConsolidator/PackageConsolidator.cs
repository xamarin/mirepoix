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

using ILRepacking;

using NuGet.Frameworks;
using NuGet.Packaging;

namespace Xamarin.BuildConsolidator
{
    public sealed class PackageConsolidator
    {
        sealed class Consolidation
        {
            public readonly NuGetFramework Framework;
            public readonly List<string> AssemblySearchPaths = new List<string> ();
            public readonly List<string> Assemblies = new List<string> ();

            public readonly HashSet<FrameworkAssemblyReference> FrameworkAssemblyReferences
                = new HashSet<FrameworkAssemblyReference> (new FrameworkAssemblyReferenceComparer ());

            public Consolidation (NuGetFramework framework)
                => Framework = framework;
        }

        readonly HashSet<string> consolidatedPackageIds
            = new HashSet<string> (StringComparer.OrdinalIgnoreCase);

        readonly Dictionary<NuGetFramework, Consolidation> consolidations
            = new Dictionary<NuGetFramework, Consolidation> ();

        readonly PackageBuilder packageBuilder;
        readonly IEnumerable<string> assemblySearchPaths;
        readonly string workPath;
        readonly ILogger repackLogger;

        bool consolidationStarted;

        public PackageConsolidator (
            PackageBuilder packageBuilder,
            IEnumerable<string> assemblySearchPaths = null,
            string workPath = null,
            ILogger repackLogger = null)
        {
            this.packageBuilder = packageBuilder
                ?? throw new ArgumentNullException (nameof (packageBuilder));

            this.assemblySearchPaths = assemblySearchPaths ?? Array.Empty<string> ();

            this.workPath = Path.Combine (
                Path.GetTempPath (),
                "com.xamarin.PackageConsolidator",
                "work",
                Path.GetRandomFileName ());

            this.repackLogger = repackLogger;
        }

        public void CreateMergedPackage (string packageFile)
        {
            if (packageFile == null)
                throw new ArgumentNullException (nameof (packageFile));

            foreach (var consolidation in consolidations.Values) {
                var consolidatedAssemblyFile = Path.Combine (
                    Path.GetDirectoryName (consolidation.Assemblies [0]),
                    packageBuilder.Id + ".dll");

                var repackOptions = new RepackOptions {
                    OutputFile = consolidatedAssemblyFile,
                    SearchDirectories = consolidation.AssemblySearchPaths.Concat (assemblySearchPaths),
                    InputAssemblies = consolidation.Assemblies.ToArray (),
                    Log = true,
                    LogVerbose = true
                };

                var repack = repackLogger == null
                    ? new ILRepacking.ILRepack (repackOptions)
                    : new ILRepacking.ILRepack (repackOptions, repackLogger);

                repack.Repack ();

                packageBuilder.FrameworkReferences.AddRange (
                    consolidation.FrameworkAssemblyReferences.OrderBy (o => o.AssemblyName));

                packageBuilder.Files.Add (new PhysicalPackageFile {
                    SourcePath = consolidatedAssemblyFile,
                    TargetPath = consolidatedAssemblyFile.Substring (workPath.Length)
                });
            }

            var filteredDependencyGroups = packageBuilder
                .DependencyGroups
                .Select (dg => new PackageDependencyGroup (
                    dg.TargetFramework,
                    dg.Packages.Where (p => !consolidatedPackageIds.Contains (p.Id))))
                .ToList ();

            packageBuilder.DependencyGroups.Clear ();
            packageBuilder.DependencyGroups.AddRange (filteredDependencyGroups);

            Directory.CreateDirectory (Path.GetDirectoryName (packageFile));
            using (var mergedStream = File.Open (
                packageFile,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite))
                packageBuilder.Save (mergedStream);
        }

        public void ConsolidatePackage (string packageFile)
        {
            if (!consolidationStarted) {
                consolidationStarted = true;
                try {
                    Directory.Delete (workPath, recursive: true);
                } catch {
                }
            }

            Directory.CreateDirectory (workPath);

            var package = new PackageArchiveReader (packageFile);

            if (!consolidatedPackageIds.Add (package.GetIdentity ().Id))
                return;

            packageBuilder.DependencyGroups.AddRange (
                package.GetPackageDependencies ());

            foreach (var framework in package.GetSupportedFrameworks ())
                ProcessPackageFramework (package, framework);
        }

        void ProcessPackageFramework (PackageArchiveReader package, NuGetFramework framework)
        {
            if (!consolidations.TryGetValue (framework, out var consolidation)) {
                consolidation = new Consolidation (framework);
                consolidations.Add (framework, consolidation);
            }

            foreach (var assemblyName in GetItems (package.GetFrameworkItems (), framework))
                consolidation.FrameworkAssemblyReferences.Add (new FrameworkAssemblyReference (
                    assemblyName,
                    new [] { framework }));

            package.CopyFiles (
                workPath,
                GetItems (package.GetLibItems (), framework),
                (sourceFile, targetPath, fileStream) => {
                    var targetDirectory = Path.GetDirectoryName (targetPath);
                    if (!consolidation.AssemblySearchPaths.Contains (targetDirectory))
                        consolidation.AssemblySearchPaths.Add (targetDirectory);

                    Directory.CreateDirectory (targetDirectory);

                    using (var targetStream = File.Open (
                        targetPath,
                        FileMode.Create,
                        FileAccess.Write))
                        fileStream.CopyTo (targetStream);

                    consolidation.Assemblies.Add (targetPath);

                    return targetPath;
                },
                null,
                default);

            consolidation.AssemblySearchPaths.AddRange (package
                .GetPackageDependencies ()
                .Where (d => d.TargetFramework == framework)
                .SelectMany (d => d.Packages)
                .Select (d => NuGetLocalRepoHelper.GetPackageAssemblySearchPath (
                    d.Id,
                    d.VersionRange,
                    framework))
                .Where (d => d != null));
        }

        IEnumerable<string> GetItems (
            IEnumerable<FrameworkSpecificGroup> groups,
            NuGetFramework framework)
            => groups
                .SingleOrDefault (group => group.TargetFramework == framework)
                ?.Items ?? Array.Empty<string> ();
    }
}