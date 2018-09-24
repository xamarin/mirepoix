// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Evaluation;

namespace Xamarin.MSBuild.Tooling.Solution
{
    sealed class SolutionNode
    {
        static readonly Guid solutionFolderTypeGuid = new Guid ("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
        static readonly Guid csprojTypeGuid = new Guid ("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        static readonly Guid fsprojTypeGuid = new Guid ("{F2A71F9B-5D33-465A-A702-920D77279786}");
        static readonly Guid vbprojTypeGuid = new Guid ("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
        static readonly Guid shprojTypeGuid = new Guid ("{D954291E-2A0B-460D-934E-DC6B0785DB48}");

        public SolutionNode Top { get; }
        public SolutionNode Parent { get; }
        public Guid Guid { get; }
        public Guid TypeGuid { get; }
        public string Name { get; }
        public string RelativePath { get; }

        readonly List<SolutionConfigurationPlatformMap> configurations = new List<SolutionConfigurationPlatformMap> ();
        public IReadOnlyList<SolutionConfigurationPlatformMap> Configurations => configurations;

        readonly List<SolutionNode> children = new List<SolutionNode> ();
        public IReadOnlyList<SolutionNode> Children => children;

        public SolutionNode ()
            => Top = this;

        SolutionNode (
            SolutionNode top,
            SolutionNode parent,
            string folderName)
        {
            Top = top ?? throw new ArgumentNullException (nameof (top));
            Parent = parent ?? throw new ArgumentNullException (nameof (parent));
            Name = folderName ?? throw new ArgumentNullException (nameof (folderName));
            RelativePath = folderName;
            Guid = GuidHelpers.GuidV5 (parent.Guid, folderName);
            TypeGuid = solutionFolderTypeGuid;
        }

        SolutionNode (
            SolutionNode top,
            SolutionNode parent,
            Guid projectGuid,
            string relativePath)
        {
            Top = top ?? throw new ArgumentNullException (nameof (top));
            Parent = parent ?? throw new ArgumentNullException (nameof (parent));
            Guid = projectGuid;
            Name = Path.GetFileNameWithoutExtension (relativePath);
            RelativePath = relativePath ?? throw new ArgumentNullException (nameof (relativePath));

            var extension = Path.GetExtension (relativePath).ToLowerInvariant ();
            switch (extension) {
            case ".csproj":
                TypeGuid = csprojTypeGuid;
                break;
            case ".fsproj":
                TypeGuid = fsprojTypeGuid;
                break;
            case ".vbproj":
                TypeGuid = vbprojTypeGuid;
                break;
            case ".shproj":
                TypeGuid = shprojTypeGuid;
                break;
            default:
                throw new NotSupportedException ($"'{extension}' extension is not supported");
            }
        }

        /// <summary>
        /// Adds a new solution folder child node. If a folder of the same name has
        /// already been added it is returned instead.
        /// </summary>
        public SolutionNode AddFolder (string folderName)
        {
            if (folderName == null)
                throw new ArgumentNullException (nameof (folderName));

            var child = children.Find (c =>
                c.TypeGuid == solutionFolderTypeGuid &&
                string.Equals (c.Name, folderName, StringComparison.Ordinal));

            if (child == null)
                children.Add (child = new SolutionNode (Top, this, folderName));

            return child;
        }

        /// <summary>
        /// Adds a project child node. If a project at the same <paramref name="relativePath"/>
        /// has already been added it is returned instead.
        /// </summary>
        public SolutionNode AddProject (Guid projectGuid, string relativePath)
        {
            if (relativePath == null)
                throw new ArgumentNullException (nameof (relativePath));

            var child = children.Find (c => c.RelativePath == relativePath);

            if (child == null)
                children.Add (child = new SolutionNode (
                    Top,
                    this,
                    projectGuid,
                    relativePath));

            return child;
        }

        public void AddConfigurationMap (SolutionConfigurationPlatformMap configurationMap)
        {
            if (!configurations.Contains (configurationMap))
                configurations.Add (configurationMap);
        }
    }
}