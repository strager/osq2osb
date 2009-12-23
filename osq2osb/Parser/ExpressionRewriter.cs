using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq2osb.Parser.TreeNode;

namespace osq2osb.Parser {
    class ExpressionRewriter {
        private static string[] binaryOperatorTiers = new string[] { "+-", "*/%", "^", ":" };

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
            return ReadLevel(0);
        }

        private TokenNode ReadLevel(int level) {
            var tree = ReadNumber();

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
            if(tokens.Peek().Value.ToString()[0] == '(') {
                tokens.Dequeue();

                var subTree = ReadLevel(0);

                if(tokens.Peek().Value.ToString()[0] != ')') {
                    throw new Exception("Unmatched parens");    // FIXME Better exception class.
                }

                tokens.Dequeue();

                return subTree;
            } else {
                return new TokenNode(tokens.Dequeue(), parser, null);
            }
        }
    }
}
