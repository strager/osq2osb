using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq.Parser;

namespace osq.Parser.TreeNode {
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
            this.Token = token;
        }

        public override string Execute(ExecutionContext context) {
            return Evaluate(context).ToString();
        }

        public object Evaluate(ExecutionContext context) {
            switch(this.Token.Type) {
                case TokenType.Number:
                case TokenType.String:
                    return this.Token.Value;

                case TokenType.Identifier:
                case TokenType.Symbol:
                    object item = context.GetVariable(Token.Value.ToString());

                    var asFunc = item as Func<TokenNode, ExecutionContext, object>;

                    if(asFunc != null) {
                        return asFunc.Invoke(this, context);
                    }

                    return item;
            }

            throw new InvalidOperationException("Unknown token type");
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();
            var c = this.TokenChildren;

            str.Append(this.Token.Value.ToString());

            if(c.Count != 0) {
                str.Append("(");

                str.Append(string.Join(", ", c.Select(node => node.ToString()).ToArray()));

                str.Append(")");
            }

            return str.ToString();
        }
    }
}
