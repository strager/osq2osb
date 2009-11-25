using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class DefineNode : DirectiveNode {
        public override string Parameters {
            set {
                var re = new Regex(@"^(?<name>\w+)(\((?<params>[^)]*)\)|(?<params>))\s*(?<value>.*)\s*$", RegexOptions.ExplicitCapture);
                var match = re.Match(value);

                if(!match.Success) {
                    throw new ArgumentException("Bad form for #" + DirectiveName + " directive");
                }

                Name = match.Groups["name"].Value;
                FunctionParameters = match.Groups["params"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select((string s) => { return s.Trim(); }).ToList();
                Content = match.Groups["value"].Value;
                isMultiline = Content.Trim() == "";

                base.Parameters = value;
            }
        }

        public string Name {
            get;
            private set;
        }

        public IList<string> FunctionParameters {
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
            parser.SetVariable(Name, new Action<Stack<object>>((Stack<object> stack) => {
                foreach(string paramName in FunctionParameters.Reverse()) { // Reverse because pop order is backwards.
                    parser.SetVariable(paramName, stack.Pop());
                }

                using(var stackOutput = new StringWriter()) {
                    foreach(var child in ChildrenNodes) {
                        child.Execute(parser, stackOutput);
                    }

                    if(Content != null) {
                        var contentNode = new RawTextNode(Content);
                        contentNode.Execute(parser, stackOutput);
                    }

                    stack.Push(stackOutput.ToString().Trim(Environment.NewLine.ToCharArray()));
                }
            }));
        }
    }
}
