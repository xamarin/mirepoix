// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.MSBuild.Tooling
{
    static class VisJsGenerator
    {
        static readonly string template;

        static VisJsGenerator ()
        {
            using (var stream = typeof (VisJsGenerator)
                .Assembly
                .GetManifestResourceStream ("VisJsTemplate.html"))
            using (var reader = new StreamReader (stream))
                template = reader.ReadToEnd ();
        }

        public static string Generate (DependencyGraph dependencyGraph)
        {
            var nodes = dependencyGraph
                .TopologicallySortedProjects
                .Select (node => $"{{ id: '{node.Id}', label: '{node.Label}'}}")
                .ToList ();

            var edges = dependencyGraph
                .Relationships
                .Select (rel => $"{{ from: '{rel.Dependency.Id}', to: '{rel.Dependent.Id}' }}");

            return template
                .Replace ("// @NODES@", string.Join (",\n", nodes))
                .Replace ("// @EDGES@", string.Join (",\n", edges));
        }
    }
}