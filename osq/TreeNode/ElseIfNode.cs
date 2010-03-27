using System;

namespace osq.TreeNode {
    [DirectiveAttribute("el(se)?if")]
    public class ElseIfNode : DirectiveNode {
        public TokenNode Condition {
            get;
            private set;
        }

        public ElseIfNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Condition = ExpressionRewriter.Rewrite(tokenReader);
        }

        public bool TestCondition(ExecutionContext context) {
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
            System.Diagnostics.Debug.Assert(false, "Cannot execute an elseif node");

            return "";
        }
    }
}