﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    public class TokenNode : NodeBase {
        public Token Token {
            get;
            private set;
        }

        public override Location Location {
            get {
                return Token.Location;
            }

            set {
                Token.Location = value;
            }
        }

        public IList<TokenNode> ChildrenTokenNodes {
            get;
            private set;
        }

        public TokenNode(Token token, IEnumerable<TokenNode> children = null) {
            Token = token;

            if(children == null) {
                ChildrenTokenNodes = new List<TokenNode>();
            } else {
                ChildrenTokenNodes = children.ToList();
            }
        }

        public override string Execute(ExecutionContext context) {
            var ret = Evaluate(context);

            return context.GetStringOf(ret);
        }

        public object Evaluate(ExecutionContext context) {
            switch(Token.TokenType) {
                default:
                case TokenType.Number:
                case TokenType.String:
                    return Token.Value;

                case TokenType.Identifier:
                case TokenType.Symbol:
                    object item = context.GetVariable(Token.Value.ToString());

                    var asFunc = item as ExecutionContext.OsqFunction;

                    return asFunc == null ? item : asFunc.Invoke(this, context);
            }
        }

        public IEnumerable<object> GetOperatorParameters(ExecutionContext context) {
            return ChildrenTokenNodes.Select((tokenNode) => tokenNode.Evaluate(context));
        }

        public IEnumerable<object> GetFunctionParameters(ExecutionContext context) {
            if(Token != null && Token.IsSymbol(",")) {
                return Evaluate(context) as IEnumerable<object>;
            }

            if(ChildrenTokenNodes.Count == 1) {
                var child = ChildrenTokenNodes[0].Evaluate(context);
                var childAsEnumerable = child as IEnumerable<object>;

                return childAsEnumerable ?? new object[] { child };
            }

            return ChildrenTokenNodes.Select((tokenNode) => tokenNode.Evaluate(context));
        }

        public override string ToString() {
            StringBuilder str = new StringBuilder();
            var c = ChildrenTokenNodes;

            str.Append(Token.Value.ToString());

            if(c.Count != 0) {
                str.Append("(");

                str.Append(string.Join(", ", c.Select(node => node.ToString()).ToArray()));

                str.Append(")");
            }

            return str.ToString();
        }

       public override bool Equals(object obj) {
           var objAsTokenNode = obj as TokenNode;

           if(objAsTokenNode == null) {
               return false;
           }

           return Equals(objAsTokenNode);
        }

        public bool Equals(TokenNode otherTokenNode) {
            if(otherTokenNode == null) {
                return false;
            }

            if(otherTokenNode.Token == null) {
                if(this.Token != null) {
                    return false;
                }
            } else if(!otherTokenNode.Token.Equals(this.Token)) {
                return false;
            }

            if(!otherTokenNode.ChildrenTokenNodes.SequenceEqual(this.ChildrenTokenNodes)) {
                return false;
            }

            return true;
        }
    }
}