// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;

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
                return DetectFileLineEndings (reader);
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
    }
}