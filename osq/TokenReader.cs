using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace osq {
    public class TokenReader : ITokenReader {
        public LocatedTextReaderWrapper InputReader {
            get;
            set;
        }

        public Location CurrentLocation {
            get {
                return InputReader == null ? null : InputReader.Location.Clone();
            }
        }

        public TokenReader(LocatedTextReaderWrapper reader) {
            InputReader = reader;
        }

        private static bool IsStringStart(char c) {
            return c == '"';
        }

        private static bool IsNumberChar(char c) {
            return ".0123456789".Contains(c);
        }

        private static bool IsIdentifierChar(char c) {
            return "_".Contains(c) || char.IsLetterOrDigit(c);
        }

        private static bool IsIdentifierStart(char c) {
            return "_".Contains(c) || char.IsLetter(c);
        }

        private static bool IsSymbolChar(char c) {
            return "><=!+-*/%^(),:{}".Contains(c);
        }

        private static bool IsSymbolStart(string s) {
            return new string[] {
                "",
                "+", "-", "*", "/", "^", "%",
                ",", "(", ")",
                ">", "<", ">=", "<=",
                "=", "==", "!", "!=",
                ":", "{", "}",
            }.Contains(s);
        }

        private static bool IsWhiteSpace(char c) {
            return char.IsWhiteSpace(c);
        }

        public IEnumerable<Token> ReadTokens() {
            Token curToken;

            while((curToken = ReadToken()) != null) {
                yield return curToken;
            }
        }

        private Token peekedToken = null;

        public Token ReadToken() {
            if(peekedToken != null) {
                var ret = peekedToken;
                peekedToken = null;
                return ret;
            }

            return ReadTokenForced();
        }

        public Token PeekToken() {
            peekedToken = ReadTokenForced();
            return peekedToken;
        }

        private Token ReadTokenForced() {
            int nextCharacterCode = InputReader.Peek();

            if(nextCharacterCode < 0) {
                return null;
            }

            char nextCharacter = (char)nextCharacterCode;

            var startLocation = InputReader.Location.Clone();

            Token token;

            if(IsStringStart(nextCharacter)) {
                token = ReadString();
            } else if(IsNumberChar(nextCharacter)) {
                token = ReadNumber();
            } else if(IsIdentifierStart(nextCharacter)) {
                token = ReadIdentifier();
            } else if(IsSymbolChar(nextCharacter)) {
                token = ReadSymbol();
            } else if(IsWhiteSpace(nextCharacter)) {
                token = ReadWhiteSpace();
            } else {
                throw new BadDataException("Unknown token starting with " + nextCharacter, startLocation);
            }

            token.Location = startLocation;

            return token;
        }

        private Token ReadSymbol() {
            var curToken = "";

            while(InputReader.Peek() >= 0) {
                char nextCharacter = (char)InputReader.Peek();

                if(!IsSymbolChar(nextCharacter)) {
                    break;
                }

                string newToken = curToken + nextCharacter;

                if(!IsSymbolStart(newToken)) {
                    break;
                }

                curToken = newToken;

                InputReader.Read();   // Discard.
            }

            return new Token(TokenType.Symbol, curToken);
        }

        private Token ReadNumber() {
            var startLocation = InputReader.Location.Clone();

            var numberString = new StringBuilder();

            while(IsNumberChar((char)InputReader.Peek())) {
                numberString.Append((char)InputReader.Read());
            }

            double number;

            if(!double.TryParse(numberString.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Parser.DefaultCulture, out number)) {
                throw new BadDataException("Number", startLocation);
            }

            return new Token(TokenType.Number, number);
        }

        private Token ReadIdentifier() {
            var token = new StringBuilder();

            while(InputReader.Peek() >= 0) {
                char nextCharacter = (char)InputReader.Peek();

                if(!IsIdentifierChar(nextCharacter)) {
                    break;
                }

                token.Append(nextCharacter);

                InputReader.Read();   // Discard.
            }

            return new Token(TokenType.Identifier, token.ToString());
        }

        private Token ReadString() {
            var str = new StringBuilder();

            InputReader.Read(); // Consume ".

            while(true) {
                int rawChar = InputReader.Read();

                if(rawChar < 0) {
                    throw new MissingDataException("End-of-string terminator", InputReader.Location);
                }

                char nextCharacter = (char)rawChar;

                if(nextCharacter == '"') {
                    break;
                }

                if(nextCharacter == '\\') {
                    nextCharacter = ReadEscapeCode();
                }

                str.Append(nextCharacter);
            }

            return new Token(TokenType.String, str.ToString());
        }

        private char ReadEscapeCode() {
            int nextCharacter = InputReader.Read();

            if(nextCharacter < 0) {
                throw new MissingDataException("Code following \\", InputReader.Location);
            }

            switch((char)nextCharacter) {
                case 'n':
                    return '\n';

                case 't':
                    return '\t';

                case 'r':
                    return '\r';

                case '\\':
                    return '\\';

                case '"':
                    return '"';

                default:
                    var e = new BadDataException("Unknown escape character", InputReader.Location);
                    e.Data["character"] = (char)nextCharacter;
                    throw e;
            }
        }

        private Token ReadWhiteSpace() {
            var token = new StringBuilder();

            while(InputReader.Peek() >= 0) {
                char nextCharacter = (char)InputReader.Peek();

                if(!IsWhiteSpace(nextCharacter)) {
                    break;
                }

                token.Append(nextCharacter);

                InputReader.Read();   // Discard.
            }

            return new Token(TokenType.WhiteSpace, token.ToString());
        }
    }
}
