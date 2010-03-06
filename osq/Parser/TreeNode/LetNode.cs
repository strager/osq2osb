using System;
using System.IO;

namespace osq.Parser.TreeNode {
    class LetNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public LetNode(DirectiveInfo info) :
            base(info) {
            Token token = Token.ReadToken(info.ParametersReader);

            if(token == null) {
                throw new InvalidDataException("Need a variable name for #let").AtLocation(info.ParametersReader.Location);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new InvalidDataException("Need a variable name for #let").AtLocation(token.Location);
            }

            this.Variable = token.Value.ToString();

            info.ParametersReader.SkipWhiteSpace();

            foreach(var node in Parser.ReadNodes(info.ParametersReader)) {
                this.ChildrenNodes.Add(node);
            }
        }

        protected override bool EndsWith(NodeBase node) {
            if(this.ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override string Execute(ExecutionContext context) {
            string varValue = ExecuteChildren(context);

            context.SetVariable(Variable, varValue.Trim(Environment.NewLine.ToCharArray()));

            return "";
        }
    }
}
