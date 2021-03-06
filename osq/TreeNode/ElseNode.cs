﻿using System;
using System.Collections.Generic;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("else")]
    public class ElseNode : DirectiveNode {
        public ElseNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
        }

        public override string Execute(ExecutionContext context) {
            System.Diagnostics.Debug.Assert(false, "Cannot execute an else node");

            return "";
        }
    }
}