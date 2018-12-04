// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Xamarin.Linq
{
    public static class LinqExtensions
    {
        public static unsafe bool SequenceEqual (this byte[] array1, byte[] array2, int length = -1)
        {
            if (array1 == null)
                throw new ArgumentNullException (nameof (array1));

            if (array2 == null)
                throw new ArgumentNullException (nameof (array1));

            if (array1 == array2)
                return true;

            if (array1.Length != array2.Length)
                return false;

            if (length < 0)
                length = array1.Length;

            fixed (byte* array1StartPtr = array1)
            fixed (byte* array2StartPtr = array2) {
                var array1Ptr = array1StartPtr;
                var array2Ptr = array2StartPtr;

                for (int i = 0, n = length / 8; i < n; i++) {
                    if (*((long*)array1Ptr) != *((long*)array2Ptr))
                        return false;

                    array1Ptr += 8;
                    array2Ptr += 8;
                }

                if ((length & 4) != 0) {
                    if (*((int*)array1Ptr) != *((int*)array2Ptr))
                        return false;
                    array1Ptr += 4;
                    array2Ptr += 4;
                }

                if ((length & 2) != 0) {
                    if (*((short*)array1Ptr) != *((short*)array2Ptr))
                        return false;
                    array1Ptr += 2;
                    array2Ptr += 2;
                }

                if ((length & 1) != 0 && *((byte*)array1Ptr) != *((byte*)array2Ptr))
                    return false;

                return true;
            }
        }
    }
}