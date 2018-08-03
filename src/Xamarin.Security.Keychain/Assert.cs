//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.Security
{
    static class ArgumentAssert
    {
        public static void IsNotNullOrEmpty (string argument, string parameterName)
        {
            if (argument == null)
                throw new ArgumentNullException (parameterName);

            if (argument == string.Empty)
                throw new ArgumentException (
                    "empty string is not allowed",
                    parameterName);
        }

        public static void IsNotNullOrEmpty (byte [] argument, string parameterName)
        {
            if (argument == null)
                throw new ArgumentNullException (parameterName);

            if (argument.Length == 0)
                throw new ArgumentException (
                    "empty byte array is not allowed",
                    parameterName);
        }

        public static void IsNotNull<T> (T argument, string parameterName)
            where T : class
        {
            if (argument == null)
                throw new ArgumentNullException (parameterName);
        }
    }
}