//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xunit;

using Xamarin.Preferences;

namespace Xamarin.Preferences.Tests
{
    public class PreferenceTests
    {
        public PreferenceTests ()
            => PreferenceStore.SharedInstance.RemoveAll ();

        [Fact]
        public void DeleteNonExistantPrefTest ()
        {
            var pref = new Preference<int> ("1");
            pref.Reset (); // This should not throw
        }

        readonly struct AnUnsupportedType
        {
        }

        [Fact]
        public void UnsupportedTypeTest ()
            => Assert.Throws<NotSupportedException> (
                () => new Preference<AnUnsupportedType> ("unsupported"));

        sealed class UselessConverter : TypeConverter
        {
        }

        [Fact]
        public void InvalidExplicitConverterTest ()
            => Assert.Throws<ArgumentException> (
                () => new Preference<AnUnsupportedType> (
                    "unsupported",
                    converter: new UselessConverter ()));

        void AssertPref<T> (
            T defaultValue,
            T minValue,
            T maxValue,
            IEnumerable<T> otherValues = null,
            TypeConverter converter = null,
            [CallerMemberName] string key = null)
        {
            var pref = new Preference<T> (
                key,
                defaultValue,
                converter);

            Assert.Equal (defaultValue, pref.DefaultValue);
            Assert.Equal (pref.DefaultValue, pref.GetValue ());
            Assert.Equal (defaultValue, pref.GetValue ());

            pref.SetValue (minValue);
            Assert.Equal (minValue, pref.GetValue ());

            pref.SetValue (default);
            Assert.Equal (default, pref.GetValue ());

            pref.SetValue (maxValue);
            Assert.Equal (maxValue, pref.GetValue ());

            foreach (var otherValue in otherValues ?? Array.Empty<T> ()) {
                pref.SetValue (otherValue);
                Assert.Equal (otherValue, pref.GetValue ());
            }

            pref.Reset ();
            Assert.Equal (defaultValue, pref.GetValue ());
        }

        [Fact]
        public void StringPref ()
        {
            const string defaultValue = "hello";
            var pref = new Preference<string> ("StringPref", defaultValue);

            Assert.Equal (defaultValue, pref.GetValue ());

            pref.SetValue (null);
            Assert.Empty (pref.GetValue ());

            pref.SetValue (string.Empty);
            Assert.Empty (pref.GetValue ());

            pref.SetValue ("something else");
            Assert.Equal ("something else", pref.GetValue ());

            pref.Reset ();
            Assert.Equal (defaultValue, pref.GetValue ());
        }

        [Fact]
        public void StringArrayTest ()
        {
            var pref = new Preference<string []> (
                "StringArrayPref",
                new [] { "default" });

            Assert.Collection (
                pref.GetValue (),
                v => Assert.Equal ("default", v));

            pref.SetValue (new string [0]);
            Assert.NotNull (pref.GetValue ());
            Assert.Empty (pref.GetValue ());

            pref.SetValue (new [] { "one", "two", "three" });
            Assert.Collection (
                pref.GetValue (),
                v => Assert.Equal ("one", v),
                v => Assert.Equal ("two", v),
                v => Assert.Equal ("three", v));

            pref.SetValue (new [] { string.Empty });
            Assert.Collection (
                pref.GetValue (),
                v => Assert.Equal (v, string.Empty));

            pref.Reset ();
            Assert.Collection (
                pref.GetValue (),
                v => Assert.Equal ("default", v));
        }

        [Fact]
        public void BoolPref ()
            => AssertPref (
                defaultValue: true,
                minValue: false,
                maxValue: true);

        [Fact]
        public void SBytePref ()
            => AssertPref<sbyte> (
                defaultValue: 99,
                minValue: sbyte.MinValue,
                maxValue: sbyte.MaxValue);

        [Fact]
        public void BytePref ()
            => AssertPref<byte> (
                defaultValue: 99,
                minValue: byte.MinValue,
                maxValue: byte.MaxValue);

        [Fact]
        public void Int16Pref ()
            => AssertPref<short> (
                defaultValue: 99,
                minValue: short.MinValue,
                maxValue: short.MaxValue);

        [Fact]
        public void UInt16Pref ()
            => AssertPref<ushort> (
                defaultValue: 99,
                minValue: ushort.MinValue,
                maxValue: ushort.MaxValue);

        [Fact]
        public void Int32Pref ()
            => AssertPref<int> (
                defaultValue: 99,
                minValue: int.MinValue,
                maxValue: int.MaxValue);

        [Fact]
        public void UInt32Pref ()
            => AssertPref<uint> (
                defaultValue: 99,
                minValue: uint.MinValue,
                maxValue: uint.MaxValue);

        [Fact]
        public void Int64Pref ()
            => AssertPref<long> (
                defaultValue: 99,
                minValue: long.MinValue,
                maxValue: long.MaxValue);

        [Fact]
        public void UInt64Pref ()
            => AssertPref<ulong> (
                defaultValue: 99,
                minValue: ulong.MinValue,
                maxValue: ulong.MaxValue);

        [Fact]
        public void DoublePref ()
            => AssertPref<double> (
                defaultValue: 99.999,
                minValue: double.MinValue,
                maxValue: double.MaxValue,
                otherValues: new [] {
                    double.Epsilon,
                    double.NaN,
                    double.NegativeInfinity,
                    double.PositiveInfinity,
                    Math.PI,
                    Math.E
                });

        [Fact]
        public void SinglePref ()
            => AssertPref<float> (
                defaultValue: 99.999f,
                minValue: float.MinValue,
                maxValue: float.MaxValue,
                otherValues: new [] {
                    float.Epsilon,
                    float.NaN,
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    (float)Math.PI,
                    (float)Math.E
                });

        [Fact]
        public void DateTimePref ()
            => AssertPref<DateTime> (
                defaultValue: DateTime.Now,
                minValue: DateTime.MinValue,
                maxValue: DateTime.MaxValue);

        [Fact]
        public void DateTimeOffsetPref ()
            => AssertPref<DateTimeOffset> (
                defaultValue: DateTimeOffset.Now,
                minValue: DateTimeOffset.MinValue,
                maxValue: DateTimeOffset.MaxValue);

        public enum TestEnum
        {
            A,
            B,
            C
        }

        [Fact]
        public void EnumPref ()
            => AssertPref<TestEnum> (
                defaultValue: TestEnum.B,
                TestEnum.A,
                TestEnum.C);

        [Flags]
        public enum TestFlagsEnum
        {
            None = 0,
            A = 1,
            B = 2,
            C = 4,
            D = 8,
            E = 16,
            F = 32,
            G = 64,
            All = A | B | C | D | E | F | G
        }

        IEnumerable<T> YieldFlagsEnumCombinations<T> ()
        {
            int maxValue = 0;
            foreach (var value in (T [])Enum.GetValues (typeof (T)))
                maxValue |= (int)(object)value;

            for (int i = 0; i <= maxValue; i++)
                yield return (T)(object)i;
        }

        [Fact]
        public void FlagsEnumPref ()
            => AssertPref<TestFlagsEnum> (
                defaultValue: TestFlagsEnum.A | TestFlagsEnum.F,
                minValue: TestFlagsEnum.None,
                maxValue: TestFlagsEnum.All,
                otherValues: YieldFlagsEnumCombinations<TestFlagsEnum> ());

        [Fact]
        public void ExplicitTypeConverterPref ()
            => AssertPref<Rect> (
                defaultValue: new Rect (1, 2, 3, 4.5),
                minValue: Rect.MinValue,
                maxValue: Rect.MaxValue,
                converter: new RectTypeConverter  ());

        [Fact]
        public void ImplicitTypeConverterPref ()
            => AssertPref<Rect> (
                defaultValue: new Rect (1, 2, 3, 4.5),
                minValue: Rect.MinValue,
                maxValue: Rect.MaxValue);

        [Fact]
        public void NamespacedPrefs ()
        {
            var pref1 = new Preference<int> ("1");
            var prefA1 = new Preference<string> ("A.1");
            var prefA2 = new Preference<int> ("A.2");
            var prefB1 = new Preference<double> ("B.1");

            pref1.SetValue (11);
            prefA1.SetValue ("test1");
            prefA2.SetValue (2);
            prefB1.SetValue (1.0);

            Assert.Equal (11, pref1.GetValue ());
            Assert.Equal ("test1", prefA1.GetValue ());
            Assert.Equal (2, prefA2.GetValue ());
            Assert.Equal (1.0, prefB1.GetValue ());

            prefA1.Reset ();
            Assert.Equal (prefA1.DefaultValue, prefA1.GetValue ());
            Assert.Equal (2, prefA2.GetValue ());
            Assert.Equal (11, pref1.GetValue ());

            prefA2.Reset ();
            Assert.Equal (prefA2.DefaultValue, prefA2.GetValue ());

            pref1.Reset ();
            Assert.Equal (pref1.DefaultValue, pref1.GetValue ());
            Assert.Equal (1.0, prefB1.GetValue ());

            prefB1.Reset ();
            Assert.Equal (prefB1.DefaultValue, prefB1.GetValue ());
        }

        [Fact]
        public void RemoveAllTest ()
        {
            var pref1 = new Preference<int> ("1");
            var prefA1 = new Preference<string> ("A.1");
            var prefA2 = new Preference<int> ("A.2");
            var prefB1 = new Preference<double> ("B.1");

            pref1.SetValue (11);
            prefA1.SetValue ("test1");
            prefA2.SetValue (2);
            prefB1.SetValue (1.0);

            Assert.Equal (4, PreferenceStore.SharedInstance.Keys.Count);

            PreferenceStore.SharedInstance.RemoveAll ();

            Assert.Empty (PreferenceStore.SharedInstance.Keys);

            Assert.Equal (pref1.DefaultValue, pref1.GetValue ());
            Assert.Equal (prefA1.DefaultValue, prefA1.GetValue ());
            Assert.Equal (prefA2.DefaultValue, prefA2.GetValue ());
            Assert.Equal (prefB1.DefaultValue, prefB1.GetValue ());
        }

        [Fact]
        public void PreferenceChangedRaisedOnSetValue ()
        {
            var pref = new Preference<int> ("SomeValue", 100);

            Assert.Equal (pref.DefaultValue, pref.GetValue ());

            Assert.Raises<PreferenceChangedEventArgs> (
                handler => PreferenceStore.SharedInstance.PreferenceChanged += handler,
                handler => PreferenceStore.SharedInstance.PreferenceChanged -= handler,
                () => pref.SetValue (200));

            Assert.Equal (200, pref.GetValue ());
        }

        [Fact]
        public void PreferenceChangedRaisedOnReset ()
        {
            var pref = new Preference<int> ("SomeValue", 100);

            Assert.Equal (pref.DefaultValue, pref.GetValue ());

            pref.SetValue (200);
            Assert.Equal (200, pref.GetValue ());

            Assert.Raises<PreferenceChangedEventArgs> (
                handler => PreferenceStore.SharedInstance.PreferenceChanged += handler,
                handler => PreferenceStore.SharedInstance.PreferenceChanged -= handler,
                () => pref.Reset ());

            Assert.Equal (pref.DefaultValue, pref.GetValue ());
        }
    }
}