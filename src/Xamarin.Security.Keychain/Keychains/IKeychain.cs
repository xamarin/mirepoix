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

    [EditorBrowsable (EditorBrowsableState.Advanced)]
    public static class IKeychainExtensions
    {
        public static bool TryGetSecret (
            this IKeychain keychain,
            string serviceName,
            string accountName,
            out string secret)
        {
            if (keychain.TryGetSecret (((serviceName, accountName)), out var fullSecret)) {
                secret = fullSecret.GetUtf8StringValue ();
                return true;
            }

            secret = null;
            return false;
        }

        public static bool TryGetSecretBytes (
            this IKeychain keychain,
            string serviceName,
            string accountName,
            out byte [] secret)
        {
            if (keychain.TryGetSecret (((serviceName, accountName)), out var fullSecret)) {
                secret = (byte [])fullSecret.Value;
                return true;
            }

            secret = null;
            return false;
        }

        public static void StoreSecret (
            this IKeychain keychain,
            string serviceName,
            string accountName,
            string secret,
            bool updateExisting = true)
            => keychain.StoreSecret (
                KeychainSecret.Create (
                    (serviceName, accountName),
                    secret),
                updateExisting);

        public static void StoreSecret (
            this IKeychain keychain,
            string serviceName,
            string accountName,
            byte [] secret,
            bool updateExisting = true)
            => keychain.StoreSecret (
                KeychainSecret.Create (
                    (serviceName, accountName),
                    secret),
                updateExisting);
    }
}