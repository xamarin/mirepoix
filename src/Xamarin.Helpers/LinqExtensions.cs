// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Xamarin.Linq
{
    public static class LinqExtensions
    {
        [DllImport (
            "msvcrt.dll",
            EntryPoint = "memcmp",
            CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int windows_memcmp (byte *s1, byte *s2, IntPtr n);

        [DllImport (
            "libc",
            EntryPoint = "memcmp",
            CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int posix_memcmp (byte *s1, byte *s2, IntPtr n);

        unsafe delegate int memcmp_handler (byte *s1, byte *s2, IntPtr n);

        static memcmp_handler memcmp;

        static unsafe LinqExtensions ()
        {
            if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
                memcmp = windows_memcmp;
            else
                memcmp = posix_memcmp;
        }

        /// <summary>
        /// Compares <paramref name="length"/> elements of two byte arrays starting
        /// at <paramref name="offset"/> for sequence equality using <c>memcmp</c>.
        /// </summary>
        /// <remarks>
        /// <c><paramref name="offset"/> + <paramref name="length"/></c> must produce a legal
        /// index into both <paramref name="array1"/> and <paramref name="array2"/> to avoid
        /// an out of bounds read.
        /// </remarks>
        /// <param name="array1">Array to compare against <paramref name="array2"/></param>
        /// <param name="array2">Array to compare against <paramref name="array1"/></param>
        /// <param name="offset">Starting offset for comparison into the arrays</param>
        /// <param name="length">Number of bytes to compare</param>
        /// <returns>
        /// Returns <c>true</c> if the two byte arrays are equal with respect to
        /// <paramref name="offset"/> and <paramref name="length"/>.
        /// </returns>
        public static unsafe bool SequenceEqual (this byte [] array1, byte [] array2, int offset, int length)
        {
            if (array1 == null)
                throw new ArgumentNullException (nameof (array1));

            if (array2 == null)
                throw new ArgumentNullException (nameof (array1));

            if (offset < 0)
                throw new ArgumentOutOfRangeException (
                    nameof (offset),
                    "must be >= 0");

            if (length < 0)
                throw new ArgumentOutOfRangeException (
                    nameof (length),
                    "must be >= 0");

            if (offset + length > array1.Length)
                throw new ArgumentOutOfRangeException (
                    nameof (array1),
                    "offset + length produces an index larger than the size of the array");

            if (offset + length > array2.Length)
                throw new ArgumentOutOfRangeException (
                    nameof (array2),
                    "offset + length produces an index larger than the size of the array");

            if (array1 == array2)
                return true;

            fixed (byte *array1Ptr = array1)
            fixed (byte *array2Ptr = array2)
                return memcmp (
                    array1Ptr + offset,
                    array2Ptr + offset,
                    (IntPtr)length) == 0;
        }
    }
}