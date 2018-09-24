// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Mono.Options;

namespace Xamarin.MSBuild.Tool
{
    sealed class HelpOptionSet : OptionSet
    {
        public HelpOptionSet (params string [] usageLines)
        {
            foreach (var usageLine in usageLines)
                Add (usageLine);

            Add ("");
            Add ("Options:");
            Add ("");
            Add ("h|?|help", "Show this help", v => {
                WriteOptionDescriptions (Console.Out);
                Environment.Exit (1);
            });
        }
    }
}