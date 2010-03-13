using System.Text;

namespace osq.TreeNode
{
    internal class LocalNode : DirectiveNode
    {
        public Token VariableName
        {
            get;
            private set;
        }

        public LocalNode(DirectiveInfo info) :
            base(info) {
            VariableName = Token.ReadToken(info.ParametersReader);

            if(VariableName == null) {
                throw new MissingDataException("Expected a variable name", info.ParametersReader.Location);
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

            context.SetLocalVariable(varName, context.VariableExists(varName) ? context.GetVariable(varName) : null);

            return "";
        }
    }
}