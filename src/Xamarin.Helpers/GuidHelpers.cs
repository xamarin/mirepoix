// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

namespace Xamarin
{
    public static class GuidHelpers
    {
        /// <summary>
        /// Predefined namespaces for Versions 3 and 5 GUIDs from [RFC 4122 Appendix C](https://tools.ietf.org/html/rfc4122#appendix-C).
        /// </summary>
        public static class GuidNamespace
        {
            /// <summary>Name string is a fully-qualified domain name.</summary>
            public static Guid DNS { get; } = new Guid (
                // 6ba7b810-9dad-11d1-80b4-00c04fd430c8
                0x6ba7b810,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8);

            /// <summary>Name string is a URL</summary>
            public static Guid URL { get; } = new Guid (
                // 6ba7b811-9dad-11d1-80b4-00c04fd430c8
                0x6ba7b811,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8);

            /// <summary>Name string is an ISO OID</summary>
            public static Guid OID { get; } = new Guid (
                // 6ba7b812-9dad-11d1-80b4-00c04fd430c8
                0x6ba7b812,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8);

            /// <summary> Name string is an X.500 DN (in DER or a text output format)</summary>
            public static Guid X500 { get; } = new Guid (
                // 6ba7b814-9dad-11d1-80b4-00c04fd430c8
                0x6ba7b814,
                0x9dad,
                0x11d1,
                0x80, 0xb4, 0x00, 0xc0, 0x4f, 0xd4, 0x30, 0xc8);
        }

        /// <summary>
        /// Creates a version [5 GUID/UUID](https://en.wikipedia.org/wiki/Universally_unique_identifier#Versions_3_and_5_(namespace_name-based))
        /// by combining a given <paramref name="namespaceGuid"/> and arbitrary <paramref name="name"/>, hashed together using SHA-1.
        /// See <see cref="GuidNamespace"/> for pre-defined namespaces.
        /// </summary>
        public static Guid GuidV5 (Guid namespaceGuid, string name)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            using (var sha1 = SHA1.Create ())
                return CreateHashedGuid (sha1, 0x50, namespaceGuid, name);
        }

        /// <summary>
        /// Creates a version [3 GUID/UUID](https://en.wikipedia.org/wiki/Universally_unique_identifier#Versions_3_and_5_(namespace_name-based))
        /// by combining a given <paramref name="namespaceGuid"/> and arbitrary <paramref name="name"/>, hashed together using MD5.
        /// See <see cref="GuidNamespace"/> for pre-defined namespaces.
        /// </summary>
        public static Guid GuidV3 (Guid namespaceGuid, string name)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            using (var md5 = MD5.Create ())
                return CreateHashedGuid (md5, 0x30, namespaceGuid, name);
        }

        static unsafe Guid CreateHashedGuid (
            HashAlgorithm hashAlgorithm,
            byte version,
            Guid namespaceGuid,
            string name)
        {
            var namespaceBytes = namespaceGuid.ToByteArray ();
            var nameBytes = Encoding.UTF8.GetBytes (name);
            Swap (namespaceBytes);

            hashAlgorithm.TransformBlock (namespaceBytes, 0, namespaceBytes.Length, null, 0);
            hashAlgorithm.TransformFinalBlock (nameBytes, 0, nameBytes.Length);

            var guid = hashAlgorithm.Hash;
            Array.Resize (ref guid, 16);

            guid [6] = (byte)((guid [6] & 0x0F) | version);
            guid [8] = (byte)((guid [8] & 0x3F) | 0x80);

            Swap (guid);
            return new Guid (guid);

            void Swap (byte [] g)
            {
                void SwapAt (int left, int right)
                {
                    var t = g [left];
                    g [left] = g [right];
                    g [right] = t;
                }

                SwapAt (0, 3);
                SwapAt (1, 2);
                SwapAt (4, 5);
                SwapAt (6, 7);
            }
        }
    }
}