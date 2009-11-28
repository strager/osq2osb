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
                Value = value;

                base.Parameters = value;
            }
        }

        public string Value {
            get;
            private set;
        }

        public RepNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output) {
            var value = Parser.ReplaceExpressions(Value);
            double count;

            try {
                count = double.Parse(value);
            } catch(FormatException e) {
                throw new ParserException("Bad form for #" + DirectiveName + " directive: " + value, Parser, Location, e);
            }

            for(int i = 0; i < count; ++i) {
                ExecuteChildren(output);
            }
        }
    }
}
