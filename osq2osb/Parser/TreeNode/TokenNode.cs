using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq2osb.Parser;

namespace osq2osb.Parser.TreeNode {
    public class TokenNode : NodeBase {
        public Tokenizer.Token Token {
            get;
            private set;
        }

        public object Value {
            get {
                return ExecuteToken();
            }
        }

        public IList<TokenNode> TokenChildren {
            get {
                return ChildrenNodes.Select((node) => node as TokenNode).Where((node) => node != null).ToList();
            }
        }

        public TokenNode(Tokenizer.Token token, Parser parser, Location location) :
            base(null, parser, location) {
            this.Token = token;
        }

        public override void Execute(TextWriter output) {
            output.Write(ExecuteToken());
        }

        protected object ExecuteToken() {
            switch(this.Token.Type) {
                case Tokenizer.TokenType.Number:
                case Tokenizer.TokenType.String:
                    return this.Token.Value;

                case Tokenizer.TokenType.Identifier:
                case Tokenizer.TokenType.Symbol:
                    object item = Parser.GetVariable(Token.Value.ToString());

                    var asFunc = item as Func<TokenNode, object>;

                    if(asFunc != null) {
                        return asFunc.Invoke(this);
                    }
                    
                    return item;
            }

            throw new InvalidOperationException("Unknown token type");
        }

        // XXX DEBUG
        private static int printDepth = 0;

        public void Print() {
            Console.WriteLine(new string(' ', printDepth) + Token.Value.ToString());
            ++printDepth;
            foreach(var child in TokenChildren) {
                child.Print();
            }
            --printDepth;
        }
    }
}
