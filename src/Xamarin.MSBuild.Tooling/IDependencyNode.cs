// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Xamarin.MSBuild.Tooling
{
    public interface IDependencyNode
    {
        string Id { get; }
        string Label { get; }
    }
}