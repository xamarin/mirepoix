// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Mono.Options;

namespace Xamarin.MSBuild.Tool
{
    abstract class FancyCommand : Command
    {
        protected FancyCommand (string name, string help = null) : base (name, help)
        {
        }

        protected static int Error (string message, int exitCode = 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine (message);
            Console.ResetColor ();
            return exitCode;
        }
    }
}