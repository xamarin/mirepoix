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

        public static unsafe bool SequenceEqual (this byte [] array1, byte [] array2, int offset, int length)
        {
            if (array1 == null)
                throw new ArgumentNullException (nameof (array1));

            if (array2 == null)
                throw new ArgumentNullException (nameof (array1));

            if (array1 == array2)
                return true;

            if (offset < 0)
                throw new ArgumentOutOfRangeException (
                    nameof (offset),
                    "must be >= 0");

            if (offset + length > array1.Length)
                throw new ArgumentOutOfRangeException (
                    nameof (array1),
                    "offset + length produces an index larger than the size of the array");

            if (offset + length > array2.Length)
                throw new ArgumentOutOfRangeException (
                    nameof (array2),
                    "offset + length produces an index larger than the size of the array");

            fixed (byte *array1Ptr = array1)
            fixed (byte *array2Ptr = array2)
                return memcmp (
                    array1Ptr + offset,
                    array2Ptr + offset,
                    (IntPtr)length) == 0;
        }
    }
}