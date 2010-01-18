using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class IfNode : DirectiveNode {
        public TokenNode Condition {
            get;
            private set;
        }

        public IfNode(DirectiveInfo info) :
            base(info) {
            Condition = Parser.ExpressionToTokenNode(info.Parameters, info.ParametersLocation);
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        protected bool TestCondition(ExecutionContext context) {
            object val = Condition.Evaluate(context);
            
            if(val is double) {
                return (double)val != 0;
            } else if(val is string) {
                return !string.IsNullOrEmpty((string)val);
            } else {
                throw new InvalidDataException("Condition returns unknown data type");
            }
        }

        public override void Execute(TextWriter output, ExecutionContext context) {
            IEnumerable<NodeBase> nodes = ExecutableChildren;

            bool condition = TestCondition(context);

            while(true) {
                if(condition == true) {
                    nodes = nodes.TakeWhile((child) => !(child is ElseNode || child is ElseIfNode));

                    foreach(var node in nodes) {
                        node.Execute(output, context);
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
        }
    }
}
