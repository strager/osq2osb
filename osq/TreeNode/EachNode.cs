using System.IO;
using System.Text;

namespace osq.TreeNode {
    internal class EachNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public TokenNode Values {
            get;
            private set;
        }

        public EachNode(DirectiveInfo info) :
            base(info) {
            Token token = Token.ReadToken(info.ParametersReader);

            if(token == null) {
                throw new InvalidDataException("Need a variable name for #let").AtLocation(info.ParametersReader.Location);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new InvalidDataException("Need a variable name for #let").AtLocation(token.Location);
            }

            Variable = token.Value.ToString();

            Values = ExpressionRewriter.Rewrite(Token.ReadTokens(info.ParametersReader));
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            object expr = Values.Evaluate(context);

            object[] values = expr as object[] ?? new object[] { expr };

            foreach(var value in values) {
                context.SetVariable(Variable, value);

                output.Append(ExecuteChildren(context));
            }

            return output.ToString();
        }
    }
}