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
        [Theory]
        [ClassData (typeof (GB18030TestDataWithNullAndEmpty))]
        public void ToCFString (string stringDescription, string stringValue)
        {
            var cfStringPtr = CFStringCreate (stringValue);
            var roundTripStringValue = CFStringGetString (cfStringPtr);
            if (cfStringPtr != IntPtr.Zero)
                CFRelease (cfStringPtr);
            Assert.Equal (stringValue, roundTripStringValue);
        }
    }
}