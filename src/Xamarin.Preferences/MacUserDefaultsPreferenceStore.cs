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
    public sealed class MacUserDefaultsPreferenceStore : IPreferenceStore
    {
        #pragma warning disable 67
        public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;
        #pragma warning restore 67

        readonly string appDomain;

        public MacUserDefaultsPreferenceStore (string appDomain)
            => this.appDomain = appDomain
                ?? throw new ArgumentNullException (nameof (appDomain));

        public bool GetBoolean (string key, bool defaultValue = false)
            => throw new NotImplementedException ();

        public double GetDouble (string key, double defaultValue = 0)
            => throw new NotImplementedException ();

        public long GetInt64 (string key, long defaultValue = 0)
            => throw new NotImplementedException ();

        public string GetString (string key, string defaultValue = null)
            => throw new NotImplementedException ();

        public string [] GetStrings (string key, string [] defaultValue = null)
            => throw new NotImplementedException ();

        public void Set (string key, bool value)
            => throw new NotImplementedException ();

        public void Set (string key, long value)
            => throw new NotImplementedException ();

        public void Set (string key, double value)
            => throw new NotImplementedException ();

        public void Set (string key, string value)
            => throw new NotImplementedException ();

        public void Set (string key, string [] value)
            => throw new NotImplementedException ();
            
        public void Remove (string key)
            => throw new NotImplementedException ();

        public void RemoveAll ()
            => throw new NotImplementedException ();
            
        public IReadOnlyList<string> Keys
            => throw new NotImplementedException ();
    }
}