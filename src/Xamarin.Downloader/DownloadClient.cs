//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Downloader
{
    public sealed class DownloadClient
    {
        public static class DefaultOptions
        {
            public const bool UseCache = true;
            public const bool RetryOnDnsFailure = true;
            public const int MaxRetryCount = 5;
            public const int ProgressPrecision = 2;
        }

        public static DownloadClient Create<TDownloadClientDelegate> (
            string outputDirectory = null,
            string outputFileName = null,
            IReadOnlyList<(string name, string value)> requestHeaders = null,
            bool useCache = DefaultOptions.UseCache,
            bool retryOnDnsFailure = DefaultOptions.RetryOnDnsFailure,
            int maxRetryCount = DefaultOptions.MaxRetryCount,
            int progressPrecision = DefaultOptions.ProgressPrecision)
            where TDownloadClientDelegate : DownloadClientDelegate, new ()
            => new DownloadClient (
                new HttpClient (),
                () => new TDownloadClientDelegate (),
                outputDirectory,
                outputFileName,
                requestHeaders ?? Array.Empty<(string, string)> (),
                useCache,
                retryOnDnsFailure,
                maxRetryCount,
                progressPrecision);

        public static DownloadClient Create (
            string outputDirectory = null,
            string outputFileName = null,
            IReadOnlyList<(string name, string value)> requestHeaders = null,
            bool useCache = DefaultOptions.UseCache,
            bool retryOnDnsFailure = DefaultOptions.RetryOnDnsFailure,
            int maxRetryCount = DefaultOptions.MaxRetryCount,
            int progressPrecision = DefaultOptions.ProgressPrecision)
            => DownloadClient.Create<DownloadClientDelegate> (
                outputDirectory,
                outputFileName,
                requestHeaders,
                useCache,
                retryOnDnsFailure,
                maxRetryCount,
                progressPrecision);

        readonly HttpClient httpClient;
        readonly Func<DownloadClientDelegate> downloadDelegateFactory;

        public string OutputDirectory { get; }
        public string OutputFileName { get; }
        public IReadOnlyList<(string name, string value)> RequestHeaders { get; }
        public bool UseCache { get; }
        public bool RetryOnDnsFailure { get; }
        public int MaxRetryCount { get; }
        public int ProgressPrecision { get; }

        DownloadClient (
            HttpClient httpClient,
            Func<DownloadClientDelegate> downloadDelegateFactory,
            string outputDirectory,
            string outputFileName,
            IReadOnlyList<(string name, string value)> requestHeaders,
            bool useCache,
            bool retryOnDnsFailure,
            int maxRetryCount,
            int progressPrecision)
        {
            this.httpClient = httpClient;
            this.downloadDelegateFactory = downloadDelegateFactory;
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;
            RequestHeaders = requestHeaders;
            UseCache = useCache;
            RetryOnDnsFailure = retryOnDnsFailure;
            MaxRetryCount = maxRetryCount;
            ProgressPrecision = progressPrecision;
        }

        public Task<Download> DownloadAsync (
            string requestUri,
            IEnumerable<(string name, string value)> requestHeaders = null,
            string outputDirectory = null,
            string outputFileName = null,
            CancellationToken cancellationToken = default)
            => DownloadAsync (
                Download.Create (
                    requestUri ?? throw new ArgumentNullException (nameof (requestUri)),
                    requestHeaders,
                    outputDirectory,
                    outputFileName),
                cancellationToken);

        sealed class RetryDownloadException : Exception
        {
            public RetryDownloadException (string message, Exception innerException)
                : base (message, innerException)
            {
            }
        }

        async Task<Download> DownloadAsync (
            Download download,
            CancellationToken cancellationToken)
        {
            var downloadDelegate = downloadDelegateFactory ();
            var downloadStartTime = DateTimeOffset.UtcNow;

            for (int i = 0; i < MaxRetryCount; i ++) {
                try {
                    try {
                        return await DownloadAsync (
                            downloadDelegate,
                            downloadStartTime,
                            download,
                            cancellationToken);
                    } catch (IOException e) {
                        throw new RetryDownloadException ("Unable to read from HTTP stream", e);
                    }
                } catch (RetryDownloadException e) {
                    // retry up to MaxRetryCount for this URI with exponential backoff
                    downloadDelegate.NotifyDownloadAttemptFailed (
                        download,
                        e,
                        i + 1,
                        MaxRetryCount);

                    if (i + 1 == MaxRetryCount)
                        throw;
                }

                await Task.Delay (
                    TimeSpan.FromMilliseconds (Math.Pow (2, i) * 100),
                    cancellationToken);
            }

            // This should never be reached but exists to placate the compiler
            throw new Exception ($"Could not successfully download {download.RequestUri}");
        }

        async Task<Download> DownloadAsync (
            DownloadClientDelegate downloadDelegate,
            DateTimeOffset downloadStartTime,
            Download download,
            CancellationToken cancellationToken = default)
        {
            const int bufferSize = 16 * 1024;

            download = downloadDelegate.PrepareDownload (this, download);

            // if the item is already downloaded, use it
            bool IsFileAlreadyDownloaded ()
                => UseCache && download.OutputFullPath != null && File.Exists (download.OutputFullPath);

            if (IsFileAlreadyDownloaded ())
                return download;

            // if the item is already downloaded, use it
            if (UseCache && File.Exists (download.OutputFullPath))
                return download;

            DownloadClientDelegate.TryGetIntermediateFileSize (download, out var totalRead);
            long totalDownloaded = 0;

            var sampleTime = TimeSpan.FromSeconds (1);
            long lastTimerRead = 0;
            double averageSpeed = 0;
            double lastSpeed = 0;

            void TimerCallback (object state)
            {
                const double smoothing = 0.005;

                var _totalDownloaded = totalDownloaded;

                lastSpeed = _totalDownloaded - lastTimerRead;

                averageSpeed = lastTimerRead == 0
                    ? lastSpeed
                    : smoothing * lastSpeed + (1 - smoothing) * averageSpeed;

                lastTimerRead = _totalDownloaded;
            }

            using (var timer = new Timer (TimerCallback, null, sampleTime, sampleTime))
            using (var response = await GetResponseAsync (downloadDelegate, download, cancellationToken)) {
                var actualUri = response.Headers.Location?.OriginalString ?? download.RequestUri;

                if (download.OutputFileName == null)
                    download = download.WithOutputFileName (Path.GetFileName (actualUri));

                if (download.OutputFullPath == null)
                    throw new InvalidOperationException (
                        $"{nameof (Download)}.{nameof (Download.OutputFullPath)} could not be constructed");

                // check again if the item is already downloaded in the case of an HTTP redirect
                // that indicates the "real" file name, which cannot be known from our input URI
                if (IsFileAlreadyDownloaded ())
                    return download;

                File.Delete (download.OutputFullPath);

                var fileMode = FileMode.Create;
                var totalExpected = response.Content.Headers.ContentLength.GetValueOrDefault ();
                if (response.StatusCode == HttpStatusCode.PartialContent && totalRead > 0) {
                    totalExpected += totalRead;
                    fileMode = FileMode.Append;
                }

                var status = new DownloadStatus (
                    totalExpected,
                    0,
                    0,
                    0,
                    Timeout.InfiniteTimeSpan,
                    downloadStartTime);

                downloadDelegate.NotifyDownloadStatus (download, status);

                Directory.CreateDirectory (download.OutputDirectory);

                using (var sourceStream = await response.Content.ReadAsStreamAsync ().ConfigureAwait (false))
                using (var outputStream = File.Open (
                    download.OutputIntermediateFullPath,
                    fileMode,
                    FileAccess.Write,
                    FileShare.None)) {
                    cancellationToken.ThrowIfCancellationRequested ();

                    var buffer = new byte [bufferSize];
                    int read;

                    while ((read = await sourceStream
                        .ReadAsync (buffer, 0, buffer.Length, cancellationToken)
                        .ConfigureAwait (false)) != 0) {
                        cancellationToken.ThrowIfCancellationRequested ();

                        await outputStream
                            .WriteAsync (buffer, 0, read, cancellationToken)
                            .ConfigureAwait (false);

                        totalRead += read;
                        totalDownloaded += read;

                        var newProgress = (float)Math.Round (
                            totalRead / (double)totalExpected,
                            ProgressPrecision);

                        var timeRemaining = TimeSpan.Zero;
                        if (averageSpeed > 0)
                            timeRemaining = TimeSpan.FromSeconds (
                                (totalExpected - totalRead) / averageSpeed);

                        if (newProgress != status.Progress) {
                            status = new DownloadStatus (
                                totalExpected,
                                totalDownloaded,
                                newProgress,
                                (int)Math.Round (averageSpeed),
                                timeRemaining,
                                downloadStartTime);
                            downloadDelegate.NotifyDownloadStatus (download, status);
                        }
                    }

                    await outputStream
                        .FlushAsync (cancellationToken)
                        .ConfigureAwait (false);
                }

                try {
                    if (!DownloadClientDelegate.TryGetIntermediateFileSize (download, out var fileLength))
                        throw new Exception ("unable to determine the size of " +
                            download.OutputIntermediateFullPath);

                    if (totalExpected > 0 && fileLength != (long)totalExpected)
                        throw new Exception (
                            $"expected {totalExpected} bytes for {download.OutputFullPath} " +
                            $"but size on disk is {fileLength} bytes");
                } catch {
                    File.Delete (download.OutputIntermediateFullPath);
                    throw;
                }

                File.Move (download.OutputIntermediateFullPath, download.OutputFullPath);

                downloadDelegate.NotifyDownloadStatus (
                    download,
                    status.WithCompletion ());

                return download;
            }
        }

        async Task<HttpResponseMessage> GetResponseAsync (
            DownloadClientDelegate downloadDelegate,
            Download download,
            CancellationToken cancellationToken)
        {
            bool ShouldRetryForCurlError (int curlError)
            {
                switch (curlError) {
                case 6: // CURLE_COULDNT_RESOLVE_HOST
                    return RetryOnDnsFailure;
                default:
                    return true;
                }
            }

            var requestUri = new Uri (download.RequestUri);

            var redirectAttempts = -1;
            while (redirectAttempts++ < 5) {
                var httpRequest = new HttpRequestMessage (
                    HttpMethod.Get,
                    requestUri);

                foreach (var header in download.RequestHeaders)
                    httpRequest.Headers.Add (header.name, header.value);

                HttpResponseMessage response;
                try {
                    response = await httpClient.SendAsync (
                        httpRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                } catch (Exception e) when (
                    e.InnerException != null &&
                    !ShouldRetryForCurlError (e.InnerException.HResult) &&
                    e.InnerException?.GetType ().FullName == "System.Net.Http.CurlException") {
                    throw;
                } catch (Exception e) {
                    throw new RetryDownloadException ("Failed to send HTTP request", e);
                }

                cancellationToken.ThrowIfCancellationRequested ();

                await downloadDelegate.HandleHttpResponseMessageAsync (
                    download,
                    response,
                    cancellationToken).ConfigureAwait (false);

                response.EnsureSuccessStatusCode ();
                response.Headers.Add ("Location", requestUri.AbsoluteUri);

                return response;
            }

            throw new HttpRequestException ($"too many redirects for URI: {requestUri}");
        }
    }
}