using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq.TreeNode {
    public class TokenNode : NodeBase {
        public Token Token {
            get;
            private set;
        }

        public IList<TokenNode> TokenChildren {
            get {
                return ChildrenNodes.Select((node) => node as TokenNode).Where((node) => node != null).ToList();
            }
        }

        public TokenNode(Token token, Location location) :
            base(null, location) {
            Token = token;
        }

        public override string Execute(ExecutionContext context) {
            return Evaluate(context).ToString();
        }

        public object Evaluate(ExecutionContext context) {
            switch(Token.TokenType) {
                case TokenType.Number:
                case TokenType.String:
                    return Token.Value;

                case TokenType.Identifier:
                case TokenType.Symbol:
                    object item = context.GetVariable(Token.Value.ToString());

                    var asFunc = item as Func<TokenNode, ExecutionContext, object>;

                    return asFunc == null ? item : asFunc.Invoke(this, context);
            }

            throw new InvalidOperationException("Unknown token type");
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();
            var c = TokenChildren;

            str.Append(Token.Value.ToString());

            if(c.Count != 0) {
                str.Append("(");

                str.Append(string.Join(", ", c.Select(node => node.ToString()).ToArray()));

                str.Append(")");
            }

            return str.ToString();
        }
    }
}