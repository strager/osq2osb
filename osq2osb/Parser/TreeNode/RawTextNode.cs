using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RawTextNode : NodeBase {
        public override void Execute(Parser parser, TextWriter output) {
            output.Write(parser.ReplaceExpressions(this.Content));
        }

        public RawTextNode(string content) :
            base(content) {
        }
    }
}
