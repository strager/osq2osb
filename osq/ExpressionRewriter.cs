﻿using System;
using System.Collections.Generic;
using System.Linq;
using osq.Parser;
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

        [Obsolete]
        private ExpressionRewriter(IEnumerable<Token> tokens) {
            if(tokens == null) {
                throw new ArgumentNullException("tokens");
            }

            this.tokens = new Queue<Token>(tokens.Where((token) => token.TokenType != TokenType.WhiteSpace));
        }

        private ExpressionRewriter(ITokenReader tokenReader) {
            if(tokenReader == null) {
                throw new ArgumentNullException("tokenReader");
            }

            this.tokens = new Queue<Token>();

            Token token;
            
            while((token = tokenReader.ReadToken()) != null) {
                if(token.TokenType != TokenType.WhiteSpace) {
                    this.tokens.Enqueue(token);
                }
            }
        }

        public static TokenNode Rewrite(ITokenReader tokenReader) {
            return (new ExpressionRewriter(tokenReader)).Rewrite();
        }

        [Obsolete]
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

            while(this.tokens.Count != 0) {
                if(this.tokens.Peek().TokenType != TokenType.Symbol) {
                    break;
                }

                if(GetOperatorTier(this.tokens.Peek().Value.ToString(), BinaryOperatorTiers) < level) {
                    break;
                }

                tree = ReadBinaryExpression(tree);
            }

            return tree;
        }

        private TokenNode ReadBinaryExpression(TokenNode tree) {
            var opcodeToken = this.tokens.Dequeue();
            var right = ReadLevel(GetOperatorTier(opcodeToken.Value.ToString(), BinaryOperatorTiers) + 1);

            if(right == null) {
                throw new MissingDataException("Expected something after operator " + opcodeToken.Value);
            }

            var opcodeNode = new TokenNode(opcodeToken);

            AddChildToOperator(tree, opcodeNode);

            opcodeNode.ChildrenTokenNodes.Add(right);

            return opcodeNode;
        }

        private void AddChildToOperator(TokenNode child, TokenNode parent) {
            bool mergeCommas = (parent.Token.IsSymbol(",") && child.Token.IsSymbol(","));
            bool mergeColons = (parent.Token.IsSymbol(":") && child.Token.IsSymbol(":") && child.ChildrenTokenNodes.Count == 2);

            if(mergeCommas || mergeColons) {
                foreach(var newChild in child.ChildrenTokenNodes) {
                    parent.ChildrenTokenNodes.Add(newChild);
                }
            } else {
                parent.ChildrenTokenNodes.Add(child);
            }
        }

        private TokenNode ReadNumber() {
            if(this.tokens.Count == 0) {
                return null;
            }

            if(this.tokens.Peek().IsSymbol("(")) {
                return ReadParentheticalExpression();
            }

            if(this.tokens.Peek().IsSymbol(")")) {
                return null;
            }

            if(this.tokens.Peek().TokenType == TokenType.Identifier) {
                return ReadIdentifier();
            }

            if(GetOperatorTier(this.tokens.Peek().Value.ToString(), UnaryOperatorTiers) >= 0) {
                return ReadUnaryOperator();
            }

            return new TokenNode(this.tokens.Dequeue());
        }

        private TokenNode ReadUnaryOperator() {
            var token = this.tokens.Dequeue();
            var node = new TokenNode(token);

            node.ChildrenTokenNodes.Add(ReadLevel(GetOperatorTier(token.Value.ToString(), UnaryOperatorTiers)));

            return node;
        }

        private TokenNode ReadIdentifier() {
            var identifier = this.tokens.Dequeue();
            var identifierNode = new TokenNode(identifier);
            
            if(this.tokens.Count != 0 && this.tokens.Peek().IsSymbol("(")) {
                this.tokens.Dequeue();

                var arguments = ReadLevel(0);

                if(arguments != null) {
                    identifierNode.ChildrenTokenNodes.Add(arguments);
                }

                ReadClosingParentheses();
            }

            return identifierNode;
        }

        private TokenNode ReadParentheticalExpression() {
            var leftParentheses = this.tokens.Dequeue();

            System.Diagnostics.Debug.Assert(leftParentheses.IsSymbol("("));

            var subTree = ReadLevel(0);

            ReadClosingParentheses();

            return subTree;
        }

        private void ReadClosingParentheses() {
            if(this.tokens.Count == 0 || !this.tokens.Peek().IsSymbol(")")) {
                throw new MissingDataException("Closing parentheses");
            }

            this.tokens.Dequeue();
        }
    }
}