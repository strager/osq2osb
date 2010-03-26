using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public DefineNode(DirectiveInfo info) :
            base(info) {
            var reader = info.ParametersReader;
            var startLocation = reader.Location.Clone();

            Variable = ReadVariableName(reader, startLocation);
            FunctionParameters = ReadParameters(reader).ToList();

            reader.SkipWhiteSpace();

            foreach(var node in ReadInlineData(info, reader)) {
                ChildrenNodes.Add(node);
            }
        }

        public DefineNode(ITokenReader tokenReader, INodeReader nodeReader, Location location = null) :
            base(location) {
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

            if(tokenReader.PeekToken().IsSymbol("(")) {
                Token token = tokenReader.ReadToken();

                while(token != null && !token.IsSymbol(")")) {
                    token = tokenReader.ReadToken();

                    if(token.TokenType == TokenType.Identifier) {
                        FunctionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses");
                }
            }

            ICollection<Token> tmp = new List<Token>();
            Token t;

            while((t = tokenReader.ReadToken()) != null) {
                tmp.Add(t);
            }

            ChildrenNodes.Add(ExpressionRewriter.Rewrite(tmp));
        }

        private IEnumerable<NodeBase> ReadInlineData(DirectiveInfo info, LocatedTextReaderWrapper reader) {
            return (new Parser(info.Parser, reader)).ReadNodes();
        }

        private IEnumerable<string> ReadParameters(LocatedTextReaderWrapper reader) {
            var tokenReader = new TokenReader(reader);

            if(reader.Peek() == '(') {
                Token token = tokenReader.ReadToken();

                while(token != null && !token.IsSymbol(")")) {
                    token = tokenReader.ReadToken();

                    if(token.TokenType == TokenType.Identifier) {
                        yield return token.Value.ToString();
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses", reader.Location);
                }
            }
        }

        private string ReadVariableName(LocatedTextReaderWrapper reader, Location startLocation) {
            var tokenReader = new TokenReader(reader);
            Token token = tokenReader.ReadToken();

            if(token == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", token.Location);
            }

            return token.Value.ToString();
        }

        protected override bool EndsWith(NodeBase node) {
            if(ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            context.SetVariable(Variable, new ExecutionContext.OsqFunction((token, subContext) => {
                subContext.PushScope();

                try {
                    var parameters = token.GetChildrenTokens();

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