// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.MSBuild.Tooling.Tests
{
    public abstract class MSBuildTestBase
    {
        static MSBuildTestBase ()
            => MSBuildLocator.RegisterMSBuildPath ();
    }
}