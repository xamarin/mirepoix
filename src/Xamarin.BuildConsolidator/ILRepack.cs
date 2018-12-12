//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using ILRepacking;
using RepackKind = ILRepacking.ILRepack.Kind;
using System.Collections.Generic;

namespace Xamarin.BuildConsolidator
{
    public sealed class ILRepack : Task
    {
        [Required]
        public string OutputFile { get; set; }

        [Required]
        public string [] InputAssemblies { get; set; }

        public string [] SearchDirectories { get; set; }
        public bool Internalize { get; set; }
        public bool AllowDuplicateResources { get; set; }
        public bool AllowMultipleAssemblyLevelAttributes { get; set; }
        public bool AllowWildCards { get; set; }
        public bool AllowZeroPeKind { get; set; }
        public string AttributeFile { get; set; }
        public bool CopyAttributes { get; set; }
        public bool DebugInfo { get; set; }
        public bool DelaySign { get; set; }
        public string KeyFile { get; set; }
        public string KeyContainer { get; set; }
        public bool Parallel { get; set; }
        public bool StrongNameLost { get; set; }
        public string TargetKind { get; set; }
        public string TargetPlatformDirectory { get; set; }
        public string TargetPlatformVersion { get; set; }
        public bool UnionMerge { get; set; }
        public string Version { get; set; }
        public bool XmlDocumentation { get; set; }
        public bool NoRepackRes { get; set; }
        public bool KeepOtherVersionReferences { get; set; }
        public bool LineIndexation { get; set; }
        public string [] ExcludeInternalizeMatches { get; set; }
        public string [] AllowedDuplicateTypes { get; set; }
        public string [] AllowedDuplicateNameSpaces { get; set; }
        public string ExcludeFile { get; set; }

        public override bool Execute ()
        {
            var (inputAssemblies, searchPrefixes) = FilterInputAssemblies (InputAssemblies);
            var searchDirectories = new List<string> ();
            searchDirectories.AddRange (ResolvePaths (SearchDirectories)
                .Select (path => {
                    if (File.Exists (path))
                        return Path.GetDirectoryName (path);
                    return path;
                }));
            searchDirectories.AddRange (searchPrefixes);

            Log.LogMessage (MessageImportance.Normal, "Input Assemblies:");
            foreach (var inputAssembly in inputAssemblies)
                Log.LogMessage (MessageImportance.Normal, $"  {inputAssembly}");

            Log.LogMessage (MessageImportance.Normal, "Search Directories:");
            foreach (var searchDirectory in searchDirectories)
                Log.LogMessage (MessageImportance.Normal, $"  {searchDirectory}");

            var repackOptions = new RepackOptions {
                OutputFile = PathHelpers.ResolveFullPath (OutputFile),
                InputAssemblies = inputAssemblies,
                SearchDirectories = searchDirectories.ToArray (),
                Internalize = Internalize,
                AllowDuplicateResources = AllowDuplicateResources,
                AllowMultipleAssemblyLevelAttributes = AllowMultipleAssemblyLevelAttributes,
                AllowWildCards = AllowWildCards,
                AllowZeroPeKind = AllowZeroPeKind,
                AttributeFile = AttributeFile,
                CopyAttributes = CopyAttributes,
                DebugInfo = DebugInfo,
                DelaySign = DelaySign,
                KeyFile = KeyFile,
                KeyContainer = KeyContainer,
                Parallel = Parallel,
                StrongNameLost = StrongNameLost,
                TargetKind =  TargetKind == null
                    ? null
                    : (RepackKind?)Enum.Parse (typeof (RepackKind), TargetKind),
                TargetPlatformDirectory = TargetPlatformDirectory,
                TargetPlatformVersion = TargetPlatformVersion,
                UnionMerge = UnionMerge,
                Version = Version == null
                    ? null
                    : System.Version.Parse (Version),
                XmlDocumentation = XmlDocumentation,
                NoRepackRes = NoRepackRes,
                KeepOtherVersionReferences = KeepOtherVersionReferences,
                LineIndexation = LineIndexation,
                ExcludeFile = ExcludeFile
            };

            if (ExcludeInternalizeMatches != null) {
                foreach (var matchRegex in ExcludeInternalizeMatches)
                   repackOptions.ExcludeInternalizeMatches.Add (new Regex (matchRegex));
            }

            if (AllowedDuplicateTypes != null) {
                foreach (var typeName in AllowedDuplicateTypes) {
                    if (typeName.EndsWith (".*", StringComparison.InvariantCulture))
                        repackOptions.AllowedDuplicateNameSpaces.Add (
                            typeName.Substring (0, typeName.Length - 2));
                    else
                        repackOptions.AllowedDuplicateTypes [typeName] = typeName;
                }
            }

            if (AllowedDuplicateNameSpaces != null)
                repackOptions.AllowedDuplicateNameSpaces.AddRange (AllowedDuplicateNameSpaces);

            repackOptions.Log = true;
            repackOptions.LogVerbose = true;

            var repack = new ILRepacking.ILRepack (
                repackOptions,
                new ILRepackMSBuildLogger (Log));

            repack.Repack ();

            return true;
        }

        static string [] ResolvePaths (string [] paths)
        {
            if (paths == null || paths.Length == 0)
                return Array.Empty<string> ();

            return paths
                .Select (p => PathHelpers.ResolveFullPath (p))
                .ToArray ();
        }

        static (string [] inputAssemblies, string [] searchPrefixes) FilterInputAssemblies (string [] inputAssemblies)
        {
            inputAssemblies = ResolvePaths (inputAssemblies);

            var netstandardPrefixes = inputAssemblies
                .Where (path => Path.GetFileNameWithoutExtension (path) == "netstandard")
                .Select (Path.GetDirectoryName)
                .ToList ();

            var searchPrefixes = new List<string> (netstandardPrefixes);

            inputAssemblies = inputAssemblies
                .Where (path => {
                    foreach (var prefix in netstandardPrefixes) {
                        if (path.StartsWith (prefix, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }

                    path = Path.GetDirectoryName (path);
                    if (!searchPrefixes.Contains (path))
                        searchPrefixes.Add (path);

                    return true;
                })
                .ToArray ();

            return (inputAssemblies, searchPrefixes.ToArray ());
        }
    }
}