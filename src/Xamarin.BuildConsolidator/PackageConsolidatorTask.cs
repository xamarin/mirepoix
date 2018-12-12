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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Versioning;

namespace Xamarin.BuildConsolidator
{
    public sealed class PackageConsolidatorTask : Task
    {
        [Required]
        public string [] PackagesToConsolidate { get; set; }

        [Required]
        public string PackageOutputPath { get; set; }

        [Required]
        public string PackageId { get; set; }

        [Required]
        public string PackageVersion { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string PackageDescription { get; set; }
        public string Authors { get; set; }
        public string Copyright { get; set; }
        public bool PackageRequireLicenseAcceptance { get; set; }
        public string PackageLicenseExpression { get; set; }
        public string PackageLicenseUrl { get; set; }
        public string PackageProjectUrl { get; set; }
        public string PackageIconUrl { get; set; }
        public string PackageReleaseNotes { get; set; }
        public string PackageTags { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryType { get; set; }
        public string AssemblySearchPaths { get; set; }
        public string TempWorkPath { get; set; }

        public override bool Execute ()
        {
            var packageBuilder = new PackageBuilder {
                Id = GetString (PackageId),
                Version = NuGetVersion.Parse (PackageVersion),
                Title = GetString (Title),
                Description = GetString (Description) ?? GetString (PackageDescription),
                Copyright = GetString (Copyright),
                RequireLicenseAcceptance = PackageRequireLicenseAcceptance,
                LicenseUrl = GetUrl (PackageLicenseUrl),
                ProjectUrl = GetUrl (PackageProjectUrl),
                IconUrl = GetUrl (PackageIconUrl),
                Repository = new RepositoryMetadata {
                    Url = GetString (RepositoryUrl),
                    Type = GetString (RepositoryType),
                },
                ReleaseNotes = PackageReleaseNotes
            };

            packageBuilder.Authors.AddRange (GetArray (Authors));
            packageBuilder.Tags.AddRange (GetArray (PackageTags));

            if (!string.IsNullOrEmpty (PackageLicenseExpression)) {
                var expression = NuGetLicenseExpression.Parse (PackageLicenseExpression);
                packageBuilder.LicenseMetadata = new LicenseMetadata (
                    LicenseType.Expression,
                    PackageLicenseExpression,
                    expression,
                    null,
                    LicenseMetadata.CurrentVersion);
            }

            var packageConsolidator = new PackageConsolidator (
                packageBuilder,
                GetAssemblySearchPaths (AssemblySearchPaths),
                GetString (TempWorkPath),
                new ILRepackMSBuildLogger (Log));

            LogMetadata (
                (nameof (PackageId), packageBuilder.Id),
                (nameof (PackageVersion), packageBuilder.Version),
                (nameof (Title), packageBuilder.Title),
                (nameof (Authors), packageBuilder.Authors),
                (nameof (Description), packageBuilder.Description),
                (nameof (Copyright), packageBuilder.Copyright),
                (nameof (PackageTags), packageBuilder.Tags),
                (nameof (PackageRequireLicenseAcceptance), packageBuilder.RequireLicenseAcceptance),
                (nameof (PackageLicenseExpression), packageBuilder.LicenseMetadata?.LicenseExpression),
                (nameof (PackageLicenseUrl), packageBuilder.LicenseUrl),
                (nameof (PackageProjectUrl), packageBuilder.ProjectUrl),
                (nameof (RepositoryType), packageBuilder.Repository?.Type),
                (nameof (RepositoryUrl), packageBuilder.Repository?.Url),
                (nameof (PackageReleaseNotes), packageBuilder.ReleaseNotes));

            var consolidatedPackagePath = PathHelpers.ResolveFullPath (
                PackageOutputPath,
                $"{packageBuilder.Id}.{packageBuilder.Version}.nupkg");

            foreach (var packagePath in PackagesToConsolidate
                .Select (p => PathHelpers.ResolveFullPath (p))
                .Where (p => p != consolidatedPackagePath))
                packageConsolidator.ConsolidatePackage (packagePath);

            packageConsolidator.CreateMergedPackage (consolidatedPackagePath);

            Log.LogMessage (
                MessageImportance.High,
                $"Successfully created package '{consolidatedPackagePath}'.");

            return true;
        }

        void LogMetadata (params (string, object)[] fields)
        {
            Log.LogMessage ("Consolidated Package Metadata:");

            foreach (var (key, value) in fields) {
                string s;
                switch (value) {
                case null:
                    continue;
                case IEnumerable<string> e:
                    s = string.Join (", ", e);
                    if (string.IsNullOrEmpty (s))
                        continue;
                    break;
                default:
                    s = value.ToString ();
                    break;
                }

                Log.LogMessage ($"  {key}: {s}");
            }
        }

        static string GetString (string str)
            => string.IsNullOrWhiteSpace (str) ? null : str;

        static string [] GetArray (string str)
            => str?.Split (new [] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string> ();

        static Uri GetUrl (string uriString)
        {
            if (string.IsNullOrEmpty (uriString))
                return null;

            // Fix up the :// part in case this URI came from MSBuild,
            // which seems to be path-normalizing :// into :/
            var parts = uriString.Split (new [] { ':' }, 2);
            if (parts.Length == 2 &&
                parts [1].Length >= 2 &&
                parts [1][0] == '/' &&
                parts [1][1] != '/')
                uriString = $"{parts [0]}:/{parts [1]}";

            if (Uri.TryCreate (uriString, UriKind.Absolute, out var uri))
                return uri;

            return null;
        }

        static IEnumerable<string> GetAssemblySearchPaths (string assemblySearchPaths)
        {
            foreach (var path in GetArray (assemblySearchPaths)) {
                if (path.Length > 0 && path [0] == '{') {
                    if (path == "{TargetFrameworkDirectory}") {
                        var corlibPath = Path.GetDirectoryName (typeof (object).Assembly.Location);
                        yield return corlibPath;
                        yield return Path.Combine (corlibPath, "Facades");
                    }

                    continue;
                }

                yield return path;
            }
        }
    }
}