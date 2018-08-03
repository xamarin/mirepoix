//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Xamarin.Security.Keychains
{
    [EditorBrowsable (EditorBrowsableState.Never)]
    public sealed class OSKeychain : IKeychain
    {
        readonly IKeychain keychain;

        public OSKeychain ()
        {
            if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
                keychain = new AppleKeychain ();
            else
                keychain = new DPAPIKeychain ();
        }

        public bool TryGetSecret (KeychainSecretName name, out KeychainSecret secret)
            => keychain.TryGetSecret (name, out secret);

        public void StoreSecret (KeychainSecret secret, bool updateExisting = true)
            => keychain.StoreSecret (secret, updateExisting);
    }
}