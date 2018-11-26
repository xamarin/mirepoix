//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.Preferences
{
    public sealed class PreferenceChangedEventArgs : EventArgs
    {
        public string Key { get; }

        internal PreferenceChangedEventArgs (string key)
        {
            if (key == null)
                throw new ArgumentNullException (nameof (key));

            Key = key;
        }
    }
}