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
    public sealed class MacUserDefaultsPreferenceStore : IPreferenceStore, IDisposable
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

        #pragma warning disable 67
        public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;
        #pragma warning restore 67

        readonly CFString applicationId;

        public MacUserDefaultsPreferenceStore (string applicationId)
        {
            if (applicationId == null)
                throw new ArgumentNullException (nameof (applicationId));

            this.applicationId = new CFString (applicationId);
        }

        public void Dispose ()
            => applicationId.Dispose ();

        void OnPreferenceChanged (string key)
            => PreferenceChanged?.Invoke (this, new PreferenceChangedEventArgs (key));

        IntPtr GetValue<T> (string key, long expectedCFTypeID)
        {
            using (var cfKey = new CFString (key)) {
                var valuePtr = CFPreferences.CopyValue (
                    cfKey.Handle,
                    applicationId.Handle,
                    CFPreferences.kCFPreferencesCurrentUser,
                    CFPreferences.kCFPreferencesAnyHost);

                if (valuePtr != IntPtr.Zero && CoreFoundation.CFGetTypeID (valuePtr) != expectedCFTypeID)
                    throw new InvalidCastException (
                        $"Unable to read native defaults value as CFTypeID '{expectedCFTypeID}'" +
                        $"(for conversion to {typeof (T)})");

                return valuePtr;
            }
        }

        public bool GetBoolean (string key, bool defaultValue = false)
        {
            var valuePtr = GetValue<bool> (
                key,
                CoreFoundation.CFTypeID.CFBoolean);

            if (valuePtr == IntPtr.Zero)
                return defaultValue;

            return CFBoolean.ToBoolean (valuePtr);
        }

        public double GetDouble (string key, double defaultValue = 0)
        {
            var valuePtr = GetValue<double> (
                key,
                CoreFoundation.CFTypeID.CFNumber);

            if (valuePtr == IntPtr.Zero)
                return defaultValue;

            using (var cfNumber = new CFNumber (valuePtr))
                return cfNumber.ToDouble ();
        }

        public long GetInt64 (string key, long defaultValue = 0)
        {
            var valuePtr = GetValue<long> (
                key,
                CoreFoundation.CFTypeID.CFNumber);

            if (valuePtr == IntPtr.Zero)
                return defaultValue;

            using (var cfNumber = new CFNumber (valuePtr))
                return cfNumber.ToInt64 ();
        }

        public string GetString (string key, string defaultValue = null)
        {
            var valuePtr = GetValue<string> (
                key,
                CoreFoundation.CFTypeID.CFString);

            if (valuePtr == IntPtr.Zero)
                return defaultValue;

            using (var cfString = new CFString (valuePtr))
                return cfString.ToString ();
        }

        public string [] GetStrings (string key, string [] defaultValue = null)
            => CFArray.FromCFStringArray (GetValue<string []> (
                key,
                CoreFoundation.CFTypeID.CFArray)) ?.ToArray () ?? defaultValue;

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

            OnPreferenceChanged (key);
        }

        public void Set (string key, bool value)
            => SetValue (key, CFBoolean.ToCFBoolean (value));

        public void Set (string key, long value)
        {
            using (var cfValue = new CFNumber (value))
                SetValue (key, cfValue.Handle);
        }

        public void Set (string key, double value)
        {
            using (var cfValue = new CFNumber (value))
                SetValue (key, cfValue.Handle);
        }

        public void Set (string key, string value)
        {
            if (value == null) {
                SetValue (key, IntPtr.Zero);
                return;
            }

            using (var cfValue = new CFString (value))
                SetValue (key, cfValue.Handle);
        }

        public void Set (string key, string [] value)
        {
            if (value == null) {
                SetValue (key, IntPtr.Zero);
                return;
            }

            using (var cfArray = new CFMutableArray ()) {
                cfArray.AddRange (value);
                SetValue (key, cfArray.Handle);
            }
        }

        public void Remove (string key)
            => SetValue (key, IntPtr.Zero);

        public void RemoveAll ()
        {
            foreach (var key in Keys)
                Remove (key);
        }

        public IReadOnlyList<string> Keys
            => CFArray.FromCFStringArray (CFPreferences.CopyKeyList (
                applicationId.Handle,
                CFPreferences.kCFPreferencesCurrentUser,
                CFPreferences.kCFPreferencesAnyHost))
                ?? (IReadOnlyList<string>)Array.Empty<string> ();
    }
}