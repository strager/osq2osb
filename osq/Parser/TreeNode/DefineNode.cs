using System;
using System.Collections.Generic;
using System.IO;

namespace osq.Parser.TreeNode {
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
                throw new InvalidDataException("Need a variable name for #define").AtLocation(startLocation);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new InvalidDataException("Need a variable name for #define").AtLocation(token.Location);
            }

            this.Variable = token.Value.ToString();

            if(reader.Peek() == '(') {
                token = Token.ReadToken(reader);

                while(token != null && !token.IsSymbol(")")) {
                    token = Token.ReadToken(reader);

                    if(token.TokenType == TokenType.Identifier) {
                        FunctionParameters.Add(token.Value.ToString());
                    }
                }

                if(token == null) {
                    throw new InvalidDataException("#define without closing parentheses").AtLocation(reader.Location);
                }
            }

            reader.SkipWhiteSpace();

            foreach(var node in Parser.ReadNodes(reader)) {
                this.ChildrenNodes.Add(node);
            }
        }

        protected override bool EndsWith(NodeBase node) {
            if(this.ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override string Execute(ExecutionContext context) {
            context.SetVariable(Variable, new Func<TokenNode, ExecutionContext, object>((TokenNode token, ExecutionContext subContext) => {
                var parameters = token.TokenChildren;

                if(parameters.Count == 1 && parameters[0].Token.IsSymbol(",")) {
                    parameters = parameters[0].TokenChildren;
                }

                int paramNumber = 0;

                foreach(var child in parameters) {
                    if(paramNumber >= FunctionParameters.Count) {
                        break;
                    }

                    object value = child.Evaluate(context);

                    subContext.SetVariable(FunctionParameters[paramNumber], value);

                    ++paramNumber;
                }

                string output = ExecuteChildren(context);

                return output.TrimEnd(Environment.NewLine.ToCharArray());
            }));

            return "";
        }
    }
}
