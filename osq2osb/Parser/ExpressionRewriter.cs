using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq2osb.Parser.TreeNode;

namespace osq2osb.Parser {
    public class ExpressionRewriter {
        private static string[] unaryOperatorTiers = new string[] { "", "-" };
        private static string[] binaryOperatorTiers = new string[] { ",", "+-", "*/%", "^", ":" };

        private static int GetOperatorTier(char op, string[] tiers) {
            return Array.FindIndex(tiers, (operators) => operators.IndexOf(op) >= 0);
        }

        Queue<Tokenizer.Token> tokens;
        Parser parser;

        private ExpressionRewriter(IEnumerable<Tokenizer.Token> tokens, Parser parser) {
            if(tokens == null) {
                throw new ArgumentNullException("tokens");
            }

            this.tokens = new Queue<Tokenizer.Token>(tokens);

            this.parser = parser;
        }

        public static TokenNode Rewrite(IEnumerable<Tokenizer.Token> tokens, Parser parser) {
            return (new ExpressionRewriter(tokens, parser)).Rewrite();
        }

        private TokenNode Rewrite() {
            return ReadLevel(1);
        }

        private TokenNode ReadLevel(int level) {
            var tree = ReadNumber();

            if(tree == null) {
                return null;
            }

            while(tokens.Count != 0 && tokens.Peek().Type == Tokenizer.TokenType.Symbol && GetOperatorTier(tokens.Peek().Value.ToString()[0], binaryOperatorTiers) >= level) {
                var opToken = tokens.Dequeue();
                var right = ReadLevel(level + 1);

                var opTree = new TokenNode(opToken, parser, null);

                opTree.ChildrenNodes.Add(tree);
                opTree.ChildrenNodes.Add(right);

                tree = opTree;
            }

            return tree;
        }

        private TokenNode ReadNumber() {
            if(tokens.Count == 0) {
                return null;
            }

            if(tokens.Peek().Value.ToString()[0] == '(') {
                tokens.Dequeue();

                var subTree = ReadLevel(1);

                if(tokens.Peek().Value.ToString()[0] != ')') {
                    throw new Exception("Unmatched parens");    // FIXME Better exception class.
                }

                tokens.Dequeue();

                return subTree;
            } else if(tokens.Peek().Value.ToString()[0] == ')') {
                return null;
            } else if(tokens.Peek().Type == Tokenizer.TokenType.Identifier) {
                var token = tokens.Dequeue();

                var node = new TokenNode(token, parser, null);

                if(tokens.Count != 0 && tokens.Peek().Value.ToString()[0] == '(') {
                    tokens.Dequeue();

                    var sub = ReadLevel(0);

                    // Lay tree into list.
                    while(sub != null) {
                        node.ChildrenNodes.Add(new TokenNode(sub.Token, parser, sub.Location));

                        if(sub.Token.Type != Tokenizer.TokenType.Symbol || (string)sub.Token.Value != ",") {
                            break;
                        }

                        sub = sub.TokenChildren[1];
                    }

                    // Eat the ).
                    if(tokens.Count == 0 || tokens.Peek().Value.ToString()[0] != ')') {
                        throw new Exception("Unmatched parens");    // FIXME Better exception class.
                    }

                    tokens.Dequeue();
                }

                return node;
            } else if(GetOperatorTier(tokens.Peek().Value.ToString()[0], unaryOperatorTiers) >= 0) {
                var token = tokens.Dequeue();
                var node = new TokenNode(token, parser, null);

                node.ChildrenNodes.Add(ReadLevel(GetOperatorTier(token.Value.ToString()[0], unaryOperatorTiers)));

                return node;
            } else {
                var token = tokens.Dequeue();

                return new TokenNode(token, parser, null);
            }
        }
    }
}
