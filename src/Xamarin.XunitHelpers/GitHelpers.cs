// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace Xunit
{
    public static class GitHelpers
    {
        public static string GetPathToRepoRoot (Assembly assembly = null)
        {
            var assemblyPath = (assembly ?? Assembly.GetCallingAssembly ()).Location;
            var path = assemblyPath;

            while (File.Exists (path) || Directory.Exists (path)) {
                if (Directory.Exists (Path.Combine (path, ".git")))
                    return path;

                path = Path.GetDirectoryName (path);
            }

            throw new Exception ($"{assemblyPath} is not in a git repository");
        }
    }
}