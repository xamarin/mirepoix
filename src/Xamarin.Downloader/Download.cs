//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.Downloader
{
    public sealed class Download
    {
        public string RequestUri { get; }
        public IReadOnlyList<(string name, string value)> RequestHeaders { get; }
        public string OutputDirectory { get; }
        public string OutputFileName { get; }
        public string OutputFullPath { get; }
        internal string OutputIntermediateFullPath { get; }

        Download (
            string requestUri,
            IReadOnlyList<(string name, string value)> requestHeaders,
            string outputDirectory,
            string outputFileName)
        {
            RequestUri = requestUri;
            RequestHeaders = requestHeaders;
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;

            if (!string.IsNullOrEmpty (OutputDirectory) &&
                !string.IsNullOrEmpty (OutputFileName)) {
                OutputFullPath = Path.Combine (OutputDirectory, OutputFileName);
                OutputIntermediateFullPath = OutputFullPath + ".downloading";
            }
        }

        internal static Download Create (
            string requestUri,
            IEnumerable<(string name, string value)> requestHeaders,
            string outputDirectory,
            string outputFileName)
            => new Download (
                requestUri,
                requestHeaders?.ToArray () ?? Array.Empty<(string, string)> (),
                outputDirectory,
                outputFileName);

        internal Download WithRequestHeaders (IEnumerable<(string name, string value)> requestHeaders)
            => requestHeaders == RequestHeaders
                ? this
                : new Download (
                    RequestUri,
                    requestHeaders?.ToArray () ?? Array.Empty<(string, string)> (),
                    OutputDirectory,
                    OutputFileName);

        internal Download WithOutputDirectory (string outputDirectory)
            => outputDirectory == OutputDirectory
                ? this
                : new Download (
                    RequestUri,
                    RequestHeaders,
                    outputDirectory,
                    OutputFileName);

        internal Download WithOutputFileName (string outputFileName)
            => outputFileName == OutputFileName
                ? this
                : new Download (
                    RequestUri,
                    RequestHeaders,
                    OutputDirectory,
                    outputFileName);
    }
}