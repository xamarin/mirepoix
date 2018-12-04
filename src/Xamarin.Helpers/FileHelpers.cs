// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Linq;

namespace Xamarin
{
    public static class FileHelpers
    {
        /// <summary>
        /// Returns either <c>&quot;\r\n&quot;</c> (Windows CRLF ending), <c>&quot;\n&quot;</c> (Unix LF ending),
        /// <c>&quot;\r&quot;</c> (Legacy Mac CR ending), or <c>null</c> (no endings detected) based on the first
        /// occurrence of a valid line ending in the file.
        /// </summary>
        public static string DetectFileLineEnding (string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException (nameof (fileName));

            using (var reader = new StreamReader (fileName))
                return DetectFileLineEnding (reader);
        }

        [EditorBrowsable (EditorBrowsableState.Never)]
        public static string DetectFileLineEnding (StreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException (nameof (reader));

            string lineEnding = null;

            while (true) {
                var c = (char)reader.Read ();
                switch (c) {
                case '\r':
                    lineEnding = "\r";
                    break;
                case '\n':
                    return lineEnding + "\n";
                case char.MaxValue:
                    return lineEnding;
                default:
                    if (lineEnding != null)
                        return lineEnding;
                    break;
                }
            }
        }

        public static bool FileContentsAreEqual (string file1, string file2)
        {
            if (file1 == null)
                throw new ArgumentNullException (nameof (file1));

            if (file2 == null)
                throw new ArgumentNullException (nameof (file2));

            var fullFilePath1 = PathHelpers.ResolveFullPath (file1);
            var fullFilePath2 = PathHelpers.ResolveFullPath (file2);

            // fast: same contents if the paths are identical
            if (fullFilePath1 == fullFilePath2)
                return true;

            // fast: not the same contents if lengths differ on disk
            if (new FileInfo (fullFilePath1).Length != new FileInfo (fullFilePath2).Length)
                return false;

            // slow: actually compare contents one stream read buffer at a time
            using (var stream1 = File.OpenRead (fullFilePath1))
            using (var stream2 = File.OpenRead (fullFilePath2))
                return StreamContentsAreEqual (stream1, stream2);
        }

        public static bool StreamContentsAreEqual (Stream stream1, Stream stream2)
        {
            if (stream1 == null)
                throw new ArgumentNullException (nameof (stream1));

            if (stream2 == null)
                throw new ArgumentNullException (nameof (stream2));

            const int bufferSize = 4096;
            var buffer1 = new byte [bufferSize];
            var buffer2 = new byte [bufferSize];

            while (true) {
                var read1 = stream1.Read (buffer1, 0, buffer1.Length);
                var read2 = stream2.Read (buffer2, 0, buffer2.Length);
                if (read1 != read2)
                    return false;

                if (read1 <= 0)
                    return true;

                if (!buffer1.SequenceEqual (buffer2, 0, read1))
                    return false;
            }
        }
    }
}