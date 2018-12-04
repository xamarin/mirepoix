// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Xamarin.Linq
{
    public sealed class LinqExtensionsTests
    {
        static readonly Random random = new Random ();

        // NOTE: intentionally not using [MemberData]/[Theory] for these
        // tests since doing so reports thousands and thousands of discrete
        // tests which is misleading.
        static IEnumerable<byte []> GetByteArrayData ()
        {
            for (int length = 0; length <= 0x8000; length++) {
                var data = new byte [length];
                random.NextBytes (data);
                yield return data;
            }
        }

        [Fact]
        public void SequenceEqual ()
        {
            foreach (var data in GetByteArrayData ()) {
                var dataCopy = new byte [data.Length];
                Array.Copy (data, dataCopy, data.Length);

                Assert.True (data.SequenceEqual (dataCopy));
            }
        }

        [Fact]
        public void SequenceNotEqual ()
        {
            foreach (var data in GetByteArrayData ()) {
                if (data.Length == 0)
                    continue;

                var dataCopy = new byte [data.Length];
                dataCopy [random.Next (0, dataCopy.Length)]++;

                Assert.False (data.SequenceEqual (dataCopy));
            }
        }
    }
}