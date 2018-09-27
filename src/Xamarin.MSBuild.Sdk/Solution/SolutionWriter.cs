// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static Xamarin.PathHelpers;

namespace Xamarin.MSBuild.Sdk.Solution
{
    static class SolutionWriter
    {
        public static void Write (
            SolutionNode solution,
            IEnumerable<ConfigurationPlatform> solutionConfigurations,
            string solutionPath)
        {
            if (solution == null)
                throw new ArgumentNullException (nameof (solution));

            if (solutionPath == null)
                throw new ArgumentNullException (nameof (solutionPath));

            solutionPath = ResolveFullPath (solutionPath);

            var solutionDirectory = Path.GetDirectoryName (solutionPath);
            if (string.IsNullOrEmpty (solutionDirectory))
                solutionDirectory = ".";

            Directory.CreateDirectory (solutionDirectory);

            var slnFile = File.Exists (solutionPath)
                ? SlnFile.Read (solutionPath)
                : new SlnFile {
                    FullPath = solutionPath
                };

            var nestedProjectsSection = slnFile.Sections.GetOrCreateSection (
                "NestedProjects",
                SlnSectionType.PreProcess);
            nestedProjectsSection.Clear ();
            nestedProjectsSection.SkipIfEmpty = true;

            slnFile.Projects.Clear ();
            slnFile.SolutionConfigurationsSection.Clear ();
            slnFile.ProjectConfigurationsSection.Clear ();

            foreach (var solutionConfiguration in solutionConfigurations)
                slnFile.SolutionConfigurationsSection.SetValue (
                    solutionConfiguration.ToSolutionString (),
                    solutionConfiguration.ToSolutionString ());

            void WriteAllNodes (SolutionNode node)
            {
                if (node != node.Top) {
                    var project = slnFile.Projects.GetOrCreateProject (node.Guid.ToSolutionId ());
                    project.TypeGuid = node.TypeGuid.ToSolutionId ();
                    project.Name = node.Name;
                    project.FilePath = node.RelativePath;
                }

                foreach (var child in node.Children)
                    WriteAllNodes (child);
            }

            WriteAllNodes (solution);

            void WriteNestedProjects (SolutionNode node)
            {
                if (node != node.Top && node.Parent != node.Top)
                    nestedProjectsSection
                        .Properties.SetValue (
                        node.Guid.ToSolutionId (),
                        node.Parent.Guid.ToSolutionId ());

                foreach (var child in node.Children)
                    WriteNestedProjects (child);
            }

            WriteNestedProjects (solution);

            void WriteConfigurations (SolutionNode node)
            {
                foreach (var configuration in node.Configurations) {
                    var propertySet = slnFile
                        .ProjectConfigurationsSection
                        .GetOrCreatePropertySet (
                            node.Guid.ToSolutionId (),
                            ignoreCase: true);

                    void SetProperty (string property)
                        => propertySet.SetValue (
                            $"{configuration.Solution.ToSolutionString ()}.{property}",
                            configuration.Project.ToSolutionString ());

                    SetProperty ("ActiveCfg");

                    if (configuration.BuildEnabled)
                        SetProperty ("Build.0");
                }

                foreach (var child in node.Children)
                    WriteConfigurations (child);
            }

            WriteConfigurations (solution);

            slnFile.Write ();
        }

        static string ToSolutionId (this Guid guid)
            => guid.ToString ("B").ToUpperInvariant ();
    }
}