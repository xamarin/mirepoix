//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Xunit;

using Xamarin.Security.Keychains;

namespace Xamarin.Security.Tests
{
    // NOTE: CollectionDefinition/Collection needed to ensure xunit does
    // not run the keychain tests in parallel since OSKeychain will create
    // either DPAPIKeychain or AppleKeychain, which in turn can race due
    // to accessing the same OS resources at the same time if we explicitly
    // want to test, for example, KeychainTests<AppleKeychain>.

    [CollectionDefinition (nameof (KeychainImplementations))]
    public sealed class KeychainImplementations
    {
    }

    [Collection (nameof (KeychainImplementations))]
    public sealed class OSKeychainTests : IKeychainTests<OSKeychain>
    {
    }


    [Collection (nameof (KeychainImplementations))]
    public sealed class ManagedProtectionKeychainTests : IKeychainTests<OSKeychain>
    {
    }

    public abstract class IKeychainTests<TKeychain> where TKeychain : IKeychain, new ()
    {
        readonly TKeychain keychain = new TKeychain ();

        const string serviceName = "com.xamarin.mirepoix.tests";

        [WindowsMacTheory]
        [ClassData (typeof (GB18030TestData))]
        public void RoundtripString (string key, string value)
        {
            KeychainSecretName name = (serviceName, key);
            keychain.StoreSecret (KeychainSecret.Create (name, value));
            Assert.True (keychain.TryGetSecret (name, out var secret));
            Assert.Equal (value, secret.GetUtf8StringValue ());
        }

        [WindowsMacFact]
        public void RoundtripBytes ()
        {
            var random = new Random ();
            var value = new byte [1024 * 1024];
            random.NextBytes (value);
            KeychainSecretName name = (serviceName, "randomblob");
            keychain.StoreSecret (KeychainSecret.Create (name, value));
            Assert.True (keychain.TryGetSecret (name, out var secret));
            Assert.Equal (value, secret.Value);
        }

        [WindowsMacFact]
        public void UpdateSecret ()
        {
            var secret = KeychainSecret.Create (
                (serviceName, "update-existing"),
                "initial value");
            Assert.Equal ("initial value", secret.GetUtf8StringValue ());
            keychain.StoreSecret (secret);
            Assert.True (keychain.TryGetSecret (secret.Name, out var querySecret));
            Assert.NotSame (secret, querySecret);
            Assert.Equal ("initial value", querySecret.GetUtf8StringValue ());
            keychain.StoreSecret (querySecret.WithValue ("new value"));
            Assert.True (keychain.TryGetSecret (querySecret.Name, out var querySecret2));
            Assert.Equal ("new value", querySecret2.GetUtf8StringValue ());
        }

        [WindowsMacFact]
        public void StoreSecret ()
        {
            KeychainSecretName name = (serviceName, "dont-update-me");
            keychain.StoreSecret (KeychainSecret.Create (name, "initial value"));
            Assert.Throws<KeychainItemAlreadyExistsException> (() => keychain.StoreSecret (
                KeychainSecret.Create (name, "new value"),
                updateExisting: false));
        }
    }

    [Collection (nameof (KeychainImplementations))]
    public class KeychainTests
    {
        const string serviceName = "com.xamarin.mirepoix.tests";

        [WindowsMacFact]
        public void RoundtripString ()
        {
            const string key = "helper-api-roundtrip-string";
            var value = Guid.NewGuid ().ToString ();
            Keychain.StoreSecret (serviceName, key, value);
            Assert.True (Keychain.TryGetSecret (serviceName, key, out var secret));
            Assert.Equal (value, secret);
        }

        [WindowsMacFact]
        public void RoundtripBytes ()
        {
            const string key = "helper-api-roundtrip-bytes";
            var value = Guid.NewGuid ().ToByteArray ();
            Keychain.StoreSecret (serviceName, key, value);
            Assert.True (Keychain.TryGetSecretBytes (serviceName, key, out var secret));
            Assert.Equal (value, secret);
        }
    }
}