//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit
{
    public class GB18030TestDataWithNullAndEmpty : IEnumerable<object []>
    {
        static readonly List<Object []> testStrings = new List<object []> {
            new [] { "Null", null },
            new [] { "Empty", string.Empty }
        };

        static GB18030TestDataWithNullAndEmpty ()
            => testStrings.AddRange (GB18030TestData.testStrings);

        public virtual IEnumerator<object []> GetEnumerator ()
            => testStrings.GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator ()
            => GetEnumerator ();
    }

    public class GB18030TestData : IEnumerable<object []>
    {
        // Test data adapted from Workbooks:
        // https://github.com/Microsoft/workbooks/blob/master/Tests/Workbooks/Regression/GB18030.workbook
        internal static readonly List<object []> testStrings = new List<object []> {
            new [] { "Single Byte", "!\"#)6=@Aa}~" },
            new [] { "Double Byte", "啊齄丂狛狜隣郎隣兀﨩ˊ▇█〞〡¦TEL(株)‐ー*+@、〓ix1.€(一)(十)IXII!¯ぁんァヶΑ_АЯаяāɡㄅㄩ─╋(』【—__ixɑ ɡ〇〾⿻⺁ 䜣 €" },
            new [] { "Four byte (Ext-A)", "㐀㒣㕴㕵㙉㙊䵯䵰䶴䶵" },
            new [] { "Four byte (Ext-B, Optional, not supported on macOS out of the box)", "𪛖𪛕𪛔𪛓𪛒𪛑𠀃𠀂𠀁𠀀" },
            new [] { "Four byte (Mongolian)", "᠀᠐᠙ᠠᡷᢀᡨᡩᡪᡫ" },
            new [] { "Four byte (Tibetan)", "ༀཇཉཪཱྋ྾࿌࿏ྼྼ" },
            new [] { "Four byte (Yi)", "ꀀ ꒌ ꂋ ꂌ ꂍ ꂎ ꂔ ꂕ ꒐ ꓆" },
            new [] { "Four byte (Uighur)", "پپڭیئبلإلا،؟ئبتجدرشعە" },
            new [] { "Four byte (Tai Le)", "ᥐᥥᥦᥧᥨᥭᥰᥱᥲᥴ" },
            new [] { "Four byte (Hangul)", "ᄓᄕᇬᇌᇜᇱ기가힝" },
            new [] { "Emoji", "🥑🌮🍔🐈" }
        };

        public IEnumerator<object []> GetEnumerator ()
            => testStrings.GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator ()
            => GetEnumerator ();
    }
}