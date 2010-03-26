using System;

namespace osq.TreeNode {
    [DirectiveAttribute("else")]
    public class ElseNode : DirectiveNode {
        public ElseNode(DirectiveInfo info) :
            base(info) {
        }

        public ElseNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            System.Diagnostics.Debug.Assert(false, "Cannot execute an else node");

            return "";
        }
    }
}