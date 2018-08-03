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
    public class KeychainSecretTests
    {
        [Fact]
        public void Create ()
        {
            var secret = KeychainSecret.Create (("a", "b"), "c");
            Assert.Equal (("a", "b"), secret.Name);
            Assert.Equal ("a", secret.Name.Service);
            Assert.Equal ("b", secret.Name.Account);
            Assert.Equal (new byte [] { (byte)'c' }, secret.Value);
            Assert.Equal ("c", secret.GetUtf8StringValue ());
        }

        [Fact]
        public void WithName ()
            => Assert.Equal (
                ("d", "e"),
                KeychainSecret
                    .Create (("a", "b"), "c")
                    .WithName (("d", "e")).Name);

        [Fact]
        public void WithValue ()
        {
            Assert.Equal (
                "d",
                KeychainSecret
                    .Create (("a", "b"), "c")
                    .WithValue ("d").GetUtf8StringValue ());

            Assert.Equal (
                new byte [] { 240, 159, 165, 145, 240, 159, 140, 174, 240, 159, 141, 148, 240, 159, 144, 136 },
                KeychainSecret
                    .Create (("a", "b"), "c")
                    .WithValue ("🥑🌮🍔🐈")
                    .Value);
        }

        [Fact]
        public void Create_Name_NullOrEmptyNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret.Create ((null, null), Array.Empty<byte> ()));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret.Create (("a", null), Array.Empty<byte> ()));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret.Create ((null, "b"), Array.Empty<byte> ()));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret.Create ((string.Empty, string.Empty), Array.Empty<byte> ()));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret.Create (("a", string.Empty), Array.Empty<byte> ()));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret.Create ((string.Empty, "b"), Array.Empty<byte> ()));
        }

        [Fact]
        public void Create_Value_NullNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret.Create (("a", "b"), null as string));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret.Create (("a", "b"), null as byte []));
        }

        [Fact]
        public void WithName_NullOrEmptyNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName ((null, null)));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName (("a", null)));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName ((null, "a")));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName ((string.Empty, string.Empty)));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName (("a", string.Empty)));

            Assert.Throws<ArgumentException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithName ((string.Empty, "a")));
        }

        [Fact]
        public void WithValue_NullNotAllowed ()
        {
            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithValue (null as string));

            Assert.Throws<ArgumentNullException> (
                () => KeychainSecret
                    .Create (("a", "b"), Array.Empty<byte> ())
                    .WithValue (null as byte []));
        }
    }
}