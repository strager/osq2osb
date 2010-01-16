using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class ForNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<variable>\w+)\b\s*(?<values>.*)$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ParserException("Bad form for #" + DirectiveName + " directive", Parser, Location);
                }

                Variable = match.Groups["variable"].Value;

                string data = match.Groups["values"].Value;

                using(StringReader reader = new StringReader(data)) {
                    NodeBase node = Parser.ReadNode(reader);

                    while(node != null) {
                        this.Values.Add(node);

                        node = Parser.ReadNode(reader);
                    }
                }

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        public ICollection<NodeBase> Values {
            get;
            private set;
        }

        public ForNode(Parser parser, Location location) :
            base(parser, location) {
            Values = new List<NodeBase>();
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output) {
            double counter = double.NaN;

            while(true) {
                // Syntax: min max [step]
                string str;

                using(var writer = new StringWriter()) {
                    foreach(var node in Values) {
                        node.Execute(writer);
                    }

                    str = writer.ToString();
                }

                var parts = str.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length < 2 || parts.Length > 3) {
                    throw new ParserException("Bad #for form: " + str, Parser, Location);
                }

                double min = double.Parse(parts[0]);
                double max = double.Parse(parts[1]);
                double step = parts.Length >= 3 ? double.Parse(parts[2]) : 1;

                if(double.IsNaN(counter)) {
                    counter = min;
                }

                Parser.SetVariable(Variable, counter);

                ExecuteChildren(output);

                counter = System.Convert.ToDouble(Parser.GetVariable(Variable));
                counter += step;

                if(counter >= max) {
                    break;
                }
            }
        }
    }
}
