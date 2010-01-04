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
                    throw new ParserException("Bad form for #" + DirectiveName + " directive", Parser, Location);
                }

                Name = match.Groups["name"].Value;
                FunctionParameters = match.Groups["params"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select((string s) => { return s.Trim(); }).ToList();
                
                this.ChildrenNodes.Clear();

                string data = match.Groups["value"].Value;

                using(StringReader reader = new StringReader(data)) {
                    NodeBase node = Parser.ReadNode(reader);

                    while(node != null) {
                        this.ChildrenNodes.Add(node);

                        node = Parser.ReadNode(reader);
                    }
                }

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

        public DefineNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            if(this.ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output) {
            Parser.SetVariable(Name, new Func<TokenNode, object>((TokenNode token) => {
                int paramNumber = 0;

                foreach(var child in token.TokenChildren) {
                    if(paramNumber >= FunctionParameters.Count) {
                        throw new ParserException("Invokation uses too many parameters", Parser, new Location());
                    }

                    object value = child.Value;

                    Parser.SetVariable(FunctionParameters[paramNumber], value);

                    ++paramNumber;
                }

                using(var funcOutput = new StringWriter()) {
                    ExecuteChildren(funcOutput);

                    return funcOutput.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                }
            }));
        }
    }
}
