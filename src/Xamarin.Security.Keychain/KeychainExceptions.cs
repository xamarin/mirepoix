//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.Security
{
    public abstract class KeychainException : Exception
    {
        internal KeychainException (string message, Exception innerException = null)
            : base (message, innerException)
        {
        }
    }

    public sealed class KeychainItemNotFoundException : KeychainException
    {
        internal KeychainItemNotFoundException (string message, Exception innerException = null)
            : base (message, innerException)
        {
        }
    }
}