// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

using Xunit;

using Xamarin.ProcessControl;

namespace Xamarin.ProcessControl.Tests
{
    public class ExecTests
    {
        [UnixFact]
        public void ImplicitlyExecAssemblyWithMonoOnUnix ()
        {
            var exec = new Exec (ProcessArguments.Create ("test.exe", "a", "b"));
            Assert.False (exec.Elevated);
            Assert.Equal (
                exec.Arguments,
                ProcessArguments.Create ("mono", "test.exe", "a", "b"));
        }

        [MacFact]
        public void ElevateOnMac ()
        {
            var exec = new Exec (
                ProcessArguments.Create ("installer"),
                ExecFlags.Elevate);
            Assert.True (exec.Elevated);
            Assert.Equal (
                exec.Arguments,
                ProcessArguments.Create ("/usr/bin/sudo", "installer"));
        }

        [LinuxFact]
        public void IgnoreElevateOnUnix ()
        {
            // On VSTS the hosted Linux pool runs as root (sigh, but we can take
            // advantage of that fact to test that we are _not_ wrapping with
            // sudo if we already are root).
            if (Environment.GetEnvironmentVariable ("TF_BUILD") == "True") {
                var exec = new Exec (
                    ProcessArguments.Create ("installer"),
                    ExecFlags.Elevate);
                Assert.False (exec.Elevated);
                Assert.Equal (
                    exec.Arguments,
                    ProcessArguments.Create ("installer"));
            }
        }

        [MacFact]
        public void ElevateOnMacAndImplicitlyExecAssemblyWithMonoOnUnix ()
        {
            var exec = new Exec (
                ProcessArguments.Create ("test.exe", "a", "b"),
                ExecFlags.Elevate);
            Assert.True (exec.Elevated);
            Assert.Equal (
                exec.Arguments,
                ProcessArguments.Create ("/usr/bin/sudo", "mono", "test.exe", "a", "b"));
        }
    }
}