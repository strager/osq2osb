using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RepNode : DirectiveNode {
        public override string Parameters {
            set {
                try {
                    Count = int.Parse(value);
                } catch(FormatException e) {
                    throw new ParserException("Bad form for #" + DirectiveName + " directive", Parser, Location, e);
                }

                base.Parameters = value;
            }
        }

        public int Count {
            get;
            private set;
        }

        public RepNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            var directive = node as DirectiveNode;

            if(directive != null && directive.DirectiveName == "end" + this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output) {
            for(int i = 0; i < Count; ++i) {
                ExecuteChildren(output);
            }
        }
    }
}
