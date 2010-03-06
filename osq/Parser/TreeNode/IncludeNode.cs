using System.Text;
using System.IO;

namespace osq.Parser.TreeNode {
    class IncludeNode : DirectiveNode {
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
                throw new InvalidDataException("Need string for filename").AtLocation(this.Location);
            }

            if(this.Location != null && this.Location.FileName != null) {
                filePath = Path.GetDirectoryName(this.Location.FileName) + Path.DirectorySeparatorChar + filePath;
            }

            using(var inputFile = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using(var reader = new LocatedTextReaderWrapper(inputFile, new Location(filePath))) {
                foreach(var node in Parser.ReadNodes(reader)) {
                    output.Append(node.Execute(context));
                }
            }

            if(!context.Dependencies.Contains(filePath)) {
                context.Dependencies.Add(filePath);
            }

            return output.ToString();
        }
    }
}
