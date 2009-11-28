using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class EachNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<variable>\w+)\b\s*(?<values>.*)$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ParserException("Bad form for #" + DirectiveName + " directive", Parser, Location);
                }

                Variable = match.Groups["variable"].Value;
                Values = match.Groups["values"].Value;

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        public string Values {
            get;
            private set;
        }

        public EachNode(Parser parser, Location location) :
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
            var values = Parser.ReplaceExpressions(Values).Split(new char[] { ',' });

            foreach(var value in values) {
                Parser.SetVariable(Variable, value);

                ExecuteChildren(output);
            }
        }
    }
}
