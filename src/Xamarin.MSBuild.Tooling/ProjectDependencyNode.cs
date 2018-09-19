// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

using Microsoft.Build.Evaluation;

namespace Xamarin.MSBuild.Tooling
{
    public sealed class ProjectDependencyNode : IDependencyNode
    {
        public Project Project { get; }
        public string Id { get; }
        public string Label { get; }

        public ProjectDependencyNode (Project project, string id)
        {
            Project = project;
            Id = id;
            Label = Path.GetFileNameWithoutExtension (project.FullPath);
        }

        public override string ToString ()
            => $"{Id}:{Label}";
    }
}