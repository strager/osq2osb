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

            public ConditionSet(TokenNode condition) :
                this(condition, new List<NodeBase>()) {
            }

            public ConditionSet(TokenNode condition, IList<NodeBase> children) {
                Condition = condition;
                ChildrenNodes = children;
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
            var curConditionSet = new ConditionSet(condition);

            Conditions.Add(curConditionSet);

            var childrenNodes = nodeReader.TakeWhile((node) => {
                var endDirective = node as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    return false;
                }

                return true;
            });

            foreach(var node in childrenNodes) {
                bool isConditional;
                var newCondition = GetConditionFromNode(node, out isConditional);

                if(isConditional) {
                    if(newCondition != null && curConditionSet.Condition == null) {
                        throw new BadDataException("Can't have an elseif node after an else node", node.Location);
                    }

                    curConditionSet = new ConditionSet(newCondition);

                    Conditions.Add(curConditionSet);

                    continue;
                }

                curConditionSet.ChildrenNodes.Add(node);
            }
        }

        private static TokenNode GetConditionFromNode(NodeBase node, out bool isConditional) {
            isConditional = true;

            var asElseIf = node as ElseIfNode;

            if(asElseIf != null) {
                return asElseIf.Condition;
            }

            var asElse = node as ElseNode;

            if(asElse != null) {
                return null;
            }

            isConditional = false;

            return null;
        }

        public ConditionSet GetTrueConditionSet(ExecutionContext context) {
            return Conditions.FirstOrDefault((conditionSet) => conditionSet.Condition == null || context.GetBoolFrom(conditionSet.Condition) == true);
        }

        public override string Execute(ExecutionContext context) {
            var conditionSet = GetTrueConditionSet(context);

            if(conditionSet == null) {
                return "";
            }

            var output = new StringBuilder();

            foreach(var child in conditionSet.ChildrenNodes) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}