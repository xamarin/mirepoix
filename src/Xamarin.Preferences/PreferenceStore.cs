//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Xamarin.Preferences
{
    public abstract class PreferenceStore
    {
        static PreferenceStore defaultPreferenceStore;
        static PreferenceStore initializedPreferenceStore;

        public static bool ReturnDefaultValueOnException = true;

        public static PreferenceStore SharedInstance {
            get {
                if (initializedPreferenceStore != null)
                    return initializedPreferenceStore;

                if (defaultPreferenceStore == null)
                    defaultPreferenceStore = new MemoryOnlyPreferenceStore ();

                return defaultPreferenceStore;
            }
        }

        public static void Initialize (PreferenceStore preferenceStore)
        {
            if (initializedPreferenceStore != null)
                throw new InvalidOperationException (
                    $"{nameof(PreferenceStore)} has already been initialized with " +
                    initializedPreferenceStore.GetType ().FullName);

            initializedPreferenceStore = preferenceStore;
        }

        /// <summary>
        /// Strictly for unit tests. Do not use elsewhere!
        /// </summary>
        internal static void InitializeForUnitTests (PreferenceStore preferenceStore)
            => initializedPreferenceStore = preferenceStore;

        public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;

        protected virtual void OnPreferenceChanged (string key)
            => PreferenceChanged?.Invoke (this, new PreferenceChangedEventArgs (key));

        public void SetValue (string key, object value, TypeConverter converter)
        {
            // We will attempt to make at most two writes to storage:
            // 1. allow the storage to handle the value as-is
            // 2. convert the value to a string and then store it

            for (int i = 0; i < 2; i++) {
                // null values always imply a removal - this is primarily due to the
                // limiting semantics of CFPreferences on macOS and a desire for
                // consistent behavior with null across PreferenceStore implementations
                if (value == null) {
                    Remove (key);
                    return;
                }

                // If the value could be stored natively by the subclass, we're all good
                if (StorageSetValue (key, value)) {
                    OnPreferenceChanged (key);
                    return;
                }

                // Otherwise, try to convert it to a string representation and try again
                // by going to the second iteration of the loop
                value = converter == null
                    ? Convert.ChangeType (
                        value,
                        typeof (string),
                        CultureInfo.InvariantCulture)
                    : converter.ConvertToInvariantString (value);
            }

            throw new NotImplementedException (
                $"{GetType ()} must implement string value storage at a minimum");
        }

        protected abstract bool StorageSetValue (string key, object value);

        public bool TryGetValue (
            string key,
            Type valueType,
            TypeCode valueTypeCode,
            out object value)
            => StorageTryGetValue (
                key,
                valueType,
                valueTypeCode,
                out value);

        protected abstract bool StorageTryGetValue (
            string key,
            Type valueType,
            TypeCode valueTypeCode,
            out object value);

        public void Remove (string key)
        {
            if (StorageRemove (key))
                OnPreferenceChanged (key);
        }

        protected abstract bool StorageRemove (string key);

        public virtual void RemoveAll ()
        {
            foreach (var key in GetKeys () ?? Array.Empty<string> ())
                Remove (key);
        }

        // For unit tests only unless there is a clear use-case for making this public
        internal IReadOnlyList<string> Keys => GetKeys ();

        protected abstract IReadOnlyList<string> GetKeys ();
    }
}