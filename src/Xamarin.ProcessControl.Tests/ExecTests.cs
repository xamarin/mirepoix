// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

        [UnixFact]
        public void ElevateOnUnix ()
        {
            var exec = new Exec (
                ProcessArguments.Create ("installer"),
                ExecFlags.Elevate);
            Assert.True (exec.Elevated);
            Assert.Equal (
                exec.Arguments,
                ProcessArguments.Create ("/usr/bin/sudo", "installer"));
        }

        [UnixFact]
        public void ElevateAndImplicitlyExecAssemblyWithMonoOnUnix ()
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