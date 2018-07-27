//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Downloader
{
    public class DownloadClientDelegate
    {
        internal static bool TryGetIntermediateFileSize (Download download, out long fileSize)
        {
            if (download.OutputIntermediateFullPath != null) {
                var downloadingFileInfo = new FileInfo (download.OutputIntermediateFullPath);
                if (downloadingFileInfo.Exists) {
                    fileSize = downloadingFileInfo.Length;
                    return true;
                }
            }

            fileSize = 0;
            return false;
        }

        internal protected virtual Download PrepareDownload (
            DownloadClient downloadClient,
            Download download)
        {
            download = download.WithOutputDirectory (
                download.OutputDirectory
                    ?? downloadClient.OutputDirectory
                    ?? Environment.CurrentDirectory);

            if (download.OutputFileName == null) {
                var fileName = Path.GetFileName (download.RequestUri);
                if (string.IsNullOrEmpty (fileName))
                    fileName = "UnknownDownload";
                download = download.WithOutputFileName (fileName);
            }

            var rangeHeaders = Array.Empty<(string, string)> ();
            if (TryGetIntermediateFileSize (download, out var totalRead))
                rangeHeaders = new [] {
                    ("Range", $"bytes={totalRead}-"),
                    ("x-ms-range", $"bytes={totalRead}-"),
                    ("x-ms-version", "2011-08-18")
                };

            download = download.WithRequestHeaders (downloadClient
                .RequestHeaders
                .Concat (download.RequestHeaders)
                .Concat (rangeHeaders));

            return download;
        }

        internal protected virtual void NotifyDownloadAttemptFailed (
            Download download,
            Exception exception,
            int attempt,
            int maxAttempts)
        {
        }

        internal protected virtual void NotifyDownloadStatus (
            Download download,
            DownloadStatus status)
        {
        }

        internal protected virtual Task HandleHttpResponseMessageAsync (
            Download download,
            HttpResponseMessage response,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}