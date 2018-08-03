//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Text;

using Xamarin.Security.Keychains;

namespace Xamarin.Security
{
    public static class Keychain
    {
        internal static readonly Encoding Utf8 = new UTF8Encoding (false, false);

        static readonly IKeychain keychain = new OSKeychain ();

        static Keychain ()
        {
        }

        public static bool TryGetSecret (KeychainSecretName name, out KeychainSecret secret)
            => keychain.TryGetSecret (name, out secret);

        public static void StoreSecret (KeychainSecret secret, bool updateExisting = true)
            => keychain.StoreSecret (secret, updateExisting);
    }
}