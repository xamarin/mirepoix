// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.MSBuild.Tooling.Solution
{
    public struct SolutionConfigurationPlatformMap : IEquatable<SolutionConfigurationPlatformMap>
    {
        public ConfigurationPlatform Solution { get; }
        public ConfigurationPlatform Project { get; }

        public bool BuildEnabled { get; }

        public SolutionConfigurationPlatformMap (
            ConfigurationPlatform solutionConfiguration,
            ConfigurationPlatform projectConfiguration,
            bool buildEnabled = true)
        {
            Solution = solutionConfiguration;
            Project = projectConfiguration;
            BuildEnabled = buildEnabled;
        }

        public static bool operator == (SolutionConfigurationPlatformMap lhs, SolutionConfigurationPlatformMap rhs)
            => lhs.Equals (rhs);

        public static bool operator != (SolutionConfigurationPlatformMap lhs, SolutionConfigurationPlatformMap rhs)
            => !lhs.Equals (rhs);

        public bool Equals (SolutionConfigurationPlatformMap other)
            => Solution == other.Solution && Project == other.Project;

        public override bool Equals (object obj)
            => obj is ConfigurationPlatform other && Equals (other);

        public override int GetHashCode ()
            => HashHelpers.Hash (
                Solution.GetHashCode (),
                Project.GetHashCode ());

        public override string ToString ()
            => $"{Solution} = {Project}";
    }
}