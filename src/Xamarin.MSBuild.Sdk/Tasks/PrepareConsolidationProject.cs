// Copyright (c) Microsoft Corporation. All rights reserved.
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

            var projectsToConsolidate = dependencyGraph
                .TopologicallySortedProjects
                .Where (project => {
                    if (ResolveFullPath (project.ProjectPath) == projectPath)
                        return false;

                    if (string.IsNullOrEmpty (ConsolidationConditionMetadataName))
                        return true;

                    return project.ProjectReferenceItems.Any (
                        pr => bool.TryParse (
                            pr.GetMetadataValue (ConsolidationConditionMetadataName),
                            out var consolidate) && consolidate);
                })
                .ToList ();

            foreach (var project in projectsToConsolidate) {
                foreach (var item in project.Project.AllEvaluatedItems) {
                    if (ShouldExcludeItem (item))
                        continue;

                    List<TaskItem> collection;

                    var itemSpec = item.EvaluatedInclude;
                    var useItemSpecFullPath = false;
                    var itemSpecFullPath = ResolveFullPath (
                        Path.GetDirectoryName (project.ProjectPath),
                        itemSpec);

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
                        collection = referenceItems;
                        break;
                    default:
                        continue;
                    }

                    if (useItemSpecFullPath)
                        itemSpec = itemSpecFullPath;

                    if (allItemSpecs.Contains (itemSpec))
                        continue;

                    allItemSpecs.Add (itemSpec);

                    var taskItem = new TaskItem (
                        itemSpec,
                        item.Metadata.ToDictionary (
                            m => m.Name,
                            m => m.EvaluatedValue));

                    collection.Add (taskItem);
                }
            }

            CompileItems = compileItems.ToArray ();
            ProjectReferenceItems = projectReferenceItems.ToArray ();
            ReferenceItems = referenceItems.ToArray ();

            return true;

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
        }
    }
}