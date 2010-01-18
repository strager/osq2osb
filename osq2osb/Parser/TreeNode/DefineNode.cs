using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class DefineNode : DirectiveNode {
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

            var location = info.ParametersLocation.Clone();

            using(var reader = new StringReader(info.Parameters)) {
                Tokenizer.Token token = Tokenizer.ReadToken(reader, location);

                if(token == null) {
                    throw new ParserException("Need a variable name for #define", location);
                }

                if(token.Type != Tokenizer.TokenType.Identifier) {
                    throw new ParserException("Need a variable name for #define", token.Location);
                }

                this.Variable = token.Value.ToString();

                if(reader.Peek() == '(') {
                    token = Tokenizer.ReadToken(reader, location);

                    while(token != null && !(token.Type == Tokenizer.TokenType.Symbol && token.Value.ToString()[0] == ')')) {
                        token = Tokenizer.ReadToken(reader, location);

                        if(token.Type == Tokenizer.TokenType.Identifier) {
                            FunctionParameters.Add(token.Value.ToString());
                        }
                    }

                    if(token == null) {
                        throw new ParserException("#define without closing parentheses", location);
                    }
                }

                foreach(var node in Parser.Parse(reader, location)) {
                    this.ChildrenNodes.Add(node);
                }
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

        public override void Execute(TextWriter output, ExecutionContext context) {
            context.SetVariable(Variable, new Func<TokenNode, ExecutionContext, object>((TokenNode token, ExecutionContext subContext) => {
                var parameters = token.TokenChildren;

                if(parameters.Count == 1 && parameters[0].Token.Type == Tokenizer.TokenType.Symbol && parameters[0].Token.Value.ToString()[0] == ',') {
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

                using(var funcOutput = new StringWriter()) {
                    ExecuteChildren(funcOutput, context);

                    return funcOutput.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                }
            }));
        }
    }
}
