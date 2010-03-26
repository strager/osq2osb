using System;
using System.IO;

namespace osq.TreeNode {
    [DirectiveAttribute("let")]
    public class LetNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public LetNode(DirectiveInfo info) :
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

            info.ParametersReader.SkipWhiteSpace();

            foreach(var node in (new Parser(info.Parser, info.ParametersReader)).ReadNodes()) {
                ChildrenNodes.Add(node);
            }
        }

        public LetNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
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

            NodeBase node;

            while((node = nodeReader.ReadNode()) != null) {
                ChildrenNodes.Add(node);
            }
        }

        protected override bool EndsWith(NodeBase node) {
            if(ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            string varValue = ExecuteChildren(context);

            context.SetVariable(Variable, varValue.Trim(Environment.NewLine.ToCharArray()));

            return "";
        }
    }
}