using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using osq.TreeNode;

namespace osq {
    public class Parser {
        internal static readonly CultureInfo DefaultCulture = new CultureInfo("en-US");

        public LocatedTextReaderWrapper InputReader {
            get;
            set;
        }

        private readonly ParserOptions options = new ParserOptions();

        public ParserOptions Options {
            get {
                return options;
            }
        }

        public Parser(Parser other) {
            InputReader = other.InputReader;
        }

        public Parser(Parser other, LocatedTextReaderWrapper newReader) :
            this(other) {
            InputReader = newReader;
        }

        public Parser(LocatedTextReaderWrapper reader) {
            InputReader = reader;
        }

        public IEnumerable<NodeBase> ReadNodes() {
            while(true) {
                var node = ReadNode();

                if(node == null) {
                    break;
                }

                yield return node;
            }
        }

        public NodeBase ReadNode() {
            int c = InputReader.Peek();

            if(c < 0) {
                return null;
            }

            if(IsExpressionStart((char)c)) {
                return ReadExpressionNode();
            } else if(IsDirectiveStart((char)c, InputReader.Location)) {
                return ReadDirectiveNode();
            } else {
                return ReadTextNode();
            }
        }

        public static TokenNode ExpressionToTokenNode(LocatedTextReaderWrapper reader) {
            return ExpressionRewriter.Rewrite(Token.ReadTokens(reader));
        }

        private NodeBase ReadExpressionNode() {
            {
                char tmp = (char)InputReader.Read();
                System.Diagnostics.Debug.Assert(IsExpressionStart(tmp));
            }

            var startLocation = InputReader.Location.Clone();

            int c = InputReader.Peek();

            if(c == '{') {
                InputReader.Read(); // Discard.

                var tokens = ReadToExpressionEnd();

                return ExpressionRewriter.Rewrite(tokens);
            }

            if(options.AllowVariableShorthand) {
                if(c == '$') {
                    InputReader.Read(); // Discard.
                    return new RawTextNode("$", startLocation);
                }
                
                Token varName = Token.ReadToken(InputReader);
                return new TokenNode(varName, startLocation);
            }

            return new RawTextNode("$", startLocation);
        }

        private IEnumerable<Token> ReadToExpressionEnd() {
            Token token;

            while((token = Token.ReadToken(InputReader)) != null) {
                if(token.IsSymbol("}")) {
                    break;
                }

                yield return token;
            }
        }

        private DirectiveNode ReadDirectiveNode() {
            return DirectiveNode.Create(this);
        }

        private RawTextNode ReadTextNode() {
            var startLocation = InputReader.Location.Clone();

            StringBuilder text = new StringBuilder();

            int c = InputReader.Peek();

            while(c >= 0 && !IsDirectiveStart((char)c, InputReader.Location) && !IsExpressionStart((char)c)) {
                text.Append((char)c);

                InputReader.Read(); // Discard; already peeked.
                c = InputReader.Peek();
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