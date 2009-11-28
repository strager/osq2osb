using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class ElseIfNode : IfNode {
        public ElseIfNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override void Execute(TextWriter output) {
            throw new InvalidOperationException("Cannot execute an elseif node");
        }
    }
}
