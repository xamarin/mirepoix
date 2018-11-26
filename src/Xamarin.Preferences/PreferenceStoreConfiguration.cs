//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Xamarin.Preferences
{
    public sealed class PreferenceStoreConfiguration
    {
        readonly string macosAppDomain;

        readonly RegistryHive windowsRegistryHive;
        readonly RegistryView windowsRegistryView;
        readonly string windowsRegistrySubKey;

        readonly bool memoryStoreFallback;

        public PreferenceStoreConfiguration ()
        {
        }

        PreferenceStoreConfiguration (
            string macosAppDomain,
            RegistryHive windowsRegistryHive,
            RegistryView windowsRegistryView,
            string windowsRegistrySubKey,
            bool memoryStoreFallback)
        {
            this.macosAppDomain = macosAppDomain;

            this.windowsRegistryHive = windowsRegistryHive;
            this.windowsRegistryView = windowsRegistryView;
            this.windowsRegistrySubKey = windowsRegistrySubKey;

            this.memoryStoreFallback = memoryStoreFallback;
        }

        public PreferenceStoreConfiguration WithMac (string macosAppDomain)
            => new PreferenceStoreConfiguration (
                macosAppDomain,
                this.windowsRegistryHive,
                this.windowsRegistryView,
                this.windowsRegistrySubKey,
                this.memoryStoreFallback);

        public PreferenceStoreConfiguration WithWindows (
            string registrySubKey,
            RegistryHive registryHive = RegistryHive.CurrentUser,
            RegistryView registryView = RegistryView.Default)
            => new PreferenceStoreConfiguration (
                this.macosAppDomain,
                registryHive,
                registryView,
                registrySubKey,
                this.memoryStoreFallback);
        
        public PreferenceStoreConfiguration WithMemoryFallback (
            bool memoryFallback)
            => new PreferenceStoreConfiguration (
                this.macosAppDomain,
                this.windowsRegistryHive,
                this.windowsRegistryView,
                this.windowsRegistrySubKey,
                memoryFallback);

        public IPreferenceStore Create ()
        {
            if (macosAppDomain != null && RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
                return new MemoryOnlyPreferenceStore ();

            if (windowsRegistrySubKey != null && RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
                return new RegistryPreferenceStore (
                    windowsRegistryHive,
                    windowsRegistryView,
                    windowsRegistrySubKey);
            
            if (memoryStoreFallback)
                return new MemoryOnlyPreferenceStore ();

            throw new PlatformNotSupportedException (
                "Either the platform is not supported or the configuration has not been " +
                "specified appropriately for the current platform.");
        }

        public void CreateAndInitialize ()
            => PreferenceStore.Initialize (Create ());
    }
}