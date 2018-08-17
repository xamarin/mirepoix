//
// Locale.cs
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2017 Microsoft. All rights reserved.

namespace Mono.Security.Cryptography
{
	static class Locale
	{
		public static string GetText (string message, params object [] args)
			=> string.Format (message, args);
	}
}