using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb {
    class ExpressionRewriter {
        public enum TokenType {
            None,
            Symbol,
            Number,
            Identifier,
            Whitespace
        };

        public static TokenType GetTokenType(string token) {
            if(token.Length == 0) {
                return TokenType.None;
            }

            if(char.IsWhiteSpace(token[0])) {
                return TokenType.Whitespace;
            } else if(char.IsDigit(token[0]) || token[0] == '.') {
                return TokenType.Number;
            } else if(char.IsLetter(token[0]) || "_".IndexOf(token[0]) >= 0) {
                return TokenType.Identifier;
            } else {
                return TokenType.Symbol;
            }
        }

        public static IList<string> Tokenize(string input) {
            IList<string> tokens = new List<string>();

            string curToken = "";
            var curTokenType = TokenType.None;

            Action<string, TokenType> AddToken = (nextTokenPart, nextTokenPartType) => {
                if(curToken == "") {
                    return;
                }

                tokens.Add(curToken);

                if(nextTokenPartType != TokenType.Whitespace && nextTokenPartType != TokenType.None) {
                    curToken = nextTokenPart;
                    curTokenType = nextTokenPartType;
                } else {
                    curToken = "";
                    curTokenType = TokenType.None;
                }
            };

            for(int i = 0; i < input.Length; ++i) {
                string nextTokenPart = new string(input[i], 1);

                var nextTokenPartType = GetTokenType(nextTokenPart);
                
                if((nextTokenPartType != TokenType.None && nextTokenPartType != TokenType.Whitespace) && (
                   (curTokenType == TokenType.None)
                || (curTokenType == nextTokenPartType && curTokenType != TokenType.Symbol && nextTokenPartType != TokenType.Symbol)
                || (curTokenType == TokenType.Identifier && nextTokenPartType == TokenType.Number))) {
                    curToken += nextTokenPart;

                    if(curTokenType == TokenType.None) {
                        curTokenType = nextTokenPartType;
                    }
                } else {
                    AddToken(nextTokenPart, nextTokenPartType);
                }
            }

            AddToken("", TokenType.None);

            return tokens;
        }

        public static IList<string> InfixToPostfix(IEnumerable<string> tokens) {
            List<string> output = new List<string>();
            Stack<string> operators = new Stack<string>();

            foreach(var token in tokens) {
                var tokenType = GetTokenType(token);
                
                switch(tokenType) {
                    case TokenType.Number:
                        output.Add(token);
                        break;

                    case TokenType.Identifier:
                        operators.Push(token);
                        break;

                    case TokenType.Symbol:
                        if(token[0] == ',') {
                            /* Function argument separator. */
                            while(operators.Peek()[0] != '(') {
                                output.Add(operators.Pop());
                            }
                        } else if("+-*/^%".IndexOf(token[0]) >= 0) {
                            while(true) {
                                if(operators.Count == 0) {
                                    break;
                                }

                                if(operators.Peek()[0] == '(') {
                                    break;
                                }

                                int tokenTier = "+- */%^".IndexOf(token[0]) / 3;
                                int otherTokenTier = "+- */%^".IndexOf(operators.Peek()[0]);

                                if(otherTokenTier >= 0) {
                                    otherTokenTier /= 3;
                                } else {
                                    otherTokenTier = int.MaxValue;
                                }
                                
                                if(!(tokenTier <= otherTokenTier)) {
                                    break;
                                }

                                output.Add(operators.Pop());
                            }

                            operators.Push(token);
                        } else if(token[0] == '(') {
                            operators.Push(token);
                        } else if(token[0] == ')') {
                            while(operators.Peek()[0] != '(') {
                                output.Add(operators.Pop());
                            }

                            operators.Pop();

                            if(operators.Count != 0 && GetTokenType(operators.Peek()) == TokenType.Identifier) {
                                output.Add(operators.Pop());
                            }
                        }

                        break;
                }
            }

            output.AddRange(operators);

            return output;
        }
    }
}
