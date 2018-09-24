// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Mono.Options;

namespace Xamarin.MSBuild.Tool
{
    static class Program
    {
        public static readonly string Name = typeof (Program).Assembly.GetName ().Name;

        public static string MSBuildExePath { get; private set; }

        static int Main (string [] args)
            => new CommandSet(Name) {
                { $"Usage: {Name} [GLOBAL_OPTIONS+] COMMAND [OPTIONS+]" },
                { "" },
                { "Global Options:" },
                { "" },
                { "msbuild-path=", "Use a specific MSBuild.exe", v => MSBuildExePath = v },
                { "" },
                { "Available Commands:" },
                { "" },
                new GenerateSolutionCommand ()
            }.Run (args.Length == 0 ? new [] { "help" } : args);
    }
}