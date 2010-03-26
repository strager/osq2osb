using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("local")]
    public class LocalNode : DirectiveNode {
        public Token VariableName {
            get;
            private set;
        }

        public LocalNode(DirectiveInfo info) :
            base(info) {
            var tokenReader = new TokenReader(info.ParametersReader);

            VariableName = tokenReader.ReadToken();

            if(VariableName == null) {
                throw new MissingDataException("Expected a variable name", info.ParametersReader.Location);
            }

            if(VariableName.TokenType != TokenType.Identifier) {
                throw new BadDataException("Expected a variable name", VariableName.Location);
            }
        }

        public LocalNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            var startLocation = tokenReader.CurrentLocation;

            VariableName = tokenReader.ReadToken();

            if(VariableName == null) {
                throw new MissingDataException("Expected a variable name", startLocation);
            }

            if(VariableName.TokenType != TokenType.Identifier) {
                throw new BadDataException("Expected a variable name", VariableName.Location);
            }
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            string varName = VariableName.ToString();

            context.SetLocalVariable(varName, context.IsVariableSet(varName) ? context.GetVariable(varName) : null);

            return "";
        }
    }
}