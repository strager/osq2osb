using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq {
    static class TextReaderExtensions {
        public static void SkipWhitespace(this TextReader input) {
            while(input.Peek() >= 0 && char.IsWhiteSpace((char)input.Peek())) {
                // Ignore character.
                input.Read();
            }
        }

        public static void SkipWhitespace(this TextReader input, Parser.Location location) {
            while(input.Peek() >= 0 && char.IsWhiteSpace((char)input.Peek())) {
                location.AdvanceCharacter((char)input.Read());
            }
        }
    }
}
