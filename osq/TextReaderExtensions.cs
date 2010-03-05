using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq {
    static class TextReaderExtensions {
        public static void SkipWhitespace(this LocatedTextReaderWrapper input) {
            while(input.Peek() >= 0 && char.IsWhiteSpace((char)input.Peek())) {
                input.Location.AdvanceCharacter((char)input.Read());
            }
        }
    }
}
