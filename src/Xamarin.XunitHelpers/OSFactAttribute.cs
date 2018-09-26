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
    public class WindowsFactAttribute : OSFactAttribute
    {
        public WindowsFactAttribute () : base (OSPlatform.Windows)
        {
        }
    }

    public class LinuxFactAttribute : OSFactAttribute
    {
        public LinuxFactAttribute () : base (OSPlatform.Linux)
        {
        }
    }

    public class MacFactAttribute : OSFactAttribute
    {
        public MacFactAttribute () : base (OSPlatform.OSX)
        {
        }
    }

    public class UnixFactAttribute : OSFactAttribute
    {
        public UnixFactAttribute () : base (OSPlatform.Linux, OSPlatform.OSX)
        {
        }
    }

    public class WindowsMacFactAttribute : OSFactAttribute
    {
        public WindowsMacFactAttribute () : base (OSPlatform.Windows, OSPlatform.OSX)
        {
        }
    }

    public class OSFactAttribute : FactAttribute
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

        protected OSFactAttribute (params OSPlatform [] osPlatforms)
            : this (osPlatforms.Select (osPlatform => osPlatform.ToString ()).ToArray ())
        {
        }

        public OSFactAttribute (params string [] osNames)
        {
            foreach (var osName in osNames) {
                if (RuntimeInformation.IsOSPlatform (OSPlatform.Create (osName.ToUpperInvariant ())))
                    return;
            }

            osSkipString = $"Only available on {string.Join (", ", osNames)}";
        }
    }
}