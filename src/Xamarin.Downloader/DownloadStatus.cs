//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.Downloader
{
    /// <summary>
    /// Immutable record representing the status of a download at a point in time.
    /// </summary>
    public sealed class DownloadStatus
    {
        /// <summary>
        /// The total number of bytes expected to be downloaded.
        /// </summary>
        public long BytesExpected { get; }

        /// <summary>
        /// The total number of bytes already downloaded.
        /// </summary>
        public long BytesDownloaded { get; }

        /// <summary>
        /// The completion percentage of the download (0-1 inclusive).
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// The current rolling average speed of the download in bytes per second.
        /// </summary>
        public int BytesPerSecond { get; }

        /// <summary>
        /// The estimated time remaining to complete the download.
        /// </summary>
        public TimeSpan EstimatedTimeRemaining { get; }

        /// <summary>
        /// The amount of time taken to perform the download so far. If <see cref="Progress" />
        /// is 1 (completed), this value is the total amount of time the download took.
        /// </summary>
        public TimeSpan TimeElapsed { get; }

        /// <summary>
        /// When the download was started.
        /// </summary>
        public DateTimeOffset StartTime { get; }

        internal DownloadStatus (
            long bytesExpected,
            long bytesDownloaded,
            float progress,
            int bytesPerSecond,
            TimeSpan estimatedTimeRemaining,
            DateTimeOffset startTime)
        {
            BytesExpected = bytesExpected;
            BytesDownloaded = bytesDownloaded;
            Progress = progress;
            BytesPerSecond = bytesPerSecond;
            EstimatedTimeRemaining = estimatedTimeRemaining;
            TimeElapsed = DateTimeOffset.UtcNow - startTime;
            StartTime = startTime;
        }

        internal DownloadStatus WithCompletion ()
            => new DownloadStatus (
                BytesExpected,
                BytesExpected,
                1,
                0,
                TimeSpan.Zero,
                StartTime);
    }
}