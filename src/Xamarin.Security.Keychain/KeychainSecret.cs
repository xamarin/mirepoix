//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Security
{
    public sealed class KeychainSecret
    {
        public KeychainSecretName Name { get; }

        readonly byte [] value;
        public IReadOnlyList<byte> Value => value;

        KeychainSecret (
            KeychainSecretName name,
            byte [] value)
        {
            Name = name;
            this.value = value;
        }

        public string GetUtf8StringValue ()
            => Keychain.Utf8.GetString (value);

        public static KeychainSecret Create (
            KeychainSecretName name,
            byte [] value)
        {
            name.Assert (nameof (name));
            Assert.IsNotNull (value, nameof (value));

            return new KeychainSecret (
                name,
                value);
        }

        public static KeychainSecret Create (
            KeychainSecretName name,
            string value)
        {
            name.Assert (nameof (name));
            Assert.IsNotNull (value, nameof (value));

            return new KeychainSecret (
                name,
                value == null ? null : Keychain.Utf8.GetBytes (value));
        }

        public KeychainSecret WithName (KeychainSecretName name)
        {
            name.Assert (nameof (name));

            return new KeychainSecret (
                name,
                value);
        }

        public KeychainSecret WithValue (byte [] value)
        {
            Assert.IsNotNull (value, nameof (value));

            return new KeychainSecret (
                Name,
                value);
        }

        public KeychainSecret WithValue (string value)
        {
            Assert.IsNotNull (value, nameof (value));

            return new KeychainSecret (
                Name,
                Keychain.Utf8.GetBytes (value));
        }
    }
}