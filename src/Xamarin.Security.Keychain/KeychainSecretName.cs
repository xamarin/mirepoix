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
    public struct KeychainSecretName : IEquatable<KeychainSecretName>
    {
        public string Service { get; }
        public string Account { get; }

        public KeychainSecretName (
            string service,
            string account)
        {
            ArgumentAssert.IsNotNullOrEmpty (service, nameof (service));
            ArgumentAssert.IsNotNullOrEmpty (account, nameof (account));

            Service = service;
            Account = account;
        }

        public bool Equals (KeychainSecretName other)
            => Service == other.Service && Account == other.Account;

        public override bool Equals (object obj)
            => obj is KeychainSecretName other && Equals (other);

        public override int GetHashCode ()
            => Service.GetHashCode () * unchecked((int)0xa5555529) + Account.GetHashCode ();

        public override string ToString ()
            => $"{Service}/{Account}";

        public void Deconstruct (out string service, out string account)
        {
            service = Service;
            account = Account;
        }

        public static implicit operator KeychainSecretName ((string service, string account) name)
            => new KeychainSecretName (name.service, name.account);

        public static bool operator == (KeychainSecretName a, KeychainSecretName b)
            => a.Equals (b);

        public static bool operator != (KeychainSecretName a, KeychainSecretName b)
            => !a.Equals (b);

        internal void Assert (string parameterName)
        {
            if (this == default)
                throw new ArgumentException ("must not be an empty name", parameterName);
        }
    }
}