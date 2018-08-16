// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using static Xamarin.IO.PathHelpers;

namespace Xamarin.MSBuild.Tooling
{
    /// <summary>
    /// Uses MSBuild to load and evaluate all projects from a set of project and/or solution
    /// paths on disk by recursively resolving any `&lt;ProjectReference&gt;` nodes. This
    /// class is immutable.
    /// </summary>
    public class DependencyGraph
    {
        public delegate ProjectCollection ProjectCollectionFactory (IDictionary<string, string> globalProperties);

        static DependencyGraph ()
        {
        }

        static ProjectCollection DefaultProjectCollectionFactory (IDictionary<string, string> globalProperties)
            => new ProjectCollection (globalProperties);

        static Dictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> items)
        {
            var newItems = new Dictionary<string, string> ();
            if (items != null) {
                foreach (var property in items)
                    newItems.Add (property.Key, property.Value);
            }
            return newItems;
        }

        static IEnumerable<T> Yield<T> (T item)
        {
            yield return item;
        }

        readonly ImmutableList<string> solutionOrProjectPaths;
        readonly ProjectCollectionFactory projectCollectionFactory;

        /// <summary>
        /// The MSBuild collection of loaded projects. Until either <see cref="LoadGraph"/>
        /// or <see cref="LoadGraphAsync"/> is called the collection will be empty, but
        /// will populated with global properties.
        /// </summary>
        public ProjectCollection ProjectCollection { get; }

        readonly ImmutableList<Project> topologicallySortedProjects;

        /// <summary>
        /// A topologically sorted list of loaded projects in <see cref="ProjectCollection"/>.
        /// </summary>
        public IReadOnlyList<Project> TopologicallySortedProjects => topologicallySortedProjects;

        DependencyGraph (
            ImmutableList<string> solutionOrProjectPaths,
            ProjectCollectionFactory projectCollectionFactory,
            ProjectCollection projectCollection,
            ImmutableList<Project> topologicallySortedProjects)
        {
            this.solutionOrProjectPaths = solutionOrProjectPaths;
            this.projectCollectionFactory = projectCollectionFactory;
            ProjectCollection = projectCollection;
            this.topologicallySortedProjects = topologicallySortedProjects;
        }

        /// <param name="solutionOrProjectPaths">The set of solution or projects paths to process.</param>
        /// <param name="globalProperties">Any global properties that should influence evaluation.</param>
        public static DependencyGraph Create (
            IEnumerable<string> solutionOrProjectPaths,
            params (string key, string value)[] globalProperties)
            => Create (
                solutionOrProjectPaths,
                (IEnumerable<(string, string)>)globalProperties);

        /// <param name="solutionOrProjectPaths">The set of solution or projects paths to process.</param>
        /// <param name="globalProperties">Any global properties that should influence evaluation.</param>
        public static DependencyGraph Create (
            IEnumerable<string> solutionOrProjectPaths,
            IEnumerable<(string key, string value)> globalProperties = null)
            => Create (
                solutionOrProjectPaths,
                globalProperties?.Select (property => new KeyValuePair<string, string> (
                    property.key,
                    property.value)));

        /// <param name="solutionOrProjectPath">A solution or project path to process.</param>
        /// <param name="globalProperties">Any global properties that should influence evaluation.</param>
        public static DependencyGraph Create (
            string solutionOrProjectPath,
            params (string key, string value)[] globalProperties)
            => Create (
                Yield (solutionOrProjectPath),
                (IEnumerable<(string, string)>)globalProperties);

        /// <param name="solutionOrProjectPath">A solution or project path to process.</param>
        /// <param name="globalProperties">Any global properties that should influence evaluation.</param>
        public static DependencyGraph Create (
            string solutionOrProjectPath,
            IEnumerable<(string key, string value)> globalProperties = null)
            => Create (
                Yield (solutionOrProjectPath),
                globalProperties?.Select (property => new KeyValuePair<string, string> (
                    property.key,
                    property.value)));

        /// <param name="solutionOrProjectPaths">The set of solution or projects paths to process.</param>
        /// <param name="globalProperties">Any global properties that should influence evaluation. May be null.</param>
        /// <param name="projectCollectionFactory">
        /// A function that takes a set of global properties and returns an MSBuild <see cref="ProjectCollection"/>.
        /// This is particularly useful if any of the more advanced constructors for <see cref="ProjectCollection"/>
        /// are needed (such as for locating custom tool chains or to provide custom logging).
        /// </param>
        public static DependencyGraph Create (
            IEnumerable<string> solutionOrProjectPaths,
            IEnumerable<KeyValuePair<string, string>> globalProperties,
            ProjectCollectionFactory projectCollectionFactory = null)
        {
            if (solutionOrProjectPaths == null)
                throw new ArgumentNullException (nameof (solutionOrProjectPaths));

            projectCollectionFactory = projectCollectionFactory ?? DefaultProjectCollectionFactory;

            return new DependencyGraph (
                solutionOrProjectPaths.ToImmutableList (),
                projectCollectionFactory,
                projectCollectionFactory (CreateDictionary (globalProperties)),
                ImmutableList<Project>.Empty);
        }

        /// <summary>
        /// Loads and fully resolves all projects in the full graph. This method
        /// may take some time for large projects and will be performed on the
        /// thread pool.
        /// <summary>
        public Task<DependencyGraph> LoadGraphAsync (CancellationToken cancellationToken = default)
            => Task.Run (() => LoadGraph (cancellationToken), cancellationToken);

        /// <summary>
        /// Loads and fully resolves all projects in the full graph. This method
        /// may take some time (block) for large projects.
        /// <summary>
        public DependencyGraph LoadGraph (CancellationToken cancellationToken = default)
        {
            var projects = projectCollectionFactory (CreateDictionary (ProjectCollection.GlobalProperties));
            var loadedProjects = new HashSet<string> ();
            var sortedProjects = ImmutableList.CreateBuilder<Project> ();

            void LoadProject (string path)
            {
                cancellationToken.ThrowIfCancellationRequested ();

                path = ResolveFullPath (path);

                if (!loadedProjects.Add (path))
                    return;

                var project = projects.LoadProject (path);

                foreach (var projectReference in project.GetItems ("ProjectReference")) {
                    cancellationToken.ThrowIfCancellationRequested ();

                    var referencePath = Path.Combine (
                        project.DirectoryPath,
                        projectReference.EvaluatedInclude);

                    if (!File.Exists (referencePath))
                        throw new FileNotFoundException (
                            $"Project '{path}' has a <ProjectReference> that does not exist: '{referencePath}'");

                    LoadProject (referencePath);
                }

                sortedProjects.Add (project);
            }

            foreach (var path in solutionOrProjectPaths) {
                cancellationToken.ThrowIfCancellationRequested ();

                if (string.Equals (Path.GetExtension (path), ".sln", StringComparison.OrdinalIgnoreCase)) {
                    foreach (var solutionProject in SolutionFile.Parse (path).ProjectsInOrder) {
                        cancellationToken.ThrowIfCancellationRequested ();

                        if (solutionProject.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat) {
                            var projectPath = Path.Combine (
                                Path.GetDirectoryName (path),
                                solutionProject.RelativePath);

                            LoadProject (projectPath);
                        }
                    }
                } else {
                    LoadProject (path);
                }
            }

            return new DependencyGraph (
                solutionOrProjectPaths,
                projectCollectionFactory,
                projects,
                sortedProjects.ToImmutableList ());
        }
    }
}