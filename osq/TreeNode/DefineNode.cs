using System;
using System.Collections.Generic;
using System.IO;

namespace osq.TreeNode {
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
            FunctionParameters = new List<string>();

            var reader = info.ParametersReader;
            var startLocation = reader.Location.Clone();

            Token token = Token.ReadToken(reader);

            if(token == null) {
                throw new MissingDataException("Variable name", startLocation);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", token.Location);
            }

            Variable = token.Value.ToString();

            if(reader.Peek() == '(') {
                token = Token.ReadToken(reader);

                while(token != null && !token.IsSymbol(")")) {
                    token = Token.ReadToken(reader);

                    if(token.TokenType == TokenType.Identifier) {
                        FunctionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new MissingDataException("Closing parentheses", reader.Location);
                }
            }

            reader.SkipWhiteSpace();

            foreach(var node in Parser.ReadNodes(reader)) {
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
            context.SetVariable(Variable, new Func<TokenNode, ExecutionContext, object>((token, subContext) => {
                var parameters = token.TokenChildren;

                if(parameters.Count == 1 && parameters[0].Token.IsSymbol(",")) {
                    parameters = parameters[0].TokenChildren;
                }

                using(var paramNameEnumerator = FunctionParameters.GetEnumerator()) {
                    foreach(var child in parameters) {
                        if(!paramNameEnumerator.MoveNext()) {
                            break;
                        }

                        object value = child.Evaluate(context);

                        subContext.SetVariable(paramNameEnumerator.Current, value);
                    }
                }

                string output = ExecuteChildren(context);

                return output.TrimEnd(Environment.NewLine.ToCharArray());
            }));

            return "";
        }
    }
}