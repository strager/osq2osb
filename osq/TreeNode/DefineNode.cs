using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("def(ine)?")]
    public class DefineNode : DirectiveNode {
        public string Variable {
            get;
            set;
        }

        public IEnumerable<string> FunctionParameters {
            get;
            set;
        }

        public IEnumerable<NodeBase> ChildrenNodes {
            get;
            set;
        }

        public DefineNode(string directiveName = null, Location location = null) :
            base(directiveName, location) {
        }

        public DefineNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            this(directiveName, location) {
            var startLocation = tokenReader.CurrentLocation;

            Token variable = tokenReader.ReadToken();

            if(variable == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(variable.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", variable.Location);
            }

            Variable = variable.ToString();

            // TODO Extract method.
            var functionParameters = new List<string>();
            FunctionParameters = functionParameters;

            Token token = tokenReader.ReadToken();

            if(token != null && token.IsSymbol("(")) {
                while(token != null && !token.IsSymbol(")")) {
                    token = tokenReader.ReadToken();

                    if(token.TokenType == TokenType.Identifier) {
                        functionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses");
                }

                token = null;
            }

            var shorthand = ReadShorthandNode(tokenReader, token);

            if(shorthand != null) {
                ChildrenNodes = new List<NodeBase> { shorthand };
            } else {
                ChildrenNodes = new List<NodeBase>(nodeReader.TakeWhile((node) => !IsEndDirective(node, DirectiveName)));
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
                    var parameters = token == null ? new TokenNode[] { } : token.ChildrenTokenNodes;

                    if(parameters.Count == 1 && parameters[0].Token.IsSymbol(",")) {
                        parameters = parameters[0].ChildrenTokenNodes;
                    }

                    var parameterVariables = FunctionParameters ?? new string[] { };

                    using(var paramNameEnumerator = parameterVariables.GetEnumerator()) {
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

        private string ExecuteChildren(ExecutionContext context) {
            if(ChildrenNodes == null) {
                return "";
            }

            var output = new StringBuilder();

            foreach(var child in ChildrenNodes) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}