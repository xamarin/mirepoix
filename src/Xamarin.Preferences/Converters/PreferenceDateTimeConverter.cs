//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Xamarin.Preferences.Converters
{
    /// <summary>
    /// A better round-tripping converter for <see cref="DateTime" />. The built-in
    /// converter does not accurately round-trip some values.
    /// </summary>
    /// <remarks>
    /// With built-in we'd expect '9999-12-31T23:59:59.9999999+00:00'
    /// but get '9999-12-31T23:59:59.0000000+00:00' instead.
    /// </remarks>
    sealed class PreferenceDateTimeConverter : PreferenceTypeConverter<DateTime>
    {
        protected override object ConvertFrom (string value)
            => DateTime.Parse (
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

        protected override string ConvertTo (DateTime value)
            => value.ToString ("o");
    }
}