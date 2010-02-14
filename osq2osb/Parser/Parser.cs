using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser {
    using TreeNode;

    public static class Parser {
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
            } catch(ParserException e) {
                throw e;
            } catch(Exception e) {
                throw new ParserException("Problem parsing node", loc, e);
            }
        }

        public static TokenNode ExpressionToTokenNode(string expression) {
            return ExpressionToTokenNode(expression, new Location());
        }

        public static TokenNode ExpressionToTokenNode(string expression, Location location) {
            using(var reader = new LocatedTextReaderWrapper(expression, location)) {
                return ExpressionToTokenNode(reader);
            }
        }

        public static TokenNode ExpressionToTokenNode(LocatedTextReaderWrapper input) {
            return ExpressionRewriter.Rewrite(Token.ReadTokens(input));
        }

        private static TokenNode ReadExpressionNode(LocatedTextReaderWrapper input) {
            var startLocation = input.Location.Clone();

            return ExpressionToTokenNode(ReadExpressionString(input), startLocation);
        }

        private static string ReadExpressionString(LocatedTextReaderWrapper input) {
            if(!IsExpressionStart((char)input.Read())) {
                throw new InvalidDataException("Expressions must begin with $");
            }

            int c = input.Peek();

            switch(c) {
                case '{':
                    input.Read();   // Discard.
                    return ReadToExpressionEnd(input);

                case '$':
                    input.Read();   // Discard.
                    return "$";

                default:
                    Token varName = Token.ReadToken(input);
                    return varName.Value.ToString();
            }
        }

        private static string ReadToExpressionEnd(LocatedTextReaderWrapper input) {
            StringBuilder expression = new StringBuilder();

            int c = input.Read();

            while(c >= 0 && c != '}') {
                expression.Append((char)c);

                c = input.Read();
            }

            // Ending } already read.

            return expression.ToString();
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

                input.Read();   // Discard; already peeked.
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
