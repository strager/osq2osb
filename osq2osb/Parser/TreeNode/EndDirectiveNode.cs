using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class EndDirectiveNode : DirectiveNode {
        public EndDirectiveNode(Parser parser) :
            base(parser) {
        }

        public override void Execute(TextWriter output) {
            throw new InvalidOperationException();
        }
    }
}
