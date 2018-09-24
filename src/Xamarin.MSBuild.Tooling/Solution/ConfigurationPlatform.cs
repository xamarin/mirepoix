// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Xamarin.MSBuild.Tooling.Solution
{
    public struct ConfigurationPlatform : IEquatable<ConfigurationPlatform>
    {
        public const string DefaultConfiguration = "Debug";
        public const string DefaultPlatform = "AnyCPU";

        public string Configuration { get; }
        public string Platform { get; }

        public ConfigurationPlatform (
            string configuration = null,
            string platform = null)
        {
            configuration = configuration?.Trim ();
            Configuration = string.IsNullOrEmpty (configuration)
                ? DefaultConfiguration
                : configuration;

            platform = platform?.Trim ();
            Platform = string.IsNullOrEmpty (platform) ||
                string.Equals (platform, "Any CPU", StringComparison.OrdinalIgnoreCase)
                ? DefaultPlatform
                : platform;
        }

        public static bool operator == (ConfigurationPlatform lhs, ConfigurationPlatform rhs)
            => lhs.Equals (rhs);

        public static bool operator != (ConfigurationPlatform lhs, ConfigurationPlatform rhs)
            => !lhs.Equals (rhs);

        public bool Equals (ConfigurationPlatform other)
            => string.Equals (Configuration, other.Configuration, StringComparison.OrdinalIgnoreCase) &&
                string.Equals (Platform, other.Platform, StringComparison.OrdinalIgnoreCase);

        public override bool Equals (object obj)
            => obj is ConfigurationPlatform other && Equals (other);

        public override int GetHashCode ()
            => HashHelpers.Hash (Configuration, Platform);

        public override string ToString ()
            => $"{Configuration}|{Platform}";

        public string ToSolutionString ()
        {
            var platform = Platform;
            if (string.Equals (platform, DefaultPlatform, StringComparison.OrdinalIgnoreCase))
                platform = "Any CPU";
            return $"{Configuration}|{platform}";
        }

        public static ConfigurationPlatform Parse (string spec)
        {
            if (string.IsNullOrEmpty (spec))
                return new ConfigurationPlatform ();

            var parts = spec.Trim ('\'').Split ('|');

            return new ConfigurationPlatform (
                parts.ElementAtOrDefault (0),
                parts.ElementAtOrDefault (1));
        }
    }
}