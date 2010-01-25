using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser {
    using TreeNode;

    public static class Parser {
        public static IEnumerable<NodeBase> Parse(TextReader input) {
            return Parse(input, new Location());
        }

        public static IEnumerable<NodeBase> Parse(TextReader input, Location location) {
            while(true) {
                var node = ParseNode(input, location);

                if(node == null) {
                    break;
                }

                yield return node;
            }
        }

        public static NodeBase ParseNode(TextReader input) {
            return ParseNode(input, new Location());
        }

        public static NodeBase ParseNode(TextReader input, Location location) {
            if(input == null) {
                throw new ArgumentNullException("input");
            }

            int c = input.Peek();

            if(c < 0) {
                return null;
            }

            var loc = location.Clone();

            try {
                if(c == '$') {
                    return ReadExpressionNode(input, location);
                } else if(c == '#' && location.Column == 1) {
                    return ReadDirectiveNode(input, location);
                } else {
                    return ReadTextNode(input, location);
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
            return ExpressionRewriter.Rewrite(Tokenizer.Tokenize(expression, location));
        }

        private static TokenNode ReadExpressionNode(TextReader input, Location location) {
            var startLocation = location.Clone();

            return ExpressionToTokenNode(ReadExpressionString(input, location), startLocation);
        }

        private static string ReadExpressionString(TextReader input, Location location) {
            if(input.Read() != '$') {
                throw new InvalidDataException("Expressions must begin with $");
            }

            if(input.Read() != '{') {
                throw new InvalidDataException("Expressions must begin with ${");
            }

            StringBuilder expression = new StringBuilder();

            int c = input.Read();

            while(c >= 0 && c != '}') {
                expression.Append((char)c);

                location.AdvanceCharacter((char)c);

                c = input.Read();
            }

            // Ending } already read.

            return expression.ToString();
        }

        private static DirectiveNode ReadDirectiveNode(TextReader input, Location location) {
            var startLocation = location.Clone();

            string line = input.ReadLine();
            location.AdvanceLine();

            return DirectiveNode.Create(line, input, startLocation);
        }

        private static RawTextNode ReadTextNode(TextReader input, Location location) {
            var startLocation = location.Clone();

            StringBuilder text = new StringBuilder();

            int c = input.Peek();

            while(c >= 0 && !(c == '#' && location.Column == 1) && c != '$') {
                text.Append((char)c);

                location.AdvanceCharacter((char)c);

                input.Read();   // Discard; already peeked.
                c = input.Peek();
            }

            return new RawTextNode(text.ToString(), startLocation);
        }
   }
}
