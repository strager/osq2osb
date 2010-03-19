using System;

namespace osq.TreeNode {
    [DirectiveAttribute("else")]
    internal class ElseNode : DirectiveNode {
        public ElseNode(DirectiveInfo info) :
            base(info) {
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