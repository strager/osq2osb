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

            Console.WriteLine("Hi " + this.Condition.ToString() + " from " + info.Parameters);
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        protected bool TestCondition(ExecutionContext context) {
            Console.WriteLine("Testing condition " + Condition.ToString());

            object val = Condition.Evaluate(context);

            Console.WriteLine(" = " + val.ToString() + " (" + val.GetType().ToString() + ")");

            if(val is double) {
                return (double)val != 0;
            } else if(val is string) {
                return !string.IsNullOrEmpty((string)val);
            } else if(val is Boolean) {
                return (Boolean)val;
            } else {
                throw new ExecutionException("Condition returns unknown data type", this.Location);
            }
        }

        public override void Execute(TextWriter output, ExecutionContext context) {
            IEnumerable<NodeBase> nodes = ExecutableChildren;

            bool condition = TestCondition(context);

            while(true) {
                Console.WriteLine(condition);

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

                    Console.WriteLine("nextNode = " + nextNode.ToString());

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
