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
                    throw new ArgumentException("Bad form for #" + DirectiveName + " directive");
                }

                Variable = match.Groups["variable"].Value;
                Values = match.Groups["values"].Value.Split(new char[] { ',' });

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        public IList<string> Values {
            get;
            private set;
        }

        protected override bool IsMultiline {
            get {
                return true;
            }
        }

        public EachNode(Parser parser) :
            base(parser) {
        }

        public override void Execute(TextWriter output) {
            foreach(var value in Values) {
                Parser.SetVariable(Variable, value);

                ExecuteChildren(output);
            }
        }
    }
}
