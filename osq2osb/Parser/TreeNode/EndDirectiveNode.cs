using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class EndDirectiveNode : DirectiveNode {
        public override void Execute(Parser parser, TextWriter output) {
            throw new InvalidOperationException();
        }
    }
}
