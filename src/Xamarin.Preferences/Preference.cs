//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

using Xamarin.Preferences.Converters;

namespace Xamarin.Preferences
{
    public sealed class Preference<T>
    {
        static Preference ()
        {
            TypeDescriptor.AddAttributes (
                typeof (DateTime),
                new TypeConverterAttribute (typeof (PreferenceDateTimeConverter)));

            TypeDescriptor.AddAttributes (
                typeof (DateTimeOffset),
                new TypeConverterAttribute (typeof (PreferenceDateTimeOffsetConverter)));
        }

        readonly Type type;
        readonly TypeCode typeCode;
        readonly TypeConverter converter;

        public string Key { get; }
        public T DefaultValue { get; }

        public Preference (
            string key,
            T defaultValue = default,
            TypeConverter converter = null)
        {
            Key = key
                ?? throw new ArgumentNullException (nameof (key));
            DefaultValue = defaultValue;
            type = typeof (T);
            typeCode = Type.GetTypeCode (type);
            this.converter = converter
                ?? TypeDescriptor.GetConverter (type);
        }

        public T GetValue ()
        {
            if (!PreferenceStore.ReturnDefaultValueOnException)
                return GetValueUnchecked ();

            try {
                return GetValueUnchecked ();
            } catch {
                return DefaultValue;
            }
        }

        T GetValueUnchecked ()
        {
            if (!PreferenceStore.SharedInstance.TryGetValue (
                Key,
                type,
                typeCode,
                out var value) || value == null)
                return DefaultValue;

            var valueType = value.GetType ();

            if (type.IsAssignableFrom (valueType))
                return (T)value;

            if (converter != null && converter.CanConvertFrom (valueType)) {
                return (T)converter.ConvertFrom (
                    null,
                    CultureInfo.InvariantCulture,
                    value);
            }

            return (T)Convert.ChangeType (
                value,
                type,
                CultureInfo.InvariantCulture);
        }

        public void SetValue (T value)
            => PreferenceStore.SharedInstance.SetValue (Key, value, converter);

        public void Reset ()
            => PreferenceStore.SharedInstance.Remove (Key);
    }
}