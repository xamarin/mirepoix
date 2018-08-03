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

    public sealed class KeychainItemAlreadyExistsException : KeychainException
    {
        internal KeychainItemAlreadyExistsException (string message, Exception innerException = null)
            : base (message, innerException)
        {
        }
    }
}