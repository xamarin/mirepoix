// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Xamarin
{
    public static class HashHelpers
    {
        const int factor = unchecked ((int)0xa5555529);

        public static int Hash (int newKey, int currentKey)
            => unchecked ((currentKey * factor) + newKey);

        public static int Hash (bool newKeyPart, int currentKey)
            => Hash (currentKey, newKeyPart ? 1 : 0);

        /// <summary>
        /// Warning: this will box enum types.
        /// </summary>
        public static int Hash<T> (T newKeyPart, int currentKey) where T : class
        {
            var hash = unchecked (currentKey * factor);
            return newKeyPart == null ? hash :  unchecked (hash + newKeyPart.GetHashCode ());
        }

        public static int Hash (params int [] values)
        {
            if (values?.Length == 0)
                return 0;

            var hash = 1;
            for (var i = 0; i < values.Length; i++)
                hash = unchecked (hash * factor + values [i]);
            return hash;
        }

        public static unsafe int Hash (params double [] values)
        {
            if (values?.Length == 0)
                return 0;

            var hash = 1;
            for (var i = 0; i < values.Length; i++) {
                var dv = values [i];
                var lv = *((long*)&dv);
                var iv = (int)(lv & 0xffffffff) ^ (int)(lv >> 32);
                hash = unchecked (hash * factor + iv);
            }
            return hash;
        }

        /// <summary>
        /// Warning: this will box value/enum types!
        /// </summary>
        public static int Hash (params object [] values)
        {
            if (values?.Length == 0)
                return 0;

            var hash = 1;
            for (var i = 0; i < values.Length; i++) {
                if (values [i] != null)
                    hash = unchecked (hash * factor + values [i].GetHashCode ());
            }
            return hash;
        }
    }
}