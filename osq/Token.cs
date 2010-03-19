using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace osq {
    public enum TokenType {
        Symbol,
        Number,
        Identifier,
        String
    }

    public class Token {
        public TokenType TokenType {
            get;
            set;
        }

        public object Value {
            get;
            set;
        }

        public Location Location {
            get;
            set;
        }

        public Token(TokenType type, object value) :
            this(type, value, null) {
        }

        public Token(TokenType type, object value, Location location) {
            TokenType = type;
            Value = value;
            Location = location;
        }

        public override string ToString() {
            return Value.ToString();
        }

        private static bool IsStringStart(char c) {
            return c == '"';
        }

        private static bool IsNumberChar(char c) {
            return ".0123456789".Contains(c);
        }

        private static bool IsIdentifierChar(char c) {
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

        public bool IsSymbol(string expected) {
            return TokenType == TokenType.Symbol && (string)Value == expected;
        }

        public static IEnumerable<Token> ReadTokens(LocatedTextReaderWrapper input) {
            Token curToken;

            while((curToken = ReadToken(input)) != null) {
                yield return curToken;
            }
        }

        public static Token ReadToken(LocatedTextReaderWrapper input) {
            input.SkipWhiteSpace();

            int nextCharacterCode = input.Peek();

            if(nextCharacterCode < 0) {
                return null;
            }

            char nextCharacter = (char)nextCharacterCode;

            var startLocation = input.Location.Clone();

            Token token;

            if(IsStringStart(nextCharacter)) {
                token = ReadString(input);
            } else if(IsNumberChar(nextCharacter)) {
                token = ReadNumber(input);
            } else if(IsIdentifierChar(nextCharacter)) {
                token = ReadIdentifier(input);
            } else if(IsSymbolChar(nextCharacter)) {
                token = ReadSymbol(input);
            } else {
                throw new BadDataException("Unknown token starting with " + nextCharacter, startLocation);
            }

            token.Location = startLocation;

            return token;
        }

        private static Token ReadSymbol(LocatedTextReaderWrapper input) {
            var curToken = "";

            while(input.Peek() >= 0) {
                char nextCharacter = (char)input.Peek();

                if(!IsSymbolChar(nextCharacter)) {
                    break;
                }

                string newToken = curToken + nextCharacter;

                if(!IsSymbolStart(newToken)) {
                    break;
                }

                curToken = newToken;

                input.Read();   // Discard.
            }

            return new Token(TokenType.Symbol, curToken);
        }

        private static Token ReadNumber(LocatedTextReaderWrapper input) {
            var startLocation = input.Location.Clone();

            var numberString = new StringBuilder();

            while(".0123456789".Contains((char)input.Peek())) {
                numberString.Append((char)input.Read());
            }

            double number;

            if(!double.TryParse(numberString.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Parser.DefaultCulture, out number)) {
                throw new BadDataException("Number", startLocation);
            }

            return new Token(TokenType.Number, number);
        }

        private static Token ReadIdentifier(LocatedTextReaderWrapper input) {
            var token = new StringBuilder();

            while(input.Peek() >= 0) {
                char nextCharacter = (char)input.Peek();

                if(!("_".Contains(nextCharacter) || char.IsLetterOrDigit(nextCharacter))) {
                    break;
                }

                token.Append(nextCharacter);

                input.Read();   // Discard.
            }

            return new Token(TokenType.Identifier, token.ToString());
        }

        private static Token ReadString(LocatedTextReaderWrapper input) {
            var str = new StringBuilder();

            input.Read(); // Consume ".

            while(true) {
                int rawChar = input.Read();

                if(rawChar < 0) {
                    throw new MissingDataException("End-of-string terminator", input.Location);
                }

                char c = (char)rawChar;

                if(c == '"') {
                    break;
                }

                if(c == '\\') {
                    int next = input.Read();

                    if(next < 0) {
                        throw new MissingDataException("Code following \\", input.Location);
                    }

                    switch((char)next) {
                        case 'n':
                            c = '\n';
                            break;

                        case 't':
                            c = '\t';
                            break;

                        case 'r':
                            c = '\r';
                            break;

                        case '\\':
                            c = '\\';
                            break;

                        case '"':
                            c = '"';
                            break;

                        default:
                            throw new BadDataException("Unknown escape character \\" + (char)next, input.Location);
                    }
                }

                str.Append(c);
            }

            return new Token(TokenType.String, str.ToString());
        }
    }
}