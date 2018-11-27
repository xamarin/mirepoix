//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static Xamarin.NativeHelpers.Dlfcn;

using CFIndex = System.Int64;

namespace Xamarin.NativeHelpers
{
    public static class CoreFoundation
    {
        public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public static readonly IntPtr CoreFoundationLibraryHandle = dlopen (CoreFoundationLibrary, 0);

        [DllImport (CoreFoundationLibrary)]
        public static extern void CFRelease (IntPtr obj);

        [DllImport (CoreFoundationLibrary)]
        public static extern IntPtr CFRetain (IntPtr obj);

        [DllImport (CoreFoundationLibrary)]
        public static extern long CFGetTypeID (IntPtr obj);

        public static class CFTypeID
        {
            [DllImport (CoreFoundationLibrary)]
            static extern long CFBooleanGetTypeID ();

            public static readonly long CFBoolean = CFBooleanGetTypeID ();

            [DllImport (CoreFoundationLibrary)]
            static extern long CFNumberGetTypeID ();

            public static readonly long CFNumber = CFNumberGetTypeID ();


            [DllImport (CoreFoundationLibrary)]
            static extern long CFArrayGetTypeID ();

            public static readonly long CFArray = CFArrayGetTypeID ();

            [DllImport (CoreFoundationLibrary)]
            static extern long CFStringGetTypeID ();

            public static readonly long CFString = CFStringGetTypeID ();
        }

        [StructLayout (LayoutKind.Sequential)]
        public readonly struct CFRange
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

        public abstract class CFObject : IDisposable
        {
            IntPtr handle;
            public IntPtr Handle => handle;

            protected CFObject (IntPtr handle, bool ownsHandle = true)
            {
                this.handle = handle;
                if (!ownsHandle)
                    CFRetain (handle);
            }

            ~CFObject ()
            {
                Dispose (false);
            }

            public void Dispose ()
            {
                Dispose (true);
                GC.SuppressFinalize (this);
            }

            protected virtual void Dispose (bool disposing)
            {
                if (handle != IntPtr.Zero) {
                    CFRelease (handle);
                    handle = IntPtr.Zero;
                }
            }
        }

        public static class CFBoolean
        {
            public static readonly IntPtr True = ReadIntPtr (CoreFoundationLibraryHandle, "kCFBooleanTrue");
            public static readonly IntPtr False = ReadIntPtr (CoreFoundationLibraryHandle, "kCFBooleanFalse");

            [DllImport (CoreFoundationLibrary)]
            static extern bool CFBooleanGetValue (IntPtr cfBooleanRef);

            public static bool ToBoolean (IntPtr cfBooleanRef)
                => CFBooleanGetValue (cfBooleanRef);

            public static IntPtr ToCFBoolean (bool value)
                => value ? True : False;
        }

        public enum CFNumberType : CFIndex
        {
            SInt8 = 1,
            SInt16 = 2,
            SInt32 = 3,
            SInt64 = 4,
            Float32 = 5,
            Float64 = 6,
            Char = 7,
            Short = 8,
            Int = 9,
            Long = 10,
            LongLong = 11,
            Float = 12,
            Double = 13,
            CFIndex = 14,
            NSInteger = 15,
            CGFloat = 16
        }

        public sealed class CFNumber : CFObject
        {
            [DllImport (CoreFoundationLibrary)]
            static extern unsafe IntPtr CFNumberCreate (IntPtr allocator, CFNumberType theType, void *valuePtr);

            public CFNumber (IntPtr handle, bool ownsHandle = true)
                : base (handle, ownsHandle)
            {
            }

            unsafe CFNumber (CFNumberType theType, void *valuePtr)
                : base (CFNumberCreate (IntPtr.Zero, theType, valuePtr))
            {
            }

            public unsafe CFNumber (sbyte value) : this (CFNumberType.SInt8, &value)
            {
            }

            public unsafe CFNumber (byte value) : this (CFNumberType.SInt8, &value)
            {
            }

            public unsafe CFNumber (short value) : this (CFNumberType.SInt16, &value)
            {
            }

