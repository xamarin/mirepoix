//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Xamarin.NativeHelpers
{
    public static class Dlfcn
    {
        [DllImport ("libc")]
		public static extern int dlclose (IntPtr handle);

        [DllImport ("libc")]
		public static extern IntPtr dlopen (string path, int mode);

		[DllImport ("libc")]
		public static extern IntPtr dlsym (IntPtr handle, string symbol);

        public static IntPtr ReadIntPtr (IntPtr handle, string symbol)
            => Marshal.ReadIntPtr (dlsym (handle, symbol));
    }
}