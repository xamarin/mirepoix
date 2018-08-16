// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Xamarin.MSBuild.Tooling
{
    public static class MSBuildLocator
    {
        static readonly string processFileName;
        static readonly bool isMono;
        static readonly bool isNetCore;
        static readonly string netCoreSdkPath;

        // check our own references since Microsoft.Build should not have been
        // loaded into the app domain yet (and we don't want to cause it to
        // load yet either since we need to hook into AssemblyResolve)
        static readonly Version referencedMSBuildVersion;
        static readonly string referencedMSBuildMajorDirectoryVersion;

        static MSBuildLocator ()
        {
            processFileName = Process.GetCurrentProcess ().MainModule.FileName;

            isMono = Type.GetType ("Mono.Runtime") != null;

            if (!isMono) {
                var netCoreInfo = GetNetCoreInfo ();
                isNetCore = netCoreInfo.runtimeVersion != null;
                netCoreSdkPath = netCoreInfo.sdkPath;
            }

            referencedMSBuildVersion = Assembly
                .GetExecutingAssembly ()
                .GetReferencedAssemblies ()
                .FirstOrDefault (IsMSBuildAssembly)
                ?.Version;

            referencedMSBuildMajorDirectoryVersion = referencedMSBuildVersion == null
                ? null
                : $"{referencedMSBuildVersion.Major}.0";
        }

        static (Version runtimeVersion, Version sdkVersion, string sdkPath) GetNetCoreInfo ()
        {
            var targetFrameworkName = Assembly
                .GetEntryAssembly ()
                ?.GetCustomAttribute<TargetFrameworkAttribute> ()
                ?.FrameworkName;

            if (targetFrameworkName == null)
                return default;

            var targetFramework = new FrameworkName (targetFrameworkName);
            if (targetFramework.Identifier != ".NETCoreApp" || targetFramework.Version == null)
                return default;

            // The .NET Core framework version at least for now happens to be
            // aligned with the runtime version, for at least the major and
            // minor components, which is what we need to map to the SDK version,
            // so just roll with it. Would like a better way of getting the
            // actual runtime version...
            var runtimeVersion = targetFramework.Version;

            // now attempt to locate an SDK; this should probably be expanded
            // to support RID-published apps as well since the process that's
            // running will be the native app itself, and not 'dotnet' from
            // the system.
            if (Path.GetFileNameWithoutExtension (processFileName) != "dotnet")
                return (runtimeVersion, null, null);

            var sdkPath = Path.Combine (Path.GetDirectoryName (processFileName), "sdk");
            if (!Directory.Exists (sdkPath))
                return (runtimeVersion, null, null);

            // The major and minor components from the runtime version
            // align with the major and minor components from SDKs.
            // Look for matching installed SDKs and pick the newest one.
            // cf. https://docs.microsoft.com/en-us/dotnet/core/versions/
            var sdkVersion = new DirectoryInfo (sdkPath)
                .EnumerateDirectories ()
                .Select (dir => {
                    Version.TryParse (dir.Name, out var version);
                    return version;
                })
                .Where (dirVersion => dirVersion != null &&
                    dirVersion.Major == runtimeVersion.Major &&
                    dirVersion.Minor == runtimeVersion.Minor)
                .OrderByDescending (dirVersion => dirVersion)
                .FirstOrDefault ();

            if (sdkVersion == null)
                return (runtimeVersion, null, null);

            return (
                runtimeVersion,
                sdkVersion,
                Path.Combine (sdkPath, sdkVersion.ToString ()));
        }

        public static void RegisterMSBuildPath (string msbuildExePath = null)
        {
            if (referencedMSBuildMajorDirectoryVersion == null)
                throw new FileNotFoundException (
                    $"Unable to determine the version of Microsoft.Build " +
                    $"referenced by {typeof (MSBuildLocator).Assembly.FullName}");

            msbuildExePath = msbuildExePath ?? Environment.GetEnvironmentVariable ("MSBUILD_EXE_PATH");

            if (string.IsNullOrEmpty (msbuildExePath)) {
                if (isMono)
                    msbuildExePath = LocateMonoMSBuild ();
                else if (isNetCore && netCoreSdkPath != null)
                    msbuildExePath = Path.Combine (netCoreSdkPath, "MSBuild.dll");
                else {
                    var vsInstallDir = Environment.GetEnvironmentVariable ("VSINSTALLDIR");
                    if (vsInstallDir != null && Directory.Exists (vsInstallDir))
                        msbuildExePath = Path.Combine (
                            vsInstallDir,
                            "MSBuild",
                            referencedMSBuildMajorDirectoryVersion,
                            "Bin",
                            "MSBuild.exe");
                }

                if (msbuildExePath != null)
                    Environment.SetEnvironmentVariable ("MSBUILD_EXE_PATH", msbuildExePath);
            }

            if (!File.Exists (msbuildExePath))
                throw new PlatformNotSupportedException (
                    $"Could not locate a standalone MSBuild installation ({msbuildExePath ?? "(null)"}). " +
                    $"Consider passing a path to MSBuild.exe/dll when calling {nameof (RegisterMSBuildPath)} or " +
                    $"setting the MSBUILD_EXE_PATH environment variable.");

            // Explicitly set the environment variable so MSBuild's BuildEnvironmentHelper
            // skips directly to its standalone mode support.
            Environment.SetEnvironmentVariable ("MSBUILD_EXE_PATH", msbuildExePath);

            // MSBuild will try to load various assemblies but the runtime doesn't know from where,
            // so explicitly try to load them from the standalone install directory.
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
                var assemblyName = new AssemblyName (e.Name);
                var assemblyPath = Path.GetDirectoryName (msbuildExePath);

                if (!IsMSBuildAssembly (assemblyName))
                    return null;

                if (assemblyName.Name == "Microsoft.Build.resources") {
                    foreach (var culture in GetCurrentCultureHierarchy ("en")) {
                        var culturePath = Path.Combine (assemblyPath, culture, assemblyName.Name + ".dll");
                        if (File.Exists (culturePath)) {
                            assemblyPath = culturePath;
                            break;
                        }
                    }
                } else {
                    assemblyPath = Path.Combine (assemblyPath, assemblyName.Name + ".dll");
                }

                if (!File.Exists (assemblyPath))
                    return null;

                return Assembly.LoadFrom (assemblyPath);
            };
        }

        static IEnumerable<string> GetCurrentCultureHierarchy (params string [] fallbackCultures)
        {
            var culture = CultureInfo.CurrentCulture;
            while (culture != null && culture.Parent != culture) {
                yield return culture.Name;
                culture = culture.Parent;
            }

            foreach (var fallbackCulture in fallbackCultures)
                yield return fallbackCulture;
        }

        static string LocateMonoMSBuild ()
        {
            // should end up with something like /Library/Frameworks/Mono.framework/Versions/5.16.0
            var monoRoot = new FileInfo (processFileName)
                .Directory
                .Parent
                .FullName;

            var msbuildRoot = Path.Combine (
                monoRoot,
                "lib",
                "mono",
                "msbuild",
                referencedMSBuildMajorDirectoryVersion,
                "bin",
                "MSBuild.dll");

            return msbuildRoot;
        }

        #region Adapted from the full MSBuildLocator

        static readonly string [] msbuildAssemblyNames = {
            "Microsoft.Build",
            "Microsoft.Build.Framework",
            "Microsoft.Build.Tasks.Core",
            "Microsoft.Build.Utilities.Core",
            "Microsoft.Build.resources"
        };

        static bool IsMSBuildAssembly (AssemblyName assemblyName)
        {
            const ulong msbuildPublicKeyToken = 0xb03f5f7f11d50a3a;

            if (!msbuildAssemblyNames.Contains (assemblyName.Name, StringComparer.OrdinalIgnoreCase))
                return false;

            var publicKeyTokenBytes = assemblyName.GetPublicKeyToken ();
            if (publicKeyTokenBytes == null || publicKeyTokenBytes.Length == 0)
                return false;

            var publicKeyToken = BitConverter.ToUInt64 (publicKeyTokenBytes, 0);

            if (BitConverter.IsLittleEndian)
                publicKeyToken = Swap (publicKeyToken);

            return publicKeyToken == msbuildPublicKeyToken;

            ulong Swap (ulong value)
            {
                value = value >> 32 | value << 32;
                value = (value & 0xFFFF0000FFFF0000) >> 16 | (value & 0x0000FFFF0000FFFF) << 16;
                return (value & 0xFF00FF00FF00FF00) >> 8 | (value & 0x00FF00FF00FF00FF) << 8;
            }
        }

        #endregion
    }
}