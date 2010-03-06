﻿using System.Text;

namespace osq.Parser.TreeNode {
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

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            object value = Value.Evaluate(context);
            double count = (double)value;

            for(int i = 0; i < count; ++i) {
                output.Append(ExecuteChildren(context));
            }

            return output.ToString();
        }
    }
}
