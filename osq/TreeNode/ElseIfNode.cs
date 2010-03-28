using System;
using osq.Parser;

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

        public override string Execute(ExecutionContext context) {
            System.Diagnostics.Debug.Assert(false, "Cannot execute an elseif node");

            return "";
        }
    }
}