using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RawTextNode : NodeBase {
        public override int LineNumber {
            get {
                return lineNumber ?? base.LineNumber;
            }

            set {
                lineNumber = value;
            }
        }

        private int? lineNumber;

        public RawTextNode(string content, Parser parser) :
            base(content, parser) {
        }

        public RawTextNode(string content, Parser parser, int lineNumber) :
            this(content, parser) {
            this.lineNumber = lineNumber;
        }

        public override void Execute(TextWriter output) {
            output.Write(Parser.ReplaceExpressions(this.Content));
        }
    }
}
