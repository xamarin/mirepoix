//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Xunit;

namespace Xamarin.Security.Tests
{
    public class KeychainSecretNameTests
    {
        [Fact]
        public void ServiceNullNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => new KeychainSecretName (null, "account"));
            Assert.Throws<ArgumentNullException> (
                () => (KeychainSecretName)(null, "account"));
        }

        [Fact]
        public void ServiceEmptyNotAllowed ()
        {
            Assert.Throws<ArgumentException> (
                () => new KeychainSecretName (string.Empty, "account"));
            Assert.Throws<ArgumentException> (
                () => (KeychainSecretName)(string.Empty, "account"));
        }

        [Fact]
        public void AccountNullNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => new KeychainSecretName ("service", null));
            Assert.Throws<ArgumentException> (
                () => (KeychainSecretName)(string.Empty, "account"));
        }

        [Fact]
        public void AccountEmptyNotAllowed ()
        {
            Assert.Throws<ArgumentException> (
                () => new KeychainSecretName ("service", string.Empty));
            Assert.Throws<ArgumentException> (
                () => (KeychainSecretName)(string.Empty, "account"));
        }

        [Fact]
        public void Equal ()
        {
            Assert.Equal (new KeychainSecretName ("a", "b"), ("a", "b"));
            Assert.True (new KeychainSecretName ("a", "b") == ("a", "b"));
            Assert.False (new KeychainSecretName ("a", "b") != ("a", "b"));
        }

        [Fact]
        public void NotEqual ()
        {
            Assert.NotEqual (new KeychainSecretName ("a", "b"), ("a", "c"));
            Assert.True (new KeychainSecretName ("a", "b") != ("a", "c"));
            Assert.False (new KeychainSecretName ("a", "b") == ("a", "c"));
        }

        [Fact]
        public void ToStringFormat ()
            => Assert.Equal ("a/b", new KeychainSecretName ("a", "b").ToString ());

        [Fact]
        public void Deconstruct ()
        {
            var (service, account) = new KeychainSecretName ("a", "b");
            Assert.Equal ("a", service);
            Assert.Equal ("b", account);
        }
    }
}