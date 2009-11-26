using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser {
    static class Tokenizer {
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

            public string Value {
                get;
                set;
            }

            public Token(TokenType type, string value) {
                this.Type = type;
                this.Value = value;
            }

            public override string ToString() {
                return Value;
            }
            
            public static Token ReadSymbol(TextReader input) {
                return new Token(TokenType.Symbol, ((char)input.Read()).ToString());
            }

            public static Token ReadNumber(TextReader input) {
                var token = new StringBuilder();

                while(".0123456789".Contains((char)input.Peek())) {
                    token.Append((char)input.Read());
                }

                return new Token(TokenType.Number, token.ToString());
            }

            public static Token ReadIdentifier(TextReader input) {
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

            public static Token ReadString(TextReader input) {
                var str = new StringBuilder();

                if((char)input.Read() != '"') {
                    throw new InvalidDataException("String must begin with \"");
                }

                while(input.Peek() >= 0) {
                    char c = (char)input.Read();

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

        public static IEnumerable<Token> Tokenize(TextReader input) {
            Token curToken;

            while((curToken = ReadToken(input)) != null) {
                yield return curToken;
            }
        }

        public static IEnumerable<Token> Tokenize(string input) {
            using(var inputReader = new StringReader(input)) {
                return Tokenize(inputReader).ToList();
            }
        }

        public static Token ReadToken(TextReader input) {
            input.SkipWhitespace();

            if(input.Peek() < 0) {
                return null;
            }

            char c = (char)input.Peek();

            if(c == '"') {
                return Token.ReadString(input);
            } else if(".0123456789".Contains(c)) {
                return Token.ReadNumber(input);
            } else if("_".Contains(c) || char.IsLetter(c)) {
                return Token.ReadIdentifier(input);
            } else {
                return Token.ReadSymbol(input);
            }
        }
    }
}
