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

            int i = input.Peek();

            if(i < 0) {
                return null;
            }

            char c = (char)i;

            var startLocation = input.Location.Clone();

            Token token;

            if(IsStringStart(c)) {
                token = ReadString(input);
            } else if(IsNumberChar(c)) {
                token = ReadNumber(input);
            } else if(IsIdentifierChar(c)) {
                token = ReadIdentifier(input);
            } else if(IsSymbolChar(c)) {
                token = ReadSymbol(input);
            } else {
                throw new BadDataException("Unknown token starting with " + c, startLocation);
            }

            token.Location = startLocation;

            return token;
        }

        private static Token ReadSymbol(LocatedTextReaderWrapper input) {
            var token = new StringBuilder();

            while(true) {
                int i = input.Peek();

                if(i < 0) {
                    break;
                }

                char c = (char)i;

                if(!IsSymbolChar(c)) {
                    break;
                }

                if(!IsSymbolStart(token.ToString() + c)) {
                    break;
                }

                token.Append(c);

                input.Read();
            }

            return new Token(TokenType.Symbol, token.ToString());
        }

        private static Token ReadNumber(LocatedTextReaderWrapper input) {
            var startLocation = input.Location.Clone();

            var token = new StringBuilder();

            while(".0123456789".Contains((char)input.Peek())) {
                token.Append((char)input.Read());
            }

            double number;

            if(!double.TryParse(token.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Parser.DefaultCulture, out number)) {
                throw new BadDataException("Number", startLocation);
            }

            return new Token(TokenType.Number, number);
        }

        private static Token ReadIdentifier(LocatedTextReaderWrapper input) {
            var token = new StringBuilder();

            while(input.Peek() >= 0) {
                char c = (char)input.Peek();

                if(!("_".Contains(c) || char.IsLetterOrDigit(c))) {
                    break;
                }

                token.Append((char)input.Read());
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