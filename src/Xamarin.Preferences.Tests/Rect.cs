//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Xamarin.Preferences.Tests
{
    sealed class RectTypeConverter : PreferenceTypeConverter<Rect>
    {
        protected override string ConvertTo (Rect value)
            => value.ToString ();

        protected override object ConvertFrom (string value)
        {
            if (value == null)
                return null;

            return Rect.Parse (value);
        }
    }

    [TypeConverter (typeof (RectTypeConverter))]
    readonly struct Rect : IEquatable<Rect>
    {
        public static readonly Rect MinValue = new Rect (
            double.MinValue,
            double.MinValue,
            double.MinValue,
            double.MinValue);

        public static readonly Rect MaxValue = new Rect (
            double.MaxValue,
            double.MaxValue,
            double.MaxValue,
            double.MaxValue);

        public readonly double X;
        public readonly double Y;
        public readonly double Width;
        public readonly double Height;

        public Rect (
            double x,
            double y,
            double width,
            double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override int GetHashCode ()
        {
            var hash = 23;
            hash = hash * 31 + X.GetHashCode ();
            hash = hash * 31 + Y.GetHashCode ();
            hash = hash * 31 + Width.GetHashCode ();
            hash = hash * 31 + Height.GetHashCode ();
            return hash;
        }

        public override bool Equals (object obj)
            => obj is Rect rect && Equals (rect);

        public bool Equals (Rect other)
            => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override string ToString ()
            => $"{X:R}, {Y:R}, {Width:R}, {Height:R}";

        public static Rect Parse (string rect)
        {
            if (string.IsNullOrEmpty (rect))
                return default;

            var parts = rect.Split (',');
            if (parts.Length != 4)
                throw new ArgumentException (
                    "must have four components separated by ','",
                    nameof (rect));

            return new Rect (
                double.Parse (parts [0]),
                double.Parse (parts [1]),
                double.Parse (parts [2]),
                double.Parse (parts [3]));
        }
    }
}