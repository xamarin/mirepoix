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
    public class MemoryOnlyPreferenceStore : PreferenceStore
    {
        readonly Dictionary<string, object> storage = new Dictionary<string, object> ();

        protected override IReadOnlyList<string> GetKeys ()
        {
            var keys = storage.Keys;
            var keysArray = new string [keys.Count];
            keys.CopyTo (keysArray, 0);
            return keysArray;
        }

        protected override bool StorageSetValue (string key, object value)
        {
            if (value is string || value is IEnumerable<string>) {
                storage [key] = value;
                return true;
            }

            return false;
        }

        protected override bool StorageTryGetValue (
            string key,
            Type valueType,
            TypeCode valueTypeCode,
            out object value)
            => storage.TryGetValue (key, out value);

        protected override bool StorageRemove (string key)
            => storage.Remove (key);
    }
}