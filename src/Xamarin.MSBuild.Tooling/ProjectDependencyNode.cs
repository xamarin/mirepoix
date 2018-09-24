// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Evaluation;

namespace Xamarin.MSBuild.Tooling
{
    public sealed class ProjectDependencyNode : IDependencyNode
    {
        readonly List<ProjectDependencyNode> parents = new List<ProjectDependencyNode> ();
        public IReadOnlyList<ProjectDependencyNode> Parents => parents;

        readonly List<ProjectItem> projectReferenceItems = new List<ProjectItem> ();
        public IReadOnlyList<ProjectItem> ProjectReferenceItems => projectReferenceItems;

        public string ProjectPath { get; }
        public string Id { get; }
        public Project Project { get; }
        public string Label { get; }
        public Exception LoadException { get; }

        internal ProjectDependencyNode (
            string projectPath,
            string id,
            Project project,
            Exception loadException)
        {
            ProjectPath = projectPath;
            Id = id;
            Project = project;
            Label = Path.GetFileNameWithoutExtension (projectPath);
            LoadException = loadException;
        }

        internal void AddParent (ProjectDependencyNode parent)
        {
            if (parent != null && !parents.Contains (parent))
                parents.Add (parent);
        }

        internal void AddProjectReferenceItem (ProjectItem projectReferenceItem)
        {
            if (projectReferenceItem != null && !projectReferenceItems.Contains (projectReferenceItem))
                projectReferenceItems.Add (projectReferenceItem);
        }

        public override string ToString ()
            => $"{Id}:{Label}";
    }
}