// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Xamarin
{
    /// <summary>
    /// Various utilities that should be provided via System.IO.Path.
    /// </summary>
    public static class PathHelpers
    {
        [DllImport ("libc")]
        static extern string realpath (string path, IntPtr resolvedName);

        static volatile bool haveRealpath = Environment.OSVersion.Platform == PlatformID.Unix;

        /// <summary>
        /// A more exhaustive version of <see cref="System.IO.Path.GetFullPath(string)"/>
        /// that additionally resolves symlinks via `realpath` on Unix systems. The path
        /// returned is also trimmed of any trailing directory separator characters.
        /// </summary>
        /// <param name="pathComponents">
        /// The path components to resolve to a full path. If multiple path compnents are
        /// specified, they are combined with <see cref="Path.Combine"/> before resolving.
        /// </param>
        /// <returns>
        /// Returns `null` if <paramref name="path"/> is `null` or an empty,
        /// string, otherwise the fully resolved path.
        /// </returns>
        public static string ResolveFullPath (params string [] pathComponents)
        {
            string path;

            if (pathComponents == null || pathComponents.Length == 0)
                return null;
            else if (pathComponents.Length == 1)
                path = pathComponents [0];
            else
                path = Path.Combine (pathComponents);

            if (string.IsNullOrEmpty (path))
                return null;

            var fullPath = Path.GetFullPath (NormalizePath (path));

            if (haveRealpath) {
                try {
                    // Path.GetFullPath expands the path, but on Unix systems
                    // does not resolve symlinks. Always attempt to resolve
                    // symlinks via realpath.
                    var realPath = realpath (fullPath, IntPtr.Zero);
                    if (!string.IsNullOrEmpty (realPath))
                        fullPath = realPath;
                } catch {
                    haveRealpath = false;
                }
            }

            if (fullPath.Length == 0)
                return null;

            if (fullPath [fullPath.Length - 1] == Path.DirectorySeparatorChar)
                return fullPath.TrimEnd (Path.DirectorySeparatorChar);

            if (fullPath [fullPath.Length - 1] == Path.AltDirectorySeparatorChar)
                return fullPath.TrimEnd (Path.AltDirectorySeparatorChar);

            return fullPath;
        }

        /// <summary>
        /// Returns a path with / and \ directory separators
        /// normalized to <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        public static string NormalizePath (string path)
            => path
                ?.Replace ('\\', Path.DirectorySeparatorChar)
                ?.Replace ('/', Path.DirectorySeparatorChar);

        /// <summary>
        /// Makes <paramref name="path"/> relative to <paramref name="basePath"/>.
        /// </summary>
        public static string MakeRelativePath (string basePath, string path)
        {
            // Adapted from MSBuild's FileUtilities.MakeRelative aka $([MSBuild]::MakeRelative(basePath, path))
            // https://github.com/Microsoft/msbuild/blob/master/src/Deprecated/Engine/Shared/FileUtilities.cs

            if (basePath == null)
                throw new ArgumentNullException (nameof (basePath));

            if (path == null)
                throw new ArgumentNullException (nameof (path));

            if (path.Length == 0)
                throw new ArgumentException ("must not be an empty string", nameof (path));

            basePath = ResolveFullPath (basePath);
            path = ResolveFullPath (path);

            // Ensure trailing slash for non-empty strings
            if (basePath.Length > 0 && basePath [basePath.Length - 1] != Path.DirectorySeparatorChar)
                basePath += Path.DirectorySeparatorChar;

            var baseUri = new Uri (basePath, UriKind.Absolute); // May throw UriFormatException

            // Try absolute first, then fall back on relative, otherwise it
            // makes some absolute UNC paths like (\\foo\bar) relative ...
            if (!Uri.TryCreate (path, UriKind.Absolute, out var pathUri))
                pathUri = new Uri (path, UriKind.Relative);

            if (!pathUri.IsAbsoluteUri)
                // The path is already a relative url, we will just normalize it...
                pathUri = new Uri (baseUri, pathUri);

            var relativeUri = baseUri.MakeRelativeUri (pathUri);
            var relativePath = Uri.UnescapeDataString (relativeUri.IsAbsoluteUri
                ? relativeUri.LocalPath
                : relativeUri.ToString ());

            return NormalizePath (relativePath);
        }

        /// <summary>
        /// Locates the path of a program in the system, first searching in <paramref name="preferPaths"/>
        /// if specified, then falling back to the paths in `PATH` environment variable. The first search
        /// path to yield a match for <paramref name="programName"/> will be used to resolve the absolute
        /// path of the program.
        /// </summary>
        /// <remarks>
        /// The functionality is analogous to the `which` command on Unix systems, and works on Windows
        /// as well. On Windows, `PATHEXT` is also respected. On Unix, files ending with `.exe` will
        /// also be returned if found - that is, one should not specify an extension at all in
        /// <paramref name="programName"/>. Finally, <paramref name="programName"/> comparison is _case
        /// insensitive_! For example, `msbuild` may yield `/path/to/MSBuild.exe`.
        /// </remarks>
        /// <param name="programName">
        /// The name of the program to find. Do not specify a file extension. See remarks.
        /// </param>
        /// <param name="preferPaths">
        /// Paths to search in preference over the `PATH` environment variable.
        /// </param>
        public static string FindProgramPath (
            string programName,
            IEnumerable<string> preferPaths = null)
        {
            var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);

            var extensions = new List<string> (Environment
                .GetEnvironmentVariable ("PATHEXT")
                ?.Split (new [] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string> ());

            if (extensions.Count == 0)
                extensions.Add (".exe");

            extensions.Insert (0, null);

            var preferPathsArray = preferPaths?.ToArray () ?? Array.Empty<string> ();

            var searchPaths = preferPathsArray.Concat (Environment
                .GetEnvironmentVariable ("PATH")
                .Split (isWindows ? ';' : ':'));

            var filesToCheck = searchPaths
                .Where (Directory.Exists)
                .Select (p => new DirectoryInfo (p))
                .SelectMany (p => p.EnumerateFiles ());

            foreach (var file in filesToCheck) {
                foreach (var extension in extensions) {
                    if (string.Equals (file.Name, programName + extension, StringComparison.OrdinalIgnoreCase))
                        return ResolveFullPath (file.FullName);
                }
            }

            string flattenedPreferPaths = null;
            var message = "Unable to find '{0}' in ";
            if (preferPathsArray.Length > 0) {
                message += "specified PreferPaths ({1}) nor PATH";
                flattenedPreferPaths = string.Join (", ", preferPathsArray.Select (p => $"'{p}'"));
            } else {
                message += "PATH";
            }

            throw new FileNotFoundException (message);
        }

        /// <summary>
        /// Helper functions for directory structures within a Git repository.
        /// </summary>
        public static class Git
        {
            /// <summary>
            /// Finds the root directory of a git repository from an assembly.
            /// If no assembly is provided, the calling assembly is implied.
            /// </summary>
            /// <remarks>
            /// Throws <see cref="DirectoryNotFoundException"/> if the root repository directory cannot be found.
            /// </remarks>
            public static string FindRepositoryRootPathFromAssembly (Assembly assemblyInRepository = null)
                => FindRepositoryRootPath ((assemblyInRepository ?? Assembly.GetCallingAssembly ()).Location);

            /// <summary>
            /// Finds the root directory of a git repository from a child path within.
            /// </summary>
            /// <remarks>
            /// Throws <see cref="DirectoryNotFoundException"/> if the root repository directory cannot be found.
            /// </remarks>
            public static string FindRepositoryRootPath (string childPathInRepository)
            {
                var path = childPathInRepository;

                while (File.Exists (path) || Directory.Exists (path)) {
                    if (Directory.Exists (Path.Combine (path, ".git")))
                        return ResolveFullPath (path);

                    path = Path.GetDirectoryName (path);
                }

                throw new DirectoryNotFoundException ($"{childPathInRepository} is not in a git repository");
            }
        }
    }
}