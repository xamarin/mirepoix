//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Xunit;

using static Xamarin.NativeHelpers.CoreFoundation;

namespace Xamarin.NativeHelpers
{
    public class CoreFoundationTests
    {
        [MacTheory]
        [ClassData (typeof (GB18030TestDataWithNullAndEmpty))]
        public void ToCFString (string stringDescription, string stringValue)
        {
            if (stringValue == null)
                return;

            using (var cfString = new CFString (stringValue)) {
                var roundTripStringValue = cfString.ToString ();
                Assert.Equal (stringValue, roundTripStringValue);
            }
        }

        #pragma warning disable xUnit1025

        [MacTheory]
        [InlineData (sbyte.MinValue)]
        [InlineData (default (sbyte))]
        [InlineData (sbyte.MaxValue)]
        public void CFNumberRoundTrip_SByte (sbyte number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToSByte ());
        }

        [MacTheory]
        [InlineData (byte.MinValue)]
        [InlineData (default (byte))]
        [InlineData (byte.MaxValue)]
        public void CFNumberRoundTrip_Byte (byte number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToByte ());
        }

        [MacTheory]
        [InlineData (short.MinValue)]
        [InlineData (default (short))]
        [InlineData (short.MaxValue)]
        public void CFNumberRoundTrip_Int16 (short number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToInt16 ());
        }

        [MacTheory]
        [InlineData (ushort.MinValue)]
        [InlineData (default (ushort))]
        [InlineData (ushort.MaxValue)]
        public void CFNumberRoundTrip_UInt16 (ushort number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToUInt16 ());
        }

        [MacTheory]
        [InlineData (int.MinValue)]
        [InlineData (default (int))]
        [InlineData (int.MaxValue)]
        public void CFNumberRoundTrip_Int32 (int number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToInt32 ());
        }

        [MacTheory]
        [InlineData (uint.MinValue)]
        [InlineData (default (uint))]
        [InlineData (uint.MaxValue)]
        public void CFNumberRoundTrip_UInt32 (uint number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToUInt32 ());
        }

        [MacTheory]
        [InlineData (long.MinValue)]
        [InlineData (default (long))]
        [InlineData (long.MaxValue)]
        public void CFNumberRoundTrip_Int64 (long number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToInt64 ());
        }

        [MacTheory]
        [InlineData (ulong.MinValue)]
        [InlineData (default (ulong))]
        [InlineData (ulong.MaxValue)]
        public void CFNumberRoundTrip_UInt64 (ulong number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToUInt64 ());
        }

        [MacTheory]
        [InlineData (float.MinValue)]
        [InlineData (default (float))]
        [InlineData (float.MaxValue)]
        [InlineData (float.Epsilon)]
        // [InlineData (float.NaN)] ... does not RT as CFNumber Float32
        [InlineData (float.NegativeInfinity)]
        [InlineData (float.PositiveInfinity)]
        public void CFNumberRoundTrip_Single (float number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToSingle ());
        }

        [MacTheory]
        [InlineData (double.MinValue)]
        [InlineData (default (double))]
        [InlineData (double.MaxValue)]
        [InlineData (double.Epsilon)]
        [InlineData (double.NaN)]
        [InlineData (double.NegativeInfinity)]
        [InlineData (double.PositiveInfinity)]
        [InlineData (Math.PI)]
        [InlineData (Math.E)]
        public void CFNumberRoundTrip_Double (double number)
        {
            using (var cfNumber = new CFNumber (number))
                Assert.Equal (number, cfNumber.ToDouble ());
        }

        #pragma warning restore xUnit1025

        [MacFact]
        public void CFBooleanRefs ()
        {
            Assert.True (CFBoolean.True != IntPtr.Zero);
            Assert.True (CFBoolean.False != IntPtr.Zero);
            Assert.True (CFBoolean.ToBoolean (CFBoolean.True));
            Assert.False (CFBoolean.ToBoolean (CFBoolean.False));
            Assert.Equal (CFBoolean.True, CFBoolean.ToCFBoolean (true));
            Assert.Equal (CFBoolean.False, CFBoolean.ToCFBoolean (false));
        }
    }
}