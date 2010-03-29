using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("if")]
    public class IfNode : DirectiveNode {
        public class ConditionSet {
            public IList<NodeBase> ChildrenNodes {
                get;
                set;
            }

            public TokenNode Condition {
                get;
                set;
            }

            public string ExecuteChildren(ExecutionContext context) {
                var output = new StringBuilder();

                foreach(var child in ChildrenNodes) {
                    output.Append(child.Execute(context));
                }

                return output.ToString();
            }
        }

        public IList<ConditionSet> Conditions {
            get;
            set;
        }

        // TODO Clean up.
        public IfNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Conditions = new List<ConditionSet>();

            var condition = ExpressionRewriter.Rewrite(tokenReader);

            var curConditionSet = new ConditionSet {
                ChildrenNodes = new List<NodeBase>(),
                Condition = condition
            };

            Conditions.Add(curConditionSet);

            NodeBase node;

            while((node = nodeReader.ReadNode()) != null) {
                var endDirective = node as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    break;
                }

                var elseIfNode = node as ElseIfNode;

                if(elseIfNode != null) {
                    if(curConditionSet.Condition == null) {
                        throw new BadDataException("Can't have an elseif node after an else node", node.Location);
                    }

                    curConditionSet = new ConditionSet {
                        ChildrenNodes = new List<NodeBase>(),
                        Condition = elseIfNode.Condition
                    };

                    Conditions.Add(curConditionSet);

                    continue;
                }

                if(node is ElseNode) {
                    curConditionSet = new ConditionSet {
                        ChildrenNodes = new List<NodeBase>(),
                        Condition = null
                    };

                    Conditions.Add(curConditionSet);

                    continue;
                }

                curConditionSet.ChildrenNodes.Add(node);
            }
        }

        public ConditionSet GetTrueConditionSet(ExecutionContext context) {
            return Conditions.FirstOrDefault((conditionSet) => conditionSet.Condition == null || context.GetBoolFrom(conditionSet.Condition) == true);
        }

        public override string Execute(ExecutionContext context) {
            var conditionSet = GetTrueConditionSet(context);

            if(conditionSet == null) {
                return "";
            }

            return conditionSet.ExecuteChildren(context);
        }
    }
}