            public unsafe CFNumber (ushort value) : this (CFNumberType.SInt16, &value)
            {
            }

            public unsafe CFNumber (int value) : this (CFNumberType.SInt32, &value)
            {
            }

            public unsafe CFNumber (uint value) : this (CFNumberType.SInt32, &value)
            {
            }

            public unsafe CFNumber (long value) : this (CFNumberType.SInt64, &value)
            {
            }

            public unsafe CFNumber (ulong value) : this (CFNumberType.SInt64, &value)
            {
            }

            public unsafe CFNumber (float value) : this (CFNumberType.Float32, &value)
            {
            }

            public unsafe CFNumber (double value) : this (CFNumberType.Float64, &value)
            {
            }

            [DllImport (CoreFoundationLibrary)]
            static extern CFNumberType CFNumberGetType (IntPtr number);

            public CFNumberType Type => CFNumberGetType (Handle);

            [DllImport (CoreFoundationLibrary)]
            static extern unsafe bool CFNumberGetValue (IntPtr number, CFNumberType theType, void *valuePtr);

            unsafe void GetValue (CFNumberType theType, void *valuePtr)
            {
                if (!CFNumberGetValue (Handle, theType, valuePtr))
                    throw new InvalidCastException ($"unable to convert CFNumber to {theType}");
            }

            public unsafe sbyte ToSByte ()
            {
                sbyte value;
                GetValue (CFNumberType.SInt8, &value);
                return value;
            }

            public byte ToByte ()
                => (byte)ToSByte ();

            public unsafe short ToInt16 ()
            {
                short value;
                GetValue (CFNumberType.SInt16, &value);
                return value;
            }

            public ushort ToUInt16 ()
                => (ushort)ToInt16 ();

            public unsafe int ToInt32 ()
            {
                int value;
                GetValue (CFNumberType.SInt32, &value);
                return value;
            }

            public uint ToUInt32 ()
                => (uint)ToInt32 ();

            public unsafe long ToInt64 ()
            {
                long value;
                GetValue (CFNumberType.SInt64, &value);
                return value;
            }

            public ulong ToUInt64 ()
                => (ulong)ToInt64 ();

            public unsafe float ToSingle ()
            {
                float value;
                GetValue (CFNumberType.Float32, &value);
                return value;
            }

            public unsafe double ToDouble ()
            {
                double value;
                GetValue (CFNumberType.Float64, &value);
                return value;
            }
        }

        public sealed class CFString : CFObject
        {
            [DllImport (CoreFoundationLibrary, CharSet = CharSet.Unicode)]
            static extern IntPtr CFStringCreateWithCharacters (IntPtr allocator, string str, IntPtr count);

            public CFString (IntPtr handle, bool ownsHandle = true)
                : base (handle, ownsHandle)
            {
            }

            public CFString (string str)
                : base (CFStringCreateWithCharacters (
                    IntPtr.Zero,
                    str ?? throw new ArgumentNullException (nameof (str)),
                    new IntPtr (str.Length)))
            {
            }

            [DllImport (CoreFoundationLibrary)]
            static extern IntPtr CFStringGetLength (IntPtr handle);

            public int Length => (int)CFStringGetLength (Handle);

            [DllImport (CoreFoundationLibrary)]
            static extern IntPtr CFStringGetCharactersPtr (IntPtr str);

            [DllImport (CoreFoundationLibrary)]
            static extern void CFStringGetCharacters (IntPtr str, CFRange range, IntPtr buffer);

            public unsafe override string ToString ()
            {
                if (Handle == IntPtr.Zero)
                    return null;

                var nlength = CFStringGetLength (Handle);
                var length = (int)nlength;
                if (nlength == IntPtr.Zero)
                    return string.Empty;

                var charPtr = CFStringGetCharactersPtr (Handle);
                if (charPtr != IntPtr.Zero)
                    return new string ((char *)charPtr, 0, length);

                try {
                    charPtr = Marshal.AllocHGlobal (length * 2);
                    CFStringGetCharacters (
                        Handle,
                        new CFRange (IntPtr.Zero, nlength),
                        charPtr);
                    return new string ((char *)charPtr, 0, (int)length);
                } finally {
                    Marshal.FreeHGlobal (charPtr);
                }
            }

            [DllImport (CoreFoundationLibrary, CharSet = CharSet.Unicode)]
		    static extern char CFStringGetCharacterAtIndex (IntPtr handle, IntPtr p);

            public char this [int index] {
                get => CFStringGetCharacterAtIndex (Handle, new IntPtr (index));
            }
        }

        public abstract class CFArrayBase : CFObject, IReadOnlyList<IntPtr>
        {
            private protected CFArrayBase (IntPtr handle, bool ownsHandle)
                : base (handle, ownsHandle)
            {
            }

            [DllImport (CoreFoundationLibrary)]
            static extern CFIndex CFArrayGetCount (IntPtr theArray);

            public int Count => (int)CFArrayGetCount (Handle);

            [DllImport (CoreFoundationLibrary)]
            static extern IntPtr CFArrayGetValueAtIndex (IntPtr theArray, CFIndex idx);

            public IntPtr this [int index] => CFArrayGetValueAtIndex (Handle, (CFIndex)index);
            public IEnumerator<IntPtr> GetEnumerator ()
            {
                for (int i = 0, n = Count; i < n; i++)
                    yield return this [i];
            }

            IEnumerator IEnumerable.GetEnumerator ()
                => GetEnumerator ();
        }

        public sealed class CFArray : CFArrayBase
        {
            static readonly IntPtr kCFTypeArrayCallBacks = dlsym (
                CoreFoundationLibraryHandle,
                nameof (kCFTypeArrayCallBacks));

            public CFArray (IntPtr handle, bool ownsHandle = true)
                : base (handle, ownsHandle)
            {
            }

            /// <summary>
            /// Read a <see cref="CFArray" /> from <paramref name="cfArrayHandle" /> containing
            /// only <see cref="CFString" /> or <c>null</c> items. If an item is not null or
            /// <see cref="CFString" />, a <see cref="ArrayTypeMismatchException" /> will be raised.
            /// </summary>
            public static List<string> FromCFStringArray (IntPtr cfArrayHandle, bool ownsHandle = true)
            {
                if (cfArrayHandle == IntPtr.Zero)
                    return null;

                using (var cfArray = new CFArray (cfArrayHandle, ownsHandle)) {
                    var list = new List<string> (cfArray.Count);

                    foreach (var itemPtr in cfArray) {
                        if (itemPtr == IntPtr.Zero)
                            list.Add (null);
                        else if (CFGetTypeID (itemPtr) != CFTypeID.CFString)
                            throw new ArrayTypeMismatchException ();

                        using (var cfString = new CFString (itemPtr, ownsHandle: false))
                           list.Add (cfString.ToString ());
                    }

                    return list;
                }
            }
        }

        public sealed class CFMutableArray : CFArrayBase
        {
            static readonly IntPtr kCFTypeArrayCallBacks = dlsym (
                CoreFoundationLibraryHandle,
                nameof (kCFTypeArrayCallBacks));

            [DllImport (CoreFoundationLibrary)]
            static extern IntPtr CFArrayCreateMutable (IntPtr allocator, CFIndex capacity, IntPtr callBacks);

            public CFMutableArray () : base (
                CFArrayCreateMutable (IntPtr.Zero, 0, kCFTypeArrayCallBacks),
                ownsHandle: true)
            {
            }

            [DllImport (CoreFoundationLibrary)]
            static extern void CFArrayAppendValue (IntPtr theArray, IntPtr value);

            public void Add (CFObject value)
                => CFArrayAppendValue (
                    Handle,
                    value == null
                        ? IntPtr.Zero
                        : value.Handle);

            public void AddRange (IEnumerable<string> values)
            {
                if (values == null)
                    throw new ArgumentNullException (nameof (values));

                foreach (var value in values) {
                    if (value == null) {
                        CFArrayAppendValue (Handle, IntPtr.Zero);
                        continue;
                    }

                    using (var cfString = new CFString (value))
                        CFArrayAppendValue (Handle, cfString.Handle);
                }
            }
        }
    }
}