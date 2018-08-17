//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;

namespace Xamarin.Security.Keychains
{
    /// <summary>
    /// An abstract base class <see cref="IKeychain"/> implementation for keychains
    /// that store items as files in a directory structure, like <see cref="DPAPIKeychain"/>
    /// and <see cref="ManagedProtectionKeychain"/>.
    /// </summary>
    [EditorBrowsable (EditorBrowsableState.Advanced)]
    public abstract class FileSystemKeychain : IKeychain
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
                Unprotect (File.ReadAllBytes (secretPath)));

            return true;
        }

        public void StoreSecret (KeychainSecret secret, bool updateExisting = true)
        {
            var secretPath = GetSecretPath (secret.Name);
            if (!updateExisting && File.Exists (secretPath))
                throw new KeychainItemAlreadyExistsException (
                    $"'{secret.Name}' already exists");

            Directory.CreateDirectory (Path.GetDirectoryName (secretPath));

            File.WriteAllBytes (secretPath, Protect ((byte [])secret.Value));
        }

        protected abstract byte [] Protect (byte [] data);

        protected abstract byte [] Unprotect (byte [] data);
    }
}