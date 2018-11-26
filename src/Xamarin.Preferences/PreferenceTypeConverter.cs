//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Xamarin.Preferences
{
    public abstract class PreferenceTypeConverter<T> : TypeConverter
    {
        protected abstract object ConvertFrom (string value);

        public sealed override bool CanConvertFrom (
            ITypeDescriptorContext context,
            Type sourceType)
            => sourceType == typeof (string);

        public sealed override object ConvertFrom (
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
            => ConvertFrom ((string)value);

        protected abstract string ConvertTo (T value);

        public sealed override bool CanConvertTo (
            ITypeDescriptorContext context,
            Type destinationType)
            => destinationType == typeof (T);

        public sealed override object ConvertTo (
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
            => ConvertTo (value == null ? default (T) : (T)value);
    }
}