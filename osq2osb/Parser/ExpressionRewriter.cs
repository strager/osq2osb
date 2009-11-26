using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    class ExpressionRewriter {
        public static IList<Tokenizer.Token> InfixToPostfix(IEnumerable<Tokenizer.Token> tokens) {
            List<Tokenizer.Token> output = new List<Tokenizer.Token>();
            Stack<Tokenizer.Token> operators = new Stack<Tokenizer.Token>();

            foreach(var token in tokens) {
                switch(token.Type) {
                    case Tokenizer.TokenType.Number:
                    case Tokenizer.TokenType.String:
                        output.Add(token);
                        break;

                    case Tokenizer.TokenType.Identifier:
                        operators.Push(token);
                        break;

                    case Tokenizer.TokenType.Symbol:
                        if(token.Value[0] == ',') {
                            /* Function argument separator. */
                            while(operators.Peek().Value[0] != '(') {
                                output.Add(operators.Pop());
                            }
                        } else if("+-*/^%".IndexOf(token.Value[0]) >= 0) {
                            while(true) {
                                if(operators.Count == 0) {
                                    break;
                                }

                                if(operators.Peek().Value[0] == '(') {
                                    break;
                                }

                                int tokenTier = "+- */%^".IndexOf(token.Value[0]) / 3;
                                int otherTokenTier = "+- */%^".IndexOf(operators.Peek().Value[0]);

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
                        } else if(token.Value[0] == '(') {
                            operators.Push(token);
                        } else if(token.Value[0] == ')') {
                            while(operators.Peek().Value[0] != '(') {
                                output.Add(operators.Pop());
                            }

                            operators.Pop();

                            if(operators.Count != 0 && operators.Peek().Type == Tokenizer.TokenType.Identifier) {
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
