using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class LetNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<variable>\w+)\b\s*(?<value>.*)\s*$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ArgumentException("Bad form for #" + DirectiveName + " directive");
                }

                Variable = match.Groups["variable"].Value;
                Content = match.Groups["value"].Value;
                isMultiline = Content.Trim() == "";

                base.Parameters = value;
            }
        }

        public string Variable {
            get;
            private set;
        }

        protected override bool IsMultiline {
            get {
                return isMultiline;
            }
        }

        private bool isMultiline = false;

        public override void Execute(Parser parser, TextWriter output) {
            using(var varWriter = new StringWriter()) {
                foreach(var child in ChildrenNodes) {
                    child.Execute(parser, varWriter);
                }

                if(Content != null) {
                    var contentNode = new RawTextNode(Content);
                    contentNode.Execute(parser, varWriter);
                }

                parser.SetVariable(Variable, varWriter.ToString().Trim(Environment.NewLine.ToCharArray()));
            }

        }
    }
}
