﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq.TreeNode {
    // TODO Rewrite.
    [DirectiveAttribute("if")]
    public class IfNode : DirectiveNode {
        public TokenNode Condition {
            get;
            private set;
        }

        public IfNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Condition = ExpressionRewriter.Rewrite(tokenReader);

            ChildrenNodes.AddMany(nodeReader.TakeWhile((node) => {
                var endDirective = node as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    return false;
                }

                return true;
            }));
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            IEnumerable<NodeBase> nodes = ChildrenNodes;

            bool condition = context.IsTrue(Condition);

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

                    condition = context.IsTrue(((ElseIfNode)nextNode).Condition);
                }
            }

            return output.ToString();
        }
    }
}