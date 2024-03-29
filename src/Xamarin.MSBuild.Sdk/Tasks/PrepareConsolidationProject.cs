﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using static Xamarin.PathHelpers;

namespace Xamarin.MSBuild.Sdk.Tasks
{
    public sealed class PrepareConsolidationProject : Task
    {
        [Required]
        public string MSBuildProjectFullPath { get; set; }

        public string ConsolidationConditionMetadataName { get; set; }

        public ITaskItem[] ConsolidateRemoveItemsRegex { get; set; }

        [Output]
        public TaskItem[] CompileItems { get; set; }

        [Output]
        public TaskItem[] ProjectReferenceItems { get; set; }

        [Output]
        public TaskItem[] ReferenceItems { get; set; }

        [Output]
        public TaskItem[] PackageReferenceItems { get; set; }

        [Output]
        public TaskItem[] EmbeddedResourceItems { get; set; }

        static readonly string [] embeddedResourceMetadataPathNames = {
            "DependentUpon",
            "LastGenOutput"
        };

        public override bool Execute ()
        {
            var dependencyGraph = DependencyGraph
                .Create (MSBuildProjectFullPath)
                .LoadGraph ();

            var projectPath = ResolveFullPath (MSBuildProjectFullPath);
            var allItemSpecs = new HashSet<string> ();

            var compileItems = new List<TaskItem> ();
            var projectReferenceItems = new List<TaskItem> ();
            var referenceItems = new List<TaskItem> ();
            var packageReferenceItems = new List<TaskItem> ();
            var embeddedResourceItems = new List<TaskItem> ();

            var projectsToConsolidate = new List<ProjectDependencyNode> ();
            foreach (var project in dependencyGraph.TopologicallySortedProjects) {
                if (ResolveFullPath (project.ProjectPath) == projectPath)
                    continue;

                if (string.IsNullOrEmpty (ConsolidationConditionMetadataName)
                    || project.ProjectReferenceItems.Any (
                    pr => bool.TryParse (
                        pr.GetMetadataValue (ConsolidationConditionMetadataName),
                        out var consolidate) && consolidate)) {
                    projectsToConsolidate.Add (project);
                } else {
                    // Keep top-level ProjectReferences that are not marked for
                    // consolidation. Otherwise they get lost due to the removal
                    // step in the PrepareConsolidationProject target.
                    var projectDirectory = Path.GetDirectoryName (project.ProjectPath);
                    foreach (var item in project.ProjectReferenceItems) {
                        var itemSpec = item.EvaluatedInclude;
                        var itemSpecFullPath = ResolveFullPath (
                            projectDirectory,
                            itemSpec);
                        var itemMetadata = GetItemMetadata (item);

                        if (itemMetadata.Count == 0)
                            continue;

                        if (!File.Exists (itemSpecFullPath))
                            continue;

                        if (allItemSpecs.Contains (itemSpecFullPath))
                            continue;

                        allItemSpecs.Add (itemSpecFullPath);

                        projectReferenceItems.Add (new TaskItem (
                            itemSpecFullPath,
                            itemMetadata));
                    }
                }
            }

            foreach (var project in projectsToConsolidate) {
                var projectDirectory = Path.GetDirectoryName (project.ProjectPath);

                foreach (var item in project.Project.AllEvaluatedItems) {
                    if (ShouldExcludeItem (item))
                        continue;

                    List<TaskItem> collection;

                    var itemSpec = item.EvaluatedInclude;
                    var useItemSpecFullPath = false;
                    var itemSpecFullPath = ResolveFullPath (
                        projectDirectory,
                        itemSpec);
                    var itemMetadata = GetItemMetadata (item);

                    switch (item.ItemType.ToLowerInvariant()) {
                    case "compile":
                        collection = compileItems;
                        useItemSpecFullPath = true;
                        break;
                    case "projectreference":
                        collection = projectReferenceItems;
                        useItemSpecFullPath = true;

                        if (projectsToConsolidate.Any (
                            p => ResolveFullPath (p.ProjectPath) == itemSpecFullPath))
                            continue;

                        break;
                    case "reference":
                        // Skip netstandard facades
                        if (itemMetadata.TryGetValue ("NuGetPackageId", out var packageId)
                            && packageId == "NETStandard.Library")
                            continue;
                        collection = referenceItems;
                        break;
                    case "packagereference":
                        collection = packageReferenceItems;
                        break;
                    case "embeddedresource":
                        collection = embeddedResourceItems;
                        useItemSpecFullPath = true;

                        foreach (var name in embeddedResourceMetadataPathNames) {
                            if (itemMetadata.TryGetValue (name, out var value))
                                itemMetadata [name] = ResolveFullPath (
                                    projectDirectory,
                                    value);
                        }

                        itemMetadata.TryGetValue ("LogicalResource", out var logicalResource);
                        itemMetadata ["LogicalResource"] = logicalResource = ComputeLogicalResourceName (
                            project.Project,
                            item,
                            logicalResource);

                        if (logicalResource.EndsWith (".resx", StringComparison.OrdinalIgnoreCase))
                            itemMetadata ["ManifestResourceName"] = Path.GetFileNameWithoutExtension (logicalResource);

                        break;
                    default:
                        continue;
                    }

                    if (useItemSpecFullPath)
                        itemSpec = itemSpecFullPath;

                    if (allItemSpecs.Contains (itemSpec))
                        continue;

                    allItemSpecs.Add (itemSpec);

                    collection.Add (new TaskItem (
                        itemSpec,
                        itemMetadata));
                }
            }

            CompileItems = compileItems.ToArray ();
            ProjectReferenceItems = projectReferenceItems.ToArray ();
            ReferenceItems = referenceItems.ToArray ();
            PackageReferenceItems = packageReferenceItems.ToArray ();
            EmbeddedResourceItems = embeddedResourceItems.ToArray ();

            return true;

            Dictionary<string, string> GetItemMetadata (ProjectItem item)
                => item.Metadata.ToDictionary (
                    i => i.Name,
                    i => i.EvaluatedValue);

            bool ShouldExcludeItem (ProjectItem item)
                => ConsolidateRemoveItemsRegex
                    ?.Where (regexItem => string.Equals (
                        item.ItemType,
                        regexItem.GetMetadata ("ItemType"),
                        StringComparison.OrdinalIgnoreCase))
                    .Any (regexItem => Regex.IsMatch (
                        item.EvaluatedInclude,
                        regexItem.ItemSpec))
                    ?? false;

            string ComputeLogicalResourceName (
                Project project,
                ProjectItem projectItem,
                string logicalResource)
            {
                if (!string.IsNullOrEmpty (logicalResource))
                    return logicalResource;

                var projectDirectory = Path.GetDirectoryName (project.FullPath);
                var resourceName = Regex.Replace (
                    MakeRelativePath (
                        projectDirectory,
                        Path.Combine (projectDirectory, projectItem.EvaluatedInclude)),
                    @"[\\\/]+",
                    ".");

                var prefix = project.GetPropertyValue ("RootNamespace");

                if (string.IsNullOrEmpty (prefix) && project.FullPath != null)
                    prefix = Path.GetFileNameWithoutExtension (project.FullPath);

                if (string.IsNullOrEmpty (prefix))
                    return resourceName;

                return $"{prefix}.{resourceName}";
            }
        }
    }
}