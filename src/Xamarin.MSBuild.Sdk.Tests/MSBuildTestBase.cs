// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.MSBuild.Sdk.Tests
{
    public abstract class MSBuildTestBase
    {
        static MSBuildTestBase ()
            => MSBuildLocator.RegisterMSBuildPath ();
    }
}