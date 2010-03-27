﻿using System;

namespace osq.TreeNode {
    [DirectiveAttribute("el(se)?if")]
    public class ElseIfNode : IfNode {
        public ElseIfNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(tokenReader, nodeReader, directiveName, location) {
        }

        public override string Execute(ExecutionContext context) {
            System.Diagnostics.Debug.Assert(false, "Cannot execute an elseif node");

            return "";
        }
    }
}