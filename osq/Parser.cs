using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using osq.TreeNode;

namespace osq {
    public static class Parser {
        internal static readonly CultureInfo DefaultCulture = new CultureInfo("en-US");

        public static IEnumerable<NodeBase> ReadNodes(LocatedTextReaderWrapper input) {
            while(true) {
                var node = ReadNode(input);

                if(node == null) {
                    break;
                }

                yield return node;
            }
        }

        public static NodeBase ReadNode(LocatedTextReaderWrapper input) {
            if(input == null) {
                throw new ArgumentNullException("input");
            }

            int c = input.Peek();

            if(c < 0) {
                return null;
            }

            var loc = input.Location.Clone();

            try {
                if(IsExpressionStart((char)c)) {
                    return ReadExpressionNode(input);
                } else if(IsDirectiveStart((char)c, loc)) {
                    return ReadDirectiveNode(input);
                } else {
                    return ReadTextNode(input);
                }
            } catch(Exception e) {
                // FIXME badly.
                throw e.AtLocation(loc);
            }
        }

        public static TokenNode ExpressionToTokenNode(LocatedTextReaderWrapper reader) {
            return ExpressionRewriter.Rewrite(Token.ReadTokens(reader));
        }

        private static NodeBase ReadExpressionNode(LocatedTextReaderWrapper input) {
            {
                char tmp = (char)input.Read();
                System.Diagnostics.Debug.Assert(IsExpressionStart(tmp));
            }

            var startLocation = input.Location.Clone();

            int c = input.Peek();

            switch(c) {
                case (int)'{':
                    input.Read(); // Discard.

                    var tokens = ReadToExpressionEnd(input);

                    return ExpressionRewriter.Rewrite(tokens);

                case (int)'$':
                    input.Read(); // Discard.
                    return new RawTextNode("$", startLocation);

                default:
                    Token varName = Token.ReadToken(input);
                    return new TokenNode(varName, startLocation);
            }
        }

        private static IEnumerable<Token> ReadToExpressionEnd(LocatedTextReaderWrapper input) {
            Token token;

            while((token = Token.ReadToken(input)) != null) {
                if(token.IsSymbol("}")) {
                    break;
                }

                yield return token;
            }
        }

        private static DirectiveNode ReadDirectiveNode(LocatedTextReaderWrapper input) {
            return DirectiveNode.Create(input);
        }

        private static RawTextNode ReadTextNode(LocatedTextReaderWrapper input) {
            var startLocation = input.Location.Clone();

            StringBuilder text = new StringBuilder();

            int c = input.Peek();

            while(c >= 0 && !IsDirectiveStart((char)c, input.Location) && !IsExpressionStart((char)c)) {
                text.Append((char)c);

                input.Read(); // Discard; already peeked.
                c = input.Peek();
            }

            return new RawTextNode(text.ToString(), startLocation);
        }

        private static bool IsExpressionStart(char c) {
            return c == '$';
        }

        private static bool IsDirectiveStart(char c, Location loc) {
            return c == '#' && loc.Column == 1;
        }
    }
}