using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("rep")]
    internal class RepNode : DirectiveNode {
        public TokenNode Value {
            get;
            private set;
        }

        public RepNode(DirectiveInfo info) :
            base(info) {
            var tokenReader = new TokenReader(info.ParametersReader);

            Value = ExpressionRewriter.Rewrite(tokenReader);
        }

        public RepNode(ITokenReader tokenReader, INodeReader nodeReader, Location location = null) :
            base(location) {
            Value = ExpressionRewriter.Rewrite(tokenReader);
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
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