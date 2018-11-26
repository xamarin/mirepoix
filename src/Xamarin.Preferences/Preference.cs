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
    public sealed class Preference<T>
    {
        readonly Type type;
        readonly TypeConverter converter;
        readonly Func<T> getter;
        readonly Action<T> setter;

        public string Key { get; }
        public T DefaultValue { get; }

        public Preference (
            string key,
            T defaultValue = default (T),
            TypeConverter converter = null)
        {
            if (key == null)
                throw new ArgumentNullException (nameof (key));

            Key = key;
            DefaultValue = defaultValue;
            type = typeof (T);

            if (converter != null) {
                this.converter = converter;
            } else if (type.IsEnum) {
                getter = GetEnum;
                setter = SetEnum;
                return;
            } else {
                switch (Type.GetTypeCode (type)) {
                case TypeCode.String:
                    getter = GetString;
                    setter = SetString;
                    return;
                case TypeCode.Boolean:
                    getter = GetBoolean;
                    setter = SetBoolean;
                    return;
                case TypeCode.Single:
                    getter = GetSingle;
                    setter = SetSingle;
                    return;
                case TypeCode.Double:
                    getter = GetDouble;
                    setter = SetDouble;
                    return;
                case TypeCode.SByte:
                    getter = GetSByte;
                    setter = SetSByte;
                    return;
                case TypeCode.Byte:
                    getter = GetByte;
                    setter = SetByte;
                    return;
                case TypeCode.Int16:
                    getter = GetInt16;
                    setter = SetInt16;
                    return;
                case TypeCode.UInt16:
                    getter = GetUInt16;
                    setter = SetUInt16;
                    return;
                case TypeCode.Int32:
                    getter = GetInt32;
                    setter = SetInt32;
                    return;
                case TypeCode.UInt32:
                    getter = GetUInt32;
                    setter = SetUInt32;
                    return;
                case TypeCode.Int64:
                    getter = GetInt64;
                    setter = SetInt64;
                    return;
                case TypeCode.UInt64:
                    getter = GetUInt64;
                    setter = SetUInt64;
                    return;
                case TypeCode.DateTime:
                    getter = GetDateTime;
                    setter = SetDateTime;
                    return;
                default:
                    if (type == typeof (string [])) {
                        getter = GetStringArray;
                        setter = SetStringArray;
                        return;
                    } else if (type == typeof (DateTimeOffset)) {
                        getter = GetDateTimeOffset;
                        setter = SetDateTimeOffset;
                        return;
                    }

                    this.converter = TypeDescriptor.GetConverter (type);
                    break;
                }
            }

            if (this.converter != null &&
                this.converter.CanConvertFrom (typeof (string)) &&
                this.converter.CanConvertTo (type)) {
                getter = GetConverter;
                setter = SetConverter;
                return;
            }

            if (converter != null)
                throw new ArgumentException (
                    $"must be able to convert from {typeof (string)} and to {type}",
                    nameof (converter));

            throw new NotSupportedException ($"preference type not supported: {type}");
        }

        public T GetValue ()
            => getter ();

        public void SetValue (T value)
            => setter (value);

        public void Reset ()
            => PreferenceStore.SharedInstance.Remove (Key);

        T GetConverter ()
        {
            var value = converter.ConvertFromInvariantString (
                PreferenceStore.SharedInstance.GetString (Key));

            if (value == null)
                return DefaultValue;

            return (T)value;
        }

        void SetConverter (T value)
            => PreferenceStore.SharedInstance.Set (
                Key,
                converter.ConvertToInvariantString (
                    null,
                    value));

        T GetEnum ()
            => (T)Enum.Parse (
                type,
                PreferenceStore.SharedInstance.GetString (
                    Key,
                    DefaultValue.ToString ()),
                true);

        void SetEnum (T value)
            => PreferenceStore.SharedInstance.Set (
                Key,
                value.ToString ());

        T GetString ()
            => (T)(object)PreferenceStore.SharedInstance.GetString (Key, (string)(object)DefaultValue);

        void SetString (T value)
            => PreferenceStore.SharedInstance.Set (Key, (string)(object)value ?? String.Empty);

        T GetBoolean ()
            => (T)(object)PreferenceStore.SharedInstance.GetBoolean (Key, (bool)(object)DefaultValue);

        void SetBoolean (T value)
            => PreferenceStore.SharedInstance.Set (Key, (bool)(object)value);

        T GetSingle ()
            => (T)(object)unchecked((float)PreferenceStore.SharedInstance.GetDouble (
                Key,
                unchecked((float)(object)DefaultValue)));

        void SetSingle (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((float)(object)value));

        T GetDouble ()
            => (T)(object)PreferenceStore.SharedInstance.GetDouble (Key, (double)(object)DefaultValue);

        void SetDouble (T value)
            => PreferenceStore.SharedInstance.Set (Key, (double)(object)value);

        T GetSByte ()
            => (T)(object)unchecked((sbyte)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((sbyte)(object)DefaultValue)));

        void SetSByte (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((sbyte)(object)value));

        T GetByte ()
            => (T)(object)unchecked((byte)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((byte)(object)DefaultValue)));

        void SetByte (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((byte)(object)value));

        T GetInt16 ()
            => (T)(object)unchecked((short)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((short)(object)DefaultValue)));

        void SetInt16 (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((short)(object)value));

        T GetUInt16 ()
            => (T)(object)unchecked((ushort)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((ushort)(object)DefaultValue)));

        void SetUInt16 (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((ushort)(object)value));

        T GetInt32 ()
            => (T)(object)unchecked((int)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((int)(object)DefaultValue)));

        void SetInt32 (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((int)(object)value));

        T GetUInt32 ()
            => (T)(object)unchecked((uint)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((uint)(object)DefaultValue)));

        void SetUInt32 (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((uint)(object)value));

        T GetInt64 ()
            => (T)(object)PreferenceStore.SharedInstance.GetInt64 (Key, (long)(object)DefaultValue);

        void SetInt64 (T value)
            => PreferenceStore.SharedInstance.Set (Key, (long)(object)value);

        T GetUInt64 ()
            => (T)(object)unchecked((ulong)PreferenceStore.SharedInstance.GetInt64 (
                Key,
                unchecked((long)(ulong)(object)DefaultValue)));

        void SetUInt64 (T value)
            => PreferenceStore.SharedInstance.Set (Key, unchecked((long)(ulong)(object)value));

        T GetDateTime ()
            => (T)(object)DateTime.Parse (
                PreferenceStore.SharedInstance.GetString (
                    Key,
                    ((DateTime)(object)DefaultValue).ToString ("o")),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

        void SetDateTime (T value)
            => PreferenceStore.SharedInstance.Set (
                Key,
                ((DateTime)(object)value).ToString ("o"));

        T GetDateTimeOffset ()
            => (T)(object)DateTimeOffset.Parse (
                PreferenceStore.SharedInstance.GetString (
                    Key,
                    ((DateTimeOffset)(object)DefaultValue).ToString ("o")),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

        void SetDateTimeOffset (T value)
            => PreferenceStore.SharedInstance.Set (
                Key,
                ((DateTimeOffset)(object)value).ToString ("o"));

        T GetStringArray ()
            => (T)(object)PreferenceStore.SharedInstance.GetStrings (
                Key,
                (string [])(object)DefaultValue);

        void SetStringArray (T value)
            => PreferenceStore.SharedInstance.Set (Key, (string [])(object)value);
    }
}