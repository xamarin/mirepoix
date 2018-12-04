// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace Xamarin
{
    public sealed class FileHelperTests
    {
        readonly Random random = new Random ();

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

        public static IEnumerable<object []> GetStreamSizes ()
        {
            IEnumerable<int> GenerateSizes ()
            {
                for (int i = 0; i <= 33; i++)
                    yield return i;

                foreach (var p2 in new [] { 1024, 4096, 8192, 1048576 }) {
                    for (int i = 0; i <= 9; i++)
                        yield return p2 + i;
                }
            }

            foreach (var size in GenerateSizes ())
                yield return new object [] { size };
        }

        (byte [], byte []) RandomBufferWithCopy (int size)
        {
            var buffer1 = new byte [size];
            random.NextBytes (buffer1);

            var buffer2 = new byte [buffer1.Length];
            Array.Copy (buffer1, buffer2, buffer2.Length);

            return (buffer1, buffer2);
        }

        [Theory]
        [MemberData (nameof (GetStreamSizes))]
        public void StreamContentsAreEqual (int streamSize)
        {
            var (buffer1, buffer2) = RandomBufferWithCopy (streamSize);

            Assert.True (FileHelpers.StreamContentsAreEqual (
                new MemoryStream (buffer1),
                new MemoryStream (buffer2)));

            if (buffer2.Length > 0) {
                buffer2 [random.Next (0, buffer2.Length)]++;

                Assert.False (FileHelpers.StreamContentsAreEqual (
                    new MemoryStream (buffer1),
                    new MemoryStream (buffer2)));
            }
        }

        [Theory]
        [MemberData (nameof (GetStreamSizes))]
        public void FileContentsAreEqual (int streamSize)
        {
            var (buffer1, buffer2) = RandomBufferWithCopy (streamSize);

            var dir = Path.Combine (
                Path.GetTempPath (),
                "com.xamarin.mirepoix.tests",
                nameof (FileContentsAreEqual));

            Directory.CreateDirectory (dir);

            var path1 = Path.Combine (dir, Path.GetRandomFileName ());
            var path2 = Path.Combine (dir, Path.GetRandomFileName ());

            File.WriteAllBytes (path1, buffer1);
            File.WriteAllBytes (path2, buffer2);

            Assert.True (FileHelpers.FileContentsAreEqual (path1, path1));
            Assert.True (FileHelpers.FileContentsAreEqual (path2, path2));
            Assert.True (FileHelpers.FileContentsAreEqual (path1, path2));

            if (buffer2.Length > 0) {
                using (var stream = new FileStream (path2, FileMode.Open, FileAccess.ReadWrite)) {
                    stream.Seek (random.Next (0, (int)stream.Length), SeekOrigin.Begin);
                    var b = (byte)(stream.ReadByte () + 1);
                    stream.Seek (-1, SeekOrigin.Current);
                    stream.WriteByte (b);
                    stream.Flush ();
                }

                Assert.False (FileHelpers.FileContentsAreEqual (path1, path2));
            }
        }
    }
}