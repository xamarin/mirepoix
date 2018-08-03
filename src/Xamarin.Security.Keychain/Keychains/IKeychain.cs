//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Xamarin.Security.Keychains
{
    [EditorBrowsable (EditorBrowsableState.Advanced)]
    public interface IKeychain
    {
        bool TryGetSecret (KeychainSecretName name, out KeychainSecret secret);

        void StoreSecret (KeychainSecret secret, bool updateExisting = true);
    }
}