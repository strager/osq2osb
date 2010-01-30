using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RepNode : DirectiveNode {
        public TokenNode Value {
            get;
            private set;
        }

        public RepNode(DirectiveInfo info) :
            base(info) {
            Value = Parser.ExpressionToTokenNode(info.ParametersReader);
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output, ExecutionContext context) {
            object value = Value.Evaluate(context);
            double count = (double)value;

            for(int i = 0; i < count; ++i) {
                ExecuteChildren(output, context);
            }
        }
    }
}
