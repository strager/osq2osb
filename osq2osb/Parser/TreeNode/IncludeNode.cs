using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
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
                throw new ExecutionException("Need string for filename", this.Location);
            }

            if(this.Location != null && this.Location.Filename != null) {
                filePath = Path.GetDirectoryName(this.Location.Filename) + Path.DirectorySeparatorChar + filePath;
            }

            using(var inputFile = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using(var reader = new LocatedTextReaderWrapper(inputFile, new Location(filePath))) {
                foreach(var node in Parser.ReadNodes(reader)) {
                    output.Append(node.Execute(context));
                }
            }

            if(!context.Dependancies.Contains(filePath)) {
                context.Dependancies.Add(filePath);
            }

            return output.ToString();
        }
    }
}
