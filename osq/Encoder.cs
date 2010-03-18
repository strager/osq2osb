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

    public class Encoder {
        private List<ConvertedNode> scriptNodes = null;

        private Parser parser;

        public Parser Parser {
            get {
                return this.parser;
            }

            set {
                if(value == null) {
                    throw new ArgumentNullException("value");
                }

                this.parser = value;
            }
        }

        private ExecutionContext executionContext;

        public ExecutionContext ExecutionContext {
            get {
                return this.executionContext;
            }

            set {
                if(value == null) {
                    throw new ArgumentNullException("value");
                }

                this.executionContext = value;
            }
        }

        public Encoder() :
            this(new Parser()) {
        }

        public Encoder(LocatedTextReaderWrapper reader) :
            this(new Parser(reader)) {
        }

        public Encoder(LocatedTextReaderWrapper reader, ExecutionContext context) :
            this(new Parser(reader), context) {
        }

        public Encoder(Parser parser) :
            this(parser, new ExecutionContext()) {
        }

        public Encoder(Parser parser, ExecutionContext context) {
            Parser = parser;
            ExecutionContext = context;
        }

        public string Encode() {
            scriptNodes = new List<ConvertedNode>();

            var output = new StringBuilder();

            using(var bufferingReader = new BufferingTextReaderWrapper(Parser.InputReader))
            using(var myReader = new LocatedTextReaderWrapper(bufferingReader, Parser.InputReader.Location.Clone())) { // Sorry we have to do this...
                var parser = new Parser(Parser, myReader);

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
