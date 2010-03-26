using System;

namespace osq.TreeNode {
    [DirectiveAttribute("el(se)?if")]
    internal class ElseIfNode : IfNode {
        public ElseIfNode(DirectiveInfo info) :
            base(info) {
        }

        public ElseIfNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(tokenReader, nodeReader, directiveName, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            System.Diagnostics.Debug.Assert(false, "Cannot execute an elseif node");

            return "";
        }
    }
}