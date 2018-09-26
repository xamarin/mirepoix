// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

            var solutionDirectory = Path.GetDirectoryName (solutionPath);
            if (string.IsNullOrEmpty (solutionDirectory))
                solutionDirectory = ".";

            Directory.CreateDirectory (solutionDirectory);

            var encoding = new UTF8Encoding (true, true);
            using (var writer = new StreamWriter (solutionPath, false, encoding)
                { NewLine = "\r\n" })
                Write (
                    solution,
                    solutionConfigurations,
                    writer);
        }

        public static void Write (
            SolutionNode solution,
            IEnumerable<ConfigurationPlatform> solutionConfigurations,
            TextWriter writer)
        {
            string FixPath (string path)
                => path.Replace ('/', '\\');

            string GuidString (Guid guid)
                => guid.ToString ("B").ToUpperInvariant ();

            writer.WriteLine ();
            writer.WriteLine ("Microsoft Visual Studio Solution File, Format Version 12.00");
            writer.WriteLine ("# Visual Studio 15");
            writer.WriteLine ("VisualStudioVersion = 15.0.26124.0");
            writer.WriteLine ("MinimumVisualStudioVersion = 15.0.26124.0");

            void WriteAllNodes (SolutionNode node)
            {
                if (node != node.Top) {
                    writer.WriteLine (
                        $"Project(\"{GuidString (node.TypeGuid)}\") = " +
                        $"\"{node.Name}\", " +
                        $"\"{FixPath (node.RelativePath)}\", " +
                        $"\"{GuidString (node.Guid)}\"");
                    writer.WriteLine ("EndProject");
                }

                foreach (var child in node.Children)
                    WriteAllNodes (child);
            }

            WriteAllNodes (solution);

            writer.WriteLine ("Global");

            writer.WriteLine ("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var configuration in solutionConfigurations)
                writer.WriteLine ($"\t\t{configuration.ToSolutionString ()} = {configuration.ToSolutionString ()}");
            writer.WriteLine ("\tEndGlobalSection");

            writer.WriteLine ("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine ("\t\tHideSolutionNode = FALSE");
            writer.WriteLine ("\tEndGlobalSection");

            writer.WriteLine ("\tGlobalSection(NestedProjects) = preSolution");

            void WriteNestedProjects (SolutionNode node)
            {
                if (node != node.Top && node.Parent != node.Top)
                    writer.WriteLine (
                        $"\t\t{GuidString (node.Guid)} = " +
                        $"{GuidString (node.Parent.Guid)}");

                foreach (var child in node.Children)
                    WriteNestedProjects (child);
            }

            WriteNestedProjects (solution);

            writer.WriteLine ("\tEndGlobalSection");

            writer.WriteLine ("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

            void WriteConfigurations (SolutionNode node)
            {
                foreach (var configuration in node.Configurations) {
                    void WriteConfiguration (string property)
                        => writer.WriteLine (
                            $"\t\t{GuidString (node.Guid)}.{configuration.Solution.ToSolutionString ()}" +
                            $".{property} = {configuration.Project.ToSolutionString ()}");

                    WriteConfiguration ("ActiveCfg");

                    if (configuration.BuildEnabled)
                        WriteConfiguration ("Build.0");
                }

                foreach (var child in node.Children)
                    WriteConfigurations (child);
            }

            WriteConfigurations (solution);

            writer.WriteLine ("\tEndGlobalSection");

            writer.WriteLine ("EndGlobal");
        }
    }
}