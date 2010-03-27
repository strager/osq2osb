using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osq.TreeNode {
    [DirectiveAttribute("let")]
    public class LetNode : DirectiveNode {
        public string Variable {
            get;
            private set;
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

            var shorthand = ReadShorthandNode(tokenReader);

            if(shorthand != null) {
                ChildrenNodes.Add(shorthand);
            } else {
                ChildrenNodes.AddMany(nodeReader.TakeWhile((node) => {
                    var endDirective = node as EndDirectiveNode;

                    if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                        return false;
                    }

                    return true;
                }));
            }
        }

        private NodeBase ReadShorthandNode(ITokenReader tokenReader) {
            ICollection<Token> tokens = new List<Token>();
            Token curToken;

            while((curToken = tokenReader.ReadToken()) != null) {
                tokens.Add(curToken);
            }

            if(tokens.Count((token) => token.TokenType != TokenType.WhiteSpace) == 0) {
                return null;
            }

            return ExpressionRewriter.Rewrite(tokens);
        }

        public override string Execute(ExecutionContext context) {
            string varValue = ExecuteChildren(context);

            context.SetVariable(Variable, varValue.Trim(Environment.NewLine.ToCharArray()));

            return "";
        }
    }
}