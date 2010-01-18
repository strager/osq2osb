using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RawTextNode : NodeBase {
        public RawTextNode(string content, Location location) :
            base(content, location) {
        }

        public override void Execute(TextWriter output, ExecutionContext context) {
            output.Write(this.Content);
        }
    }
}
