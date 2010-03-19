using System;
using System.Collections.Generic;
using System.Linq;
using osq.TreeNode;

namespace osq {
    public class ExpressionRewriter {
        private static readonly string[][] UnaryOperatorTiers = {
            new string[] { },
            new string[] { },
            new string[] { },
            new string[] { "-", "!" },
        };

        private static readonly string[][] BinaryOperatorTiers = {
            new string[] { "," },
            new string[] { "==", "!=" },
            new string[] { ">", "<", ">=", "<=" },
            new string[] { },
            new string[] { "+", "-" },
            new string[] { "*", "/", "%" },
            new string[] { "^" },
            new string[] { ":" },
        };

        private static int GetOperatorTier(string op, string[][] tiers) {
            return Array.FindIndex(tiers, (operators) => operators.Contains(op));
        }

        private readonly Queue<Token> tokens;

        private ExpressionRewriter(IEnumerable<Token> tokens) {
            if(tokens == null) {
                throw new ArgumentNullException("tokens");
            }

            this.tokens = new Queue<Token>(tokens);
        }

        public static TokenNode Rewrite(IEnumerable<Token> tokens) {
            return (new ExpressionRewriter(tokens)).Rewrite();
        }

        private TokenNode Rewrite() {
            return ReadLevel(0);
        }

        private TokenNode ReadLevel(int level) {
            var tree = ReadNumber();

            if(tree == null) {
                return null;
            }

            while(this.tokens.Count != 0
                && this.tokens.Peek().TokenType == TokenType.Symbol
                    && GetOperatorTier(this.tokens.Peek().Value.ToString(), BinaryOperatorTiers) >= level) {
                tree = ReadBinaryExpression(tree);
            }

            return tree;
        }

        private TokenNode ReadBinaryExpression(TokenNode tree) {
            var opToken = this.tokens.Dequeue();
            var right = ReadLevel(GetOperatorTier(opToken.Value.ToString(), BinaryOperatorTiers) + 1);

            if(right == null) {
                throw new MissingDataException("Expected something after operator " + opToken.Value);
            }

            var opTree = new TokenNode(opToken, null);

            if((opToken.IsSymbol(",") && tree.Token.IsSymbol(","))
                || (opToken.IsSymbol(":") && tree.Token.IsSymbol(":") && tree.GetChildrenTokens().Count == 2)) {
                foreach(var newChild in tree.ChildrenNodes) {
                    opTree.ChildrenNodes.Add(newChild);
                }
            } else {
                opTree.ChildrenNodes.Add(tree);
            }

            opTree.ChildrenNodes.Add(right);
            tree = opTree;

            return tree;
        }

        private TokenNode ReadNumber() {
            if(this.tokens.Count == 0) {
                return null;
            }

            if(this.tokens.Peek().IsSymbol("(")) {
                return ReadParentheticalExpression();
            } else if(this.tokens.Peek().IsSymbol(")")) {
                return null;
            } else if(this.tokens.Peek().TokenType == TokenType.Identifier) {
                return ReadIdentifier();
            } else if(GetOperatorTier(this.tokens.Peek().Value.ToString(), UnaryOperatorTiers) >= 0) {
                return ReadUnaryOperator();
            } else {
                var token = this.tokens.Dequeue();

                return new TokenNode(token, null);
            }
        }

        private TokenNode ReadUnaryOperator() {
            var token = this.tokens.Dequeue();
            var node = new TokenNode(token, null);

            node.ChildrenNodes.Add(ReadLevel(GetOperatorTier(token.Value.ToString(), UnaryOperatorTiers)));

            return node;
        }

        private TokenNode ReadIdentifier() {
            var token = this.tokens.Dequeue();
            var node = new TokenNode(token, null);

            if(this.tokens.Count != 0 && this.tokens.Peek().IsSymbol("(")) {
                this.tokens.Dequeue();

                var args = ReadLevel(0);

                if(args != null) {
                    node.ChildrenNodes.Add(args);
                }

                // Eat the ).
                if(this.tokens.Count == 0 || !this.tokens.Peek().IsSymbol(")")) {
                    throw new MissingDataException("Closing parentheses");
                }

                this.tokens.Dequeue();
            }

            return node;
        }

        private TokenNode ReadParentheticalExpression() {
            this.tokens.Dequeue();

            var subTree = ReadLevel(1);

            if(this.tokens.Count == 0 || !this.tokens.Peek().IsSymbol(")")) {
                throw new MissingDataException("Closing parentheses");
            }

            this.tokens.Dequeue();

            return subTree;
        }
    }
}