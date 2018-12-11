
//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using NuGet.Packaging;

namespace Xamarin.BuildConsolidator
{
    sealed class FrameworkAssemblyReferenceComparer : IEqualityComparer<FrameworkAssemblyReference>
    {
        public bool Equals (FrameworkAssemblyReference x, FrameworkAssemblyReference y)
        {
            if (!string.Equals (x.AssemblyName, y.AssemblyName, StringComparison.OrdinalIgnoreCase))
                return false;

            return x.SupportedFrameworks.SequenceEqual (y.SupportedFrameworks);
        }

        public int GetHashCode (FrameworkAssemblyReference obj)
            => 0;
    }
}