using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RawTextNode : NodeBase {
        public RawTextNode(string content, Parser parser, Location location) :
            base(content, parser, location) {
        }

        public override void Execute(TextWriter output) {
            output.Write(this.Content);
        }
    }
}
