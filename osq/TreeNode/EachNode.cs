using System.IO;
using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("each")]
    public class EachNode : DirectiveNode {
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
            var tokenReader = new TokenReader(info.ParametersReader);
            Token token = tokenReader.ReadToken();

            if(token == null) {
                throw new MissingDataException("Variable name", info.ParametersReader.Location);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", token.Location);
            }

            Variable = token.Value.ToString();

            Values = ExpressionRewriter.Rewrite(tokenReader);
        }

        public EachNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            var startLocation = tokenReader.CurrentLocation;

            Token token = tokenReader.ReadToken();

            if(token == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", token.Location);
            }

            Variable = token.Value.ToString();

            Values = ExpressionRewriter.Rewrite(tokenReader);
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
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