// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.MSBuild.Sdk
{
    public interface IDependencyNode
    {
        string Id { get; }
        string Label { get; }
        Exception LoadException { get; }
    }
}