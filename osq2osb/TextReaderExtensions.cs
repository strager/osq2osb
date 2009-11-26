using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb {
    static class TextReaderExtensions {
        public static void SkipWhitespace(this TextReader input) {
            while(input.Peek() >= 0 && char.IsWhiteSpace((char)input.Peek())) {
                // Ignore character.
                input.Read();
            }
        }
    }
}
