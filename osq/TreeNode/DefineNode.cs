using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("def(ine)?")]
    public class DefineNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public IList<string> FunctionParameters {
            get;
            private set;
        }

        public DefineNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            var startLocation = tokenReader.CurrentLocation;

            Token variable = tokenReader.ReadToken();

            if(variable == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(variable.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", variable.Location);
            }

            Variable = variable.ToString();

            FunctionParameters = new List<string>();

            Token token = tokenReader.ReadToken();

            if(token != null && token.IsSymbol("(")) {
                while(token != null && !token.IsSymbol(")")) {
                    token = tokenReader.ReadToken();

                    if(token.TokenType == TokenType.Identifier) {
                        FunctionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses");
                }

                token = null;
            }

            var shorthand = ReadShorthandNode(tokenReader, token);

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

        private NodeBase ReadShorthandNode(ITokenReader tokenReader, params Token[] previousTokens) {
            ICollection<Token> tokens = new List<Token>(previousTokens.Where((token) => token != null));
            Token curToken;

            while((curToken = tokenReader.ReadToken()) != null) {
                tokens.Add(curToken);
            }

            return ExpressionRewriter.Rewrite(tokens);
        }

        public override string Execute(ExecutionContext context) {
            context.SetVariable(Variable, new ExecutionContext.OsqFunction((token, subContext) => {
                subContext.PushScope();

                try {
                    var parameters = token == null ? new TokenNode[] { } : token.GetChildrenTokens();

                    if(parameters.Count == 1 && parameters[0].Token.IsSymbol(",")) {
                        parameters = parameters[0].GetChildrenTokens();
                    }

                    using(var paramNameEnumerator = FunctionParameters.GetEnumerator()) {
                        foreach(var child in parameters) {
                            if(!paramNameEnumerator.MoveNext()) {
                                break;
                            }

                            object value = child.Evaluate(context);

                            subContext.SetLocalVariable(paramNameEnumerator.Current, value);
                        }
                    }

                    string output = ExecuteChildren(context);

                    return output.TrimEnd(Environment.NewLine.ToCharArray());
                } finally {
                    subContext.PopScope();
                }
            }));

            return "";
        }
    }
}