using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using osq.TreeNode;

namespace osq.Parser {
    /// <summary>
    /// Parses an osq script into <see cref="NodeBase"/> instances.
    /// </summary>
    public class Parser : INodeReader {/// <summary>
        /// Gets or sets the input reader from which the <see cref="Parser"/> reads nodes.
        /// </summary>
        /// <value>The input reader.</value>
        public LocatedTextReaderWrapper InputReader {
            get;
            set;
        }

        public Location Location {
            get {
                return InputReader == null ? null : InputReader.Location;
            }
        }

        /// <summary>
        /// Parser options.
        /// </summary>
        private ParserOptions options = new ParserOptions();

        /// <summary>
        /// Gets or sets the parser options.
        /// </summary>
        /// <value>The parser options.</value>
        public ParserOptions Options {
            get {
                return this.options;
            }

            private set {
                this.options = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class given a <see cref="Parser"/> to model after.
        /// </summary>
        /// <param name="other">Parser to copy the options and reader from.</param>
        public Parser(Parser other) {
            if(other == null) {
                throw new ArgumentNullException("other");
            }

            InputReader = other.InputReader;
            Options = other.Options.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class given a <see cref="Parser"/> to model after and a new reader.
        /// </summary>
        /// <param name="other">Parser to copy the options from.</param>
        /// <param name="newReader">The new reader.</param>
        public Parser(Parser other, LocatedTextReaderWrapper newReader) :
            this(other) {
            InputReader = newReader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="reader">The reader with which to parse.</param>
        public Parser(LocatedTextReaderWrapper reader) {
            InputReader = reader;
        }

        /// <summary>
        /// Reads parser nodes from <see cref="InputReader"/>.
        /// </summary>
        /// <returns>Collection of nodes in <see cref="InputReader"/>'s contents.</returns>
        public IEnumerable<NodeBase> ReadNodes() {
            while(true) {
                var node = ReadNode();

                if(node == null) {
                    break;
                }

                yield return node;
            }
        }

        /// <summary>
        /// Reads a parser node from <see cref="InputReader"/>.
        /// </summary>
        /// <returns>Node reperesenting the read text.</returns>
        /// <exception cref="ParserException">Failed to parse a node.</exception>
        /// <exception cref="InvalidOperationException"><see cref="InputReader"/> is <c>null</c>.</exception>
        public NodeBase ReadNode() {
            if(InputReader == null) {
                throw new InvalidOperationException("Must have an InputReader to parse");
            }

            int c = InputReader.Peek();

            if(c < 0) {
                return null;
            }

            if(IsExpressionStart((char)c)) {
                return ReadExpressionNode();
            }

            if(IsDirectiveStart((char)c, this.InputReader.Location)) {
                return ReadDirectiveNode();
            }

            return ReadTextNode();
        }

        /// <summary>
        /// Reads an expression node.
        /// </summary>
        /// <returns>Node representing the read expression.</returns>
        private NodeBase ReadExpressionNode() {
            {
                char tmp = (char)InputReader.Read();
                System.Diagnostics.Debug.Assert(IsExpressionStart(tmp));
            }

            var startLocation = InputReader.Location.Clone();

            int nextCharacter = InputReader.Peek();

            if(nextCharacter == '{') {
                InputReader.Read(); // Discard.

                var tokens = ReadToExpressionEnd();

                return ExpressionRewriter.Rewrite(tokens);
            }

            if(options.AllowVariableShorthand) {
                if(nextCharacter == '$') {
                    InputReader.Read(); // Discard.
                    return new RawTextNode("$", startLocation);
                }
                
                Token varName = (new TokenReader(InputReader)).ReadToken();
                return new TokenNode(varName);
            }

            return new RawTextNode("$", startLocation);
        }

        /// <summary>
        /// Reads tokens until the end of an expression.  Discards the end of the expression.
        /// </summary>
        /// <returns>Tokens representing the expression.</returns>
        private IEnumerable<Token> ReadToExpressionEnd() {
            var tokenReader = new TokenReader(InputReader);
            Token token;

            while((token = tokenReader.ReadToken()) != null) {
                if(token.IsSymbol("}")) {
                    break;
                }

                yield return token;
            }
        }

        /// <summary>
        /// Reads a directive node, including any children nodes.
        /// </summary>
        /// <returns>Directive node.</returns>
        private DirectiveNode ReadDirectiveNode() {
            return DirectiveNode.Create(this);
        }

        /// <summary>
        /// Reads a plain text node.
        /// </summary>
        /// <returns>Raw text node.</returns>
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

        /// <summary>
        /// Determines whether the specified character is the start of an expression.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <returns>
        /// 	<c>true</c> if the specified character is the start of an expression; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsExpressionStart(char c) {
            return c == '$';
        }

        /// <summary>
        /// Determines whether the specified character at the given location is the start of an directive.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <param name="loc">The location of <paramref name="c"/>.</param>
        /// <returns>
        /// 	<c>true</c> if the specified character at the given location is the start of an directive; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDirectiveStart(char c, Location loc) {
            return c == '#' && loc.Column == 1;
        }
        
        IEnumerator<NodeBase> IEnumerable<NodeBase>.GetEnumerator() {
            return ReadNodes().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ReadNodes().GetEnumerator();
        }
    }
}