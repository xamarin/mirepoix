//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

namespace Xamarin.Security.Keychains
{
    [EditorBrowsable (EditorBrowsableState.Never)]
    public sealed class DPAPIKeychain : IKeychain
    {
        static string GetSecretPath (KeychainSecretName secretName)
            => Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
                "DPAPIKeychain",
                secretName.Service,
                secretName.Account);

        public bool TryGetSecret (KeychainSecretName name, out KeychainSecret secret)
        {
            var secretPath = GetSecretPath (name);
            if (!File.Exists (secretPath)) {
                secret = null;
                return false;
            }

            secret = KeychainSecret.Create (
                name,
                ProtectedData.Unprotect (
                    File.ReadAllBytes (secretPath),
                    null,
                    DataProtectionScope.CurrentUser));

            return true;
        }

        public void StoreSecret (KeychainSecret secret, bool updateExisting = true)
        {
            var secretPath = GetSecretPath (secret.Name);
            if (!updateExisting && File.Exists (secretPath))
                throw new KeychainItemAlreadyExistsException (
                    $"'{secret.Name}' already exists");

            Directory.CreateDirectory (Path.GetDirectoryName (secretPath));

            File.WriteAllBytes (secretPath, ProtectedData.Protect (
                (byte [])secret.Value,
                null,
                DataProtectionScope.CurrentUser));
        }
    }
}