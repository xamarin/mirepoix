//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Xamarin.Security.Keychains
{
    /// <summary>
    /// An <see cref="IKeychain"/> implementation backed by the
    /// [Windows Data Protection](https://msdn.microsoft.com/en-us/library/ms995355.aspx)
    /// API for secure storage of secrets, scoped to the current user.
    /// </summary>
    [EditorBrowsable (EditorBrowsableState.Advanced)]
    public sealed class DPAPIKeychain : FileSystemKeychain
    {
        protected override byte [] Protect (byte [] data)
            => ProtectedData.Protect (
                data,
                null,
                DataProtectionScope.CurrentUser);

        protected override byte [] Unprotect (byte [] data)
            => ProtectedData.Unprotect (
                data,
                null,
                DataProtectionScope.CurrentUser);
    }
}