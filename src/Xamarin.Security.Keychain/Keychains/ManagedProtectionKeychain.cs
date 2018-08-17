//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Xamarin.Security.Keychains
{
    /// <summary>
    /// An <see cref="IKeychain"/> implementation backed by Mono's DPAPI-compatible
    /// [`Mono.Security.Cryptography.ManagedProtection`](https://github.com/mono/mono/blob/master/mcs/class/System.Security/Mono.Security.Cryptography/ManagedProtection.cs).
    /// </summary>
    [EditorBrowsable (EditorBrowsableState.Advanced)]
    public sealed class ManagedProtectionKeychain : FileSystemKeychain
    {
        protected override byte [] Protect (byte [] data)
            => ManagedProtection.Protect (
                data,
                null,
                DataProtectionScope.CurrentUser);

        protected override byte [] Unprotect (byte [] data)
            => ManagedProtection.Unprotect (
                data,
                null,
                DataProtectionScope.CurrentUser);
    }
}