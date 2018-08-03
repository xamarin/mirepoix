//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Runtime.InteropServices;

namespace Xunit
{
    public class WindowsTheoryAttribute : OSTheoryAttribute
    {
        public WindowsTheoryAttribute () : base (OSPlatform.Windows)
        {
        }
    }

    public class LinuxTheoryAttribute : OSTheoryAttribute
    {
        public LinuxTheoryAttribute () : base (OSPlatform.Linux)
        {
        }
    }

    public class MacTheoryAttribute : OSTheoryAttribute
    {
        public MacTheoryAttribute () : base (OSPlatform.OSX)
        {
        }
    }

    public class UnixTheoryAttribute : OSTheoryAttribute
    {
        public UnixTheoryAttribute () : base (OSPlatform.Linux, OSPlatform.OSX)
        {
        }
    }

    public class OSTheoryAttribute : TheoryAttribute
    {
        readonly string osSkipString;

        string skipReason;
        public override string Skip {
            get {
                if (osSkipString == null)
                    return null;

                if (skipReason == null)
                    return osSkipString;

                return $"{osSkipString}: {skipReason}";
            }

            set => skipReason = value;
        }

        protected OSTheoryAttribute (params OSPlatform [] osPlatforms)
            : this (osPlatforms.Select (osPlatform => osPlatform.ToString ()).ToArray ())
        {
        }

        public OSTheoryAttribute (params string [] osNames)
        {
            foreach (var osName in osNames) {
                if (RuntimeInformation.IsOSPlatform (OSPlatform.Create (osName.ToUpperInvariant ())))
                    return;
            }

            osSkipString = $"Only available on {string.Join (", ", osNames)}";
        }
    }
}