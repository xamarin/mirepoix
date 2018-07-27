//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

using Xamarin.Downloader;

namespace Xamarin.Downloader.Tests
{
    public class DownloadClientTests
    {
        sealed class TestDownloadServer
        {
            readonly HttpListener httpListener;

            public string BaseUri { get; } = "http://localhost:8282/";

            public TestDownloadServer ()
            {
                var startWait = new ManualResetEvent (false);

                httpListener = new HttpListener ();
                httpListener.Prefixes.Add (BaseUri);
                httpListener.Start ();

                ThreadPool.QueueUserWorkItem (async o => {
                    startWait.Set ();

                    var buffer = new byte [512 * 1024 * 1024];
                    new Random ().NextBytes (buffer);

                    try {
                        while (true) {
                            var context = await httpListener.GetContextAsync ();
                            context.Response.StatusCode = 200;
                            context.Response.ContentLength64 = buffer.Length;
                            await context.Response.OutputStream.WriteAsync (buffer, 0, buffer.Length);
                            await context.Response.OutputStream.FlushAsync ();
                            context.Response.OutputStream.Close ();
                            context.Response.Close ();
                        }
                    } catch {
                        return;
                    }
                });

                startWait.WaitOne ();
            }
        }

        sealed class TestDownloadClientDelegate : DownloadClientDelegate
        {
        }

        readonly DownloadClient downloader = DownloadClient.Create<TestDownloadClientDelegate> (
            outputDirectory: Path.Combine (
                Path.GetTempPath (),
                "com.xamarin.mirepoix.tests",
                Guid.NewGuid ().ToString ()));

        [Fact]
        public async Task DownloadBytesAsync ()
        {
            var server = new TestDownloadServer ();
            await downloader.DownloadAsync (server.BaseUri);
        }
    }
}