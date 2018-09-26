// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

using Xunit;

namespace Xamarin
{
    public sealed class FileHelperTests
    {
        [Theory]
        [InlineData ("", null)]
        [InlineData ("Hello World", null)]
        [InlineData ("\n", "\n")]
        [InlineData ("\r", "\r")]
        [InlineData ("\r\n", "\r\n")]
        [InlineData ("\n\r", "\n")]
        [InlineData ("Hello\nWorld", "\n")]
        [InlineData ("Hello\rWorld", "\r")]
        [InlineData ("Hello\r\nWorld", "\r\n")]
        [InlineData ("Hello\n\rWorld", "\n")]
        public void DetectFileLineEnding (string fileContents, string expectedLineEnding)
        {
            var stream = new MemoryStream ();
            var writer = new StreamWriter (stream);
            writer.Write (fileContents);
            writer.Flush ();
            stream.Position = 0;
            var reader = new StreamReader (stream);
            Assert.Equal (expectedLineEnding, FileHelpers.DetectFileLineEnding (reader));
        }
    }
}