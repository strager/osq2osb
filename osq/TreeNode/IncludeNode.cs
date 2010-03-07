using System.IO;
using System.Text;

namespace osq.TreeNode {
    internal class IncludeNode : DirectiveNode {
        public TokenNode Filename {
            get;
            private set;
        }

        public IncludeNode(DirectiveInfo info) :
            base(info) {
            Filename = Parser.ExpressionToTokenNode(info.ParametersReader);
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            string filePath = Filename.Evaluate(context) as string;

            if(filePath == null) {
                throw new DataTypeException("Need string for filename", this);
            }

            if(Location != null && Location.FileName != null) {
                filePath = Path.GetDirectoryName(Location.FileName) + Path.DirectorySeparatorChar + filePath;
            }

            // Warning	29	CA2202 : Microsoft.Usage : Object 'inputFile' can be disposed more than once in method 'IncludeNode.Execute(ExecutionContext)'. To avoid generating a System.ObjectDisposedException you should not call Dispose more than one time on an object.
            using(var inputFile = File.Open(filePath, FileMode.Open, FileAccess.Read)) {
                using(var reader = new LocatedTextReaderWrapper(inputFile, new Location(filePath), false)) {
                    foreach(var node in Parser.ReadNodes(reader)) {
                        output.Append(node.Execute(context));
                    }
                }
            }

            if(!context.Dependencies.Contains(filePath)) {
                context.Dependencies.Add(filePath);
            }

            return output.ToString();
        }
    }
}