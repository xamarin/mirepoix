//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Xamarin.Preferences
{
    public sealed class MemoryOnlyPreferenceStore : IPreferenceStore
    {
        readonly Dictionary<string, object> store = new Dictionary<string, object> ();

        public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;

        void OnPreferenceChanged (string key)
            => PreferenceChanged?.Invoke (this, new PreferenceChangedEventArgs (key));

        void Set (string key, object value)
        {
            store [key] = value;
            OnPreferenceChanged (key);
        }

        public void Set (string key, bool value) => Set (key, (object)value);
        public void Set (string key, long value) => Set (key, (object)value);
        public void Set (string key, double value) => Set (key, (object)value);
        public void Set (string key, string value) => Set (key, (object)value);
        public void Set (string key, string [] value) => Set (key, (object)value);

        public bool GetBoolean (string key, bool defaultValue = false)
            => store.TryGetValue (key, out var value) ? (bool)value : defaultValue;

        public double GetDouble (string key, double defaultValue = 0.0)
            => store.TryGetValue (key, out var value) ? (double)value : defaultValue;

        public long GetInt64 (string key, long defaultValue = 0)
            => store.TryGetValue (key, out var value) ? (long)value : defaultValue;

        public string GetString (string key, string defaultValue = null)
            => store.TryGetValue (key, out var value) ? (string)value : defaultValue;

        public string [] GetStrings (string key, string [] defaultValue = null)
            => store.TryGetValue (key, out var value) ? (string [])value : defaultValue;

        public void Remove (string key)
        {
            if (store.Remove (key))
                OnPreferenceChanged (key);
        }

        public void RemoveAll ()
        {
            foreach (var key in Keys)
                Remove (key);
        }

        public IReadOnlyList<string> Keys {
            get {
                var keys = new string [store.Keys.Count];
                store.Keys.CopyTo (keys, 0);
                return keys;
            }
        }
    }
}