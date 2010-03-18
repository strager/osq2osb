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

        public string Encode(LocatedTextReaderWrapper source) {
            return Encode(source, new ExecutionContext());
        }

        public string Encode(LocatedTextReaderWrapper source, ExecutionContext context) {
            scriptNodes = new List<ConvertedNode>();

            var output = new StringBuilder();

            using(var bufferingReader = new BufferingTextReaderWrapper(source))
            using(var myReader = new LocatedTextReaderWrapper(bufferingReader, source.Location.Clone())) { // Sorry we have to do this...
                var parser = new Parser(myReader);

                NodeBase node;

                while((node = parser.ReadNode()) != null) {
                    string curOutput = node.Execute(context);

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
