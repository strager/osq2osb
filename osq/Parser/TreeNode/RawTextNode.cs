using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq.Parser.TreeNode {
    class RawTextNode : NodeBase {
        public RawTextNode(string content, Location location) :
            base(content, location) {
        }

        public override string Execute(ExecutionContext context) {
            return this.Content;
        }

        public override string ToString() {
            return this.Content;
        }
    }
}
