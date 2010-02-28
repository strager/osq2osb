using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser {
    public enum TokenType {
        Symbol,
        Number,
        Identifier,
        String
    }

    public class Token {
        public TokenType Type {
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
            this.Type = type;
            this.Value = value;
            this.Location = location;
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
            return this.Type == TokenType.Symbol && (string)this.Value == expected;
        }

        public static IEnumerable<Token> ReadTokens(LocatedTextReaderWrapper input) {
            Token curToken;

            while((curToken = ReadToken(input)) != null) {
                yield return curToken;
            }
        }

        public static Token ReadToken(LocatedTextReaderWrapper input) {
            Location loc = null;

            try {
                input.SkipWhitespace();

                int i = input.Peek();

                if(i < 0) {
                    return null;
                }

                char c = (char)i;

                loc = input.Location.Clone();

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
                    throw new InvalidDataException("Unknown token starting with " + c).AtLocation(loc);
                }

                token.Location = loc;

                return token;
            } catch(Exception e) {
                throw e.AtLocation(loc);
            }
        }

        private static Token ReadSymbol(TextReader input) {
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

        private static Token ReadNumber(TextReader input) {
            var token = new StringBuilder();

            while(".0123456789".Contains((char)input.Peek())) {
                token.Append((char)input.Read());
            }

            return new Token(TokenType.Number, Convert.ToDouble(token.ToString()));
        }

        private static Token ReadIdentifier(TextReader input) {
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

        private static Token ReadString(TextReader input) {
            var str = new StringBuilder();

            input.Read();  // Consume ".

            while(true) {
                int rawChar = input.Read();

                if(rawChar < 0) {
                    throw new InvalidDataException("Unexpected end-of-stream");
                }

                char c = (char)rawChar;

                if(c == '"') {
                    break;
                }

                if(c == '\\') {
                    int next = input.Read();

                    if(next < 0) {
                        throw new InvalidDataException("Unexpected end-of-stream following \\");
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
                            throw new InvalidDataException("Unknown escape character: \\" + (char)next);
                    }
                }

                str.Append(c);
            }

            return new Token(TokenType.String, str.ToString());
        }
    }
}
