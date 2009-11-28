using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class IfNode : DirectiveNode {
        public override string Parameters {
            set {
                Condition = value;

                base.Parameters = value;
            }
        }

        public string Condition {
            get;
            private set;
        }

        public IfNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        protected bool TestCondition() {
            string ret = Parser.ReplaceExpressions(Condition);
            double num;

            if(double.TryParse(ret, out num)) {
                return num != 0;
            } else {
                return !string.IsNullOrEmpty(ret);
            }
        }

        public override void Execute(TextWriter output) {
            IEnumerable<NodeBase> nodes = ExecutableChildren;

            bool condition = TestCondition();

            while(true) {
                if(condition == true) {
                    nodes = nodes.TakeWhile((child) => !(child is ElseNode || child is ElseIfNode));

                    foreach(var node in nodes) {
                        node.Execute(output);
                    }

                    break;
                } else {
                    nodes = nodes.SkipWhile((child) => !(child is ElseNode || child is ElseIfNode));

                    if(nodes.Count() == 0) {
                        break;
                    }

                    var nextNode = nodes.First();

                    nodes = nodes.Skip(1);

                    if(nextNode is ElseNode) {
                        condition = true;
                        continue;
                    }

                    condition = ((ElseIfNode)nextNode).TestCondition();
                }
            }
        }
    }
}
