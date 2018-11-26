//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.Preferences
{
    public static class PreferenceStore
    {
        static IPreferenceStore defaultPreferenceStore;
        static IPreferenceStore initializedPreferenceStore;

        public static IPreferenceStore SharedInstance {
            get {
                if (initializedPreferenceStore != null)
                    return initializedPreferenceStore;

                if (defaultPreferenceStore == null)
                    defaultPreferenceStore = new MemoryOnlyPreferenceStore ();

                return defaultPreferenceStore;
            }
        }

        public static void Initialize (IPreferenceStore preferenceStore)
        {
            if (initializedPreferenceStore != null)
                throw new InvalidOperationException (
                    $"{nameof(PreferenceStore)} has already been initialized with " +
                    initializedPreferenceStore.GetType ().FullName);

            initializedPreferenceStore = preferenceStore;
        }
    }
}