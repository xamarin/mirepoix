//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Xamarin.NativeHelpers
{
    public static class CoreFoundation
    {
        const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        [DllImport (CoreFoundationLibrary)]
        public static extern void CFRelease (IntPtr obj);

        [DllImport (CoreFoundationLibrary)]
        public static extern IntPtr CFRetain (IntPtr obj);

        [StructLayout (LayoutKind.Sequential)]
        public struct CFRange
        {
            public readonly IntPtr Location;
            public readonly IntPtr Length;

            public CFRange (IntPtr location, IntPtr length)
            {
                Location = location;
                Length = length;
            }

            public CFRange (int location, int length)
            {
                Location = (IntPtr)location;
                Length = (IntPtr)length;
            }
        }

        [DllImport (CoreFoundationLibrary, CharSet = CharSet.Unicode)]
        static extern IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, IntPtr count);

        [DllImport (CoreFoundationLibrary)]
        static extern IntPtr CFStringGetLength (IntPtr handle);

        public static IntPtr CFStringCreate (string str)
            => str == null
                ? IntPtr.Zero
                : CFStringCreateWithCharacters (
                    IntPtr.Zero,
                    str,
                    new IntPtr (str.Length));

        [DllImport (CoreFoundationLibrary)]
        static extern IntPtr CFStringGetCharactersPtr (IntPtr str);

        [DllImport (CoreFoundationLibrary)]
        static extern void CFStringGetCharacters (IntPtr str, CFRange range, IntPtr buffer);

        public static unsafe string CFStringGetString (IntPtr cfStringPtr)
        {
            if (cfStringPtr == IntPtr.Zero)
                return null;

            var nlength = CFStringGetLength (cfStringPtr);
            var length = (int)nlength;
            if (nlength == IntPtr.Zero)
                return string.Empty;

            var charPtr = CFStringGetCharactersPtr (cfStringPtr);
            if (charPtr != IntPtr.Zero)
                return new string ((char *)charPtr, 0, length);

            try {
                charPtr = Marshal.AllocHGlobal (length * 2);
                CFStringGetCharacters (
                    cfStringPtr,
                    new CFRange (IntPtr.Zero, nlength),
                    charPtr);
                return new string ((char *)charPtr, 0, (int)length);
            } finally {
                Marshal.FreeHGlobal (charPtr);
            }
        }
    }
}