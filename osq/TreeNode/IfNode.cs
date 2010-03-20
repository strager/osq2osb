using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("if")]
    internal class IfNode : DirectiveNode {
        public TokenNode Condition {
            get;
            private set;
        }

        public IfNode(DirectiveInfo info) :
            base(info) {
            Condition = ExpressionRewriter.Rewrite(Token.ReadTokens(info.ParametersReader));
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        protected bool TestCondition(ExecutionContext context) {
            object val = Condition.Evaluate(context);

            if(val is double) {
                return (double)val != 0;
            } else if(val is string) {
                return !string.IsNullOrEmpty((string)val);
            } else if(val is Boolean) {
                return (Boolean)val;
            } else {
                throw new DataTypeException("Condition returns unknown data type", this);
            }
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            IEnumerable<NodeBase> nodes = ExecutableChildren;

            bool condition = TestCondition(context);

            while(true) {
                if(condition == true) {
                    nodes = nodes.TakeWhile((child) => !(child is ElseNode || child is ElseIfNode));

                    foreach(var node in nodes) {
                        output.Append(node.Execute(context));
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

                    condition = ((ElseIfNode)nextNode).TestCondition(context);
                }
            }

            return output.ToString();
        }
    }
}