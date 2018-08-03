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
    // to accessing the same OS resources at the same time.

    [CollectionDefinition (nameof (KeychainImplementations))]
    public sealed class KeychainImplementations
    {
    }

    [Collection (nameof (KeychainImplementations))]
    public sealed class OSKeychainTests : KeychainTests<OSKeychain>
    {
    }

    public abstract class KeychainTests<TKeychain> where TKeychain : IKeychain, new ()
    {
        readonly TKeychain keychain = new TKeychain ();

        const string serviceName = "com.xamarin.mirepoix.tests";

        [Theory]
        [InlineData ("simple-string", "hello")]
        [InlineData ("🥑🌮🍔🐈", "🥑🌮🍔🐈")]
        public void RoundtripString (string key, string value)
        {
            KeychainSecretName name = (serviceName, key);
            keychain.StoreSecret (KeychainSecret.Create (name, value));
            Assert.True (keychain.TryGetSecret (name, out var secret));
            Assert.Equal (value, secret.GetUtf8StringValue ());
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void StoreSecret ()
        {
            KeychainSecretName name = (serviceName, "dont-update-me");
            keychain.StoreSecret (KeychainSecret.Create (name, "initial value"));
            Assert.ThrowsAny<Exception> (() => keychain.StoreSecret (
                KeychainSecret.Create (name, "new value"),
                updateExisting: false));
        }
    }
}