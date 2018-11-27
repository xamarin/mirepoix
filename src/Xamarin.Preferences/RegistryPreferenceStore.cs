//
// Author:
//   Sandy Armstrong <sandy@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Microsoft.Win32;

namespace Xamarin.Preferences
{
    public sealed class RegistryPreferenceStore : PreferenceStore
    {
        readonly RegistryHive hive;
        readonly RegistryView view;
        readonly string prefsSubKey;

        public RegistryPreferenceStore (
            RegistryHive hive,
            RegistryView view,
            string prefsSubKey)
        {
            this.hive = hive;
            this.view = view;
            this.prefsSubKey = prefsSubKey;
        }

        RegistryKey GetPrefsBaseKey ()
            => RegistryKey.OpenBaseKey (hive, view);

        RegistryKey GetPrefsKey (bool writable = false)
            => writable
                ? GetPrefsBaseKey ().CreateSubKey (prefsSubKey, writable: true)
                : GetPrefsBaseKey ().OpenSubKey (prefsSubKey);

        RegistryKey GetSubKey (KeyPath keyPath, bool writable = false)
        {
            var prefsKey = GetPrefsKey (writable);

            if (String.IsNullOrEmpty (keyPath.SubKey))
                return prefsKey;

            return writable
                ? prefsKey?.CreateSubKey (keyPath.SubKey, writable: true)
                : prefsKey?.OpenSubKey (keyPath.SubKey);
        }

        readonly struct KeyPath
        {
            public string SubKey { get; }
            public string Name { get; }

            public KeyPath (string prefKey)
            {
                var split = prefKey.LastIndexOf ('.');
                Name = prefKey.Substring (split + 1);
                if (split >= 0)
                    SubKey = prefKey.Substring (0, split).Replace ('.', '\\');
                else
                    SubKey = null;
            }
        }

        protected override bool StorageSetValue (string key, object value)
        {
            RegistryValueKind valueKind;

            switch (value) {
            case string _:
                valueKind = RegistryValueKind.String;
                break;
            case IEnumerable<string> v:
                valueKind = RegistryValueKind.MultiString;
                break;
            case bool v:
                valueKind = RegistryValueKind.DWord;
                value = v ? 1 : 0;
                break;
            case sbyte v:
                valueKind = RegistryValueKind.DWord;
                value = (int)v;
                break;
            case byte v:
                valueKind = RegistryValueKind.DWord;
                value = (int)v;
                break;
            case short v:
                valueKind = RegistryValueKind.DWord;
                value = (int)v;
                break;
            case ushort v:
                valueKind = RegistryValueKind.DWord;
                value = (int)v;
                break;
            case int _:
                valueKind = RegistryValueKind.DWord;
                break;
            case uint v:
                valueKind = RegistryValueKind.DWord;
                value = (int)v;
                break;
            case long _:
                valueKind = RegistryValueKind.QWord;
                break;
            case ulong v:
                valueKind = RegistryValueKind.QWord;
                value = (long)v;
                break;
            default:
                return false;
            }

            var keyPath = new KeyPath (key);
            using (var registryKey = GetSubKey (keyPath, writable: true))
                registryKey.SetValue (keyPath.Name, value, valueKind);

            return true;
        }

        protected override bool StorageTryGetValue (
            string key,
            Type valueType,
            TypeCode valueTypeCode,
            out object value)
        {
            var keyPath = new KeyPath (key);
            using (var registryKey = GetSubKey (keyPath)) {
                if (registryKey == null) {
                    value = null;
                    return false;
                }

                value = registryKey.GetValue (keyPath.Name);

                switch (value) {
                case int v:
                    switch (valueTypeCode) {
                    case TypeCode.SByte:
                        value = (sbyte)v;
                        return true;
                    case TypeCode.Byte:
                        value = (byte)v;
                        return true;
                    case TypeCode.Int16:
                        value = (short)v;
                        return true;
                    case TypeCode.UInt16:
                        value = (ushort)v;
                        return true;
                    case TypeCode.Int32:
                        return true;
                    case TypeCode.UInt32:
                        value = (uint)v;
                        return true;
                    }
                    break;
                case long v when valueTypeCode == TypeCode.UInt64:
                    value = (ulong)v;
                    return true;
                }

                return value != null;
            }
        }

        protected override bool StorageRemove (string key)
        {
            var keyPath = new KeyPath (key);
            using (var registryKey = GetSubKey (keyPath, writable: true)) {
                try {
                    if (registryKey != null) {
                        registryKey.DeleteValue (keyPath.Name);
                        return true;
                    }
                } catch (ArgumentException) {
                    // Expected if sub key doesn't actually exist yet
                }
            }

            return false;
        }

        public override void RemoveAll ()
        {
            var deletingPrefs = GetKeys ();

            using (var baseKey = GetPrefsBaseKey ()) {
                try {
                    baseKey.DeleteSubKeyTree (prefsSubKey);
                } catch (ArgumentException) {
                    // Expected if sub key doesn't actually exist yet
                }
            }

            foreach (var key in deletingPrefs)
                OnPreferenceChanged (key);
        }

        protected override IReadOnlyList<string> GetKeys ()
        {
            using (var prefsKey = GetPrefsKey ())
                return GetPrefKeys (prefsKey, null);
        }

        IReadOnlyList<string> GetPrefKeys (RegistryKey key, string basePath)
        {
            var keys = new List<string> ();
            if (key == null)
                return keys;

            foreach (var name in key.GetValueNames ()) {
                var prefKey = name;
                if (!String.IsNullOrEmpty (basePath))
                    prefKey = $"{basePath}.{name}";
                keys.Add (prefKey);
            }

            foreach (var subKey in key.GetSubKeyNames ()) {
                keys.AddRange (GetPrefKeys (
                    key.OpenSubKey (subKey),
                    String.IsNullOrEmpty (basePath) ? subKey : $"{basePath}.{subKey}"));
            }

            return keys;
        }
    }
}