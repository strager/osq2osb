using System;
using System.Collections.Generic;
using System.Text;
using osq.TreeNode;

namespace osq {
    internal class ConvertedNode {
        public string OriginalScript {
            get;
            set;
        }

        public NodeBase Node {
            get;
            set;
        }

        public string NodeOutput {
            get;
            set;
        }
    }

    /// <summary>
    /// Transforms an osq script into an osb script, and then back to an osq script after modifications.
    /// </summary>
    public class Encoder {
        /// <summary>
        /// List of nodes in the original osq script.
        /// </summary>
        private List<ConvertedNode> scriptNodes = null;

        /// <summary>
        /// Gets or sets the parser which transforms the osq script.
        /// </summary>
        /// <value>The parser which transforms the osq script.</value>
        public Parser.Parser Parser {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the execution context of the conversion from osq to osb.
        /// </summary>
        /// <value>The execution context of the conversion from osq to osb.</value>
        public ExecutionContext ExecutionContext {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Encoder"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="context">The execution context.</param>
        public Encoder(LocatedTextReaderWrapper reader, ExecutionContext context = null) :
            this(new Parser.Parser(reader), context) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Encoder"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="context">The execution context.</param>
        public Encoder(Parser.Parser parser, ExecutionContext context = null) {
            Parser = parser;
            ExecutionContext = context ?? new ExecutionContext();
        }

        /// <summary>
        /// Transforms the osq script into an osb script.
        /// </summary>
        /// <returns>The osb script.</returns>
        public string Encode() {
            if(ExecutionContext == null) {
                throw new InvalidOperationException("ExecutionContext must not be null");
            }

            if(Parser == null) {
                throw new InvalidOperationException("Parser must not be null");
            }

            scriptNodes = new List<ConvertedNode>();

            var output = new StringBuilder();

            using(var bufferingReader = new BufferingTextReaderWrapper(Parser.InputReader))
            using(var myReader = new LocatedTextReaderWrapper(bufferingReader, Parser.InputReader.Location.Clone())) { // Sorry we have to do this...
                var parser = new Parser.Parser(Parser, myReader);

                NodeBase node;

                while((node = parser.ReadNode()) != null) {
                    string curOutput = node.Execute(ExecutionContext);

                    output.Append(curOutput);

                    var converted = new ConvertedNode {
                        Node = node,
                        NodeOutput = curOutput,
                        OriginalScript = bufferingReader.BufferedText
                    };

                    scriptNodes.Add(converted);

                    bufferingReader.ClearBuffer();
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Transforms a modified osb script into an osq script.
        /// </summary>
        /// <param name="modifiedSource">The modified osq script.</param>
        /// <returns>The osq script.</returns>
        public string Decode(string modifiedSource) {
            if(scriptNodes == null) {
                return modifiedSource;
            }

            var output = new StringBuilder();

            foreach(var convertedNode in scriptNodes) {
                int index = modifiedSource.IndexOf(convertedNode.NodeOutput);

                if(index < 0) {
                    continue;
                }

                output.Append(modifiedSource.Substring(0, index));
                output.Append(convertedNode.OriginalScript);

                modifiedSource = modifiedSource.Substring(index + convertedNode.NodeOutput.Length);
            }

            output.Append(modifiedSource);

            return output.ToString();
        }
    }
}
