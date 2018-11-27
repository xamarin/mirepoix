//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Xamarin.NativeHelpers;
using static Xamarin.NativeHelpers.CoreFoundation;

namespace Xamarin.Preferences
{
    public sealed class MacUserDefaultsPreferenceStore : PreferenceStore, IDisposable
    {
        static class CFPreferences
        {
            public static readonly IntPtr kCFPreferencesCurrentUser = Dlfcn.ReadIntPtr (
                CoreFoundationLibraryHandle,
                nameof (kCFPreferencesCurrentUser));

            public static readonly IntPtr kCFPreferencesCurrentHost = Dlfcn.ReadIntPtr (
                CoreFoundationLibraryHandle,
                nameof (kCFPreferencesCurrentHost));

            public static readonly IntPtr kCFPreferencesAnyHost = Dlfcn.ReadIntPtr (
                CoreFoundationLibraryHandle,
                nameof (kCFPreferencesAnyHost));

            [DllImport (CoreFoundationLibrary, EntryPoint = "CFPreferencesSynchronize")]
            public static extern bool Synchronize (
                IntPtr applicationID,
                IntPtr userName,
                IntPtr hostName);

            [DllImport (CoreFoundationLibrary, EntryPoint = "CFPreferencesSetValue")]
            public static extern void SetValue (
                IntPtr key,
                IntPtr value,
                IntPtr applicationID,
                IntPtr userName,
                IntPtr hostName);

            [DllImport (CoreFoundationLibrary, EntryPoint = "CFPreferencesCopyValue")]
            public static extern IntPtr CopyValue (
                IntPtr key,
                IntPtr applicationID,
                IntPtr userName,
                IntPtr hostName);

            [DllImport (CoreFoundationLibrary, EntryPoint = "CFPreferencesCopyKeyList")]
            public static extern IntPtr CopyKeyList (
                IntPtr applicationID,
                IntPtr userName,
                IntPtr hostName);
        }

        readonly CFString applicationId;

        public MacUserDefaultsPreferenceStore (string applicationId)
        {
            if (applicationId == null)
                throw new ArgumentNullException (nameof (applicationId));

            this.applicationId = new CFString (applicationId);
        }

        public void Dispose ()
            => applicationId.Dispose ();

       void SetValue (string key, IntPtr value)
        {
            using (var cfKey = new CFString (key)) {
                CFPreferences.SetValue (
                    cfKey.Handle,
                    value,
                    applicationId.Handle,
                    CFPreferences.kCFPreferencesCurrentUser,
                    CFPreferences.kCFPreferencesAnyHost);

                CFPreferences.Synchronize (
                    applicationId.Handle,
                    CFPreferences.kCFPreferencesCurrentUser,
                    CFPreferences.kCFPreferencesAnyHost);
            }
        }

        protected override bool StorageSetValue (string key, object value)
        {
            CFObject cfValue = null;

            try {
                switch (value) {
                case bool v:
                    SetValue (key, CFBoolean.ToCFBoolean (v));
                    return true;
                case string v:
                    cfValue = new CFString (v);
                    break;
                case IEnumerable<string> v:
                    var cfArray = new CFMutableArray ();
                    cfArray.AddRange (v);
                    cfValue = cfArray;
                    break;
                case sbyte v:
                    cfValue = new CFNumber (v);
                    break;
                case byte v:
                    cfValue = new CFNumber (v);
                    break;
                case short v:
                    cfValue = new CFNumber (v);
                    break;
                case ushort v:
                    cfValue = new CFNumber (v);
                    break;
                case int v:
                    cfValue = new CFNumber (v);
                    break;
                case uint v:
                    cfValue = new CFNumber (v);
                    break;
                case long v:
                    cfValue = new CFNumber (v);
                    break;
                case ulong v:
                    cfValue = new CFNumber (v);
                    break;
                case float v:
                    cfValue = new CFNumber (v);
                    break;
                case double v:
                    cfValue = new CFNumber (v);
                    break;
                default:
                    return false;
                }

                SetValue (key, cfValue.Handle);
                return true;
            } finally {
                cfValue?.Dispose ();
            }
        }

        protected override bool StorageTryGetValue (
            string key,
            Type valueType,
            TypeCode valueTypeCode,
            out object value)
        {
            using (var cfKey = new CFString (key)) {
                var valuePtr = CFPreferences.CopyValue (
                    cfKey.Handle,
                    applicationId.Handle,
                    CFPreferences.kCFPreferencesCurrentUser,
                    CFPreferences.kCFPreferencesAnyHost);

                if (valuePtr == IntPtr.Zero) {
                    value = null;
                    return false;
                }

                var typeId = CoreFoundation.CFGetTypeID (valuePtr);

                if (typeId == CoreFoundation.CFTypeID.CFBoolean) {
                    value = CFBoolean.ToBoolean (valuePtr);
                    return true;
                }

                if (typeId == CoreFoundation.CFTypeID.CFString) {
                    using (var cfString = new CFString (valuePtr))
                        value = cfString.ToString ();
                    return true;
                }

                if (typeId == CoreFoundation.CFTypeID.CFNumber) {
                    using (var cfNumber = new CFNumber (valuePtr)) {
                        switch (cfNumber.Type) {
                        case CFNumberType.SInt8:
                        case CFNumberType.Char:
                            value = valueTypeCode == TypeCode.Byte
                                ? cfNumber.ToByte ()
                                : (object)cfNumber.ToSByte ();
                            return true;
                        case CFNumberType.SInt16:
                        case CFNumberType.Short:
                            value = valueTypeCode == TypeCode.UInt16
                                ? cfNumber.ToUInt16 ()
                                : (object)cfNumber.ToInt16 ();
                            return true;
                        case CFNumberType.SInt32:
                        case CFNumberType.Int:
                        case CFNumberType.Long:
                            value = valueTypeCode == TypeCode.UInt32
                                ? cfNumber.ToUInt32 ()
                                : (object)cfNumber.ToInt32 ();
                            return true;
                        case CFNumberType.SInt64:
                        case CFNumberType.LongLong:
                        case CFNumberType.CFIndex:
                        case CFNumberType.NSInteger:
                            value = valueTypeCode == TypeCode.UInt64
                                ? cfNumber.ToUInt64 ()
                                : (object)cfNumber.ToInt64 ();
                            return true;
                        case CFNumberType.Float32:
                        case CFNumberType.Float:
                            value = cfNumber.ToSingle ();
                            return true;
                        case CFNumberType.Float64:
                        case CFNumberType.Double:
                        case CFNumberType.CGFloat:
                            value = cfNumber.ToDouble ();
                            return true;
                        }
                    }
                }

                if (typeId == CoreFoundation.CFTypeID.CFArray) {
                    try {
                        value = CFArray
                            .FromCFStringArray (valuePtr)
                            ?.ToArray ();
                        return value != null;
                    } catch {
                        value = null;
                        return false;
                    }
                }

                value = null;
                return false;
            }
        }

        protected override bool StorageRemove (string key)
        {
            SetValue (key, IntPtr.Zero);
            return true;
        }

        protected override IReadOnlyList<string> GetKeys ()
            => CFArray.FromCFStringArray (CFPreferences.CopyKeyList (
                applicationId.Handle,
                CFPreferences.kCFPreferencesCurrentUser,
                CFPreferences.kCFPreferencesAnyHost))
                ?? (IReadOnlyList<string>)Array.Empty<string> ();
    }
}