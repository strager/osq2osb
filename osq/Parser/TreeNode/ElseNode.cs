﻿using System;

namespace osq.Parser.TreeNode {
    class ElseNode : DirectiveNode {
        public ElseNode(DirectiveInfo info) :
            base(info) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            throw new InvalidOperationException("Cannot execute an else node");
        }
    }
}
