using System;
using System.IO;
using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("for")]
    internal class ForNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public TokenNode Start {
            get;
            private set;
        }

        public TokenNode End {
            get;
            private set;
        }

        public TokenNode Step {
            get;
            private set;
        }

        public ForNode(DirectiveInfo info) :
            base(info) {
            var tokenReader = new TokenReader(info.ParametersReader);
            var node = ExpressionRewriter.Rewrite(tokenReader);

            if(!node.Token.IsSymbol(",")) {
                throw new DataTypeException("Expected comma-separated list", this);
            }

            var children = node.GetChildrenTokens();

            if(children.Count < 3 || children.Count > 4) {
                throw new MissingDataException("#for directive requires 3 to 4 parameters", info.Location);
            }

            if(children[0].Token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Identifier", children[0].Location);
            }

            Variable = children[0].Token.ToString();
            Start = children[1];
            End = children[2];
            Step = children.Count > 3 ? children[3] : null;
        }

        public ForNode(ITokenReader tokenReader, INodeReader nodeReader, Location location = null) :
            base(location) {
            var node = ExpressionRewriter.Rewrite(tokenReader);

            if(!node.Token.IsSymbol(",")) {
                throw new DataTypeException("Expected comma-separated list", this);
            }

            var children = node.GetChildrenTokens();

            if(children.Count < 3 || children.Count > 4) {
                throw new MissingDataException("#for directive requires 3 to 4 parameters", location);
            }

            if(children[0].Token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Identifier", children[0].Location);
            }

            Variable = children[0].Token.ToString();
            Start = children[1];
            End = children[2];
            Step = children.Count > 3 ? children[3] : null;
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            double counter = Convert.ToDouble(Start.Evaluate(context), Parser.DefaultCulture);

            while(true) {
                context.SetVariable(Variable, counter);

                output.Append(ExecuteChildren(context));

                counter = Convert.ToDouble(context.GetVariable(Variable), Parser.DefaultCulture);
                counter += Step == null ? 1.0 : Convert.ToDouble(Step.Evaluate(context), Parser.DefaultCulture);

                if(counter >= Convert.ToDouble(End.Evaluate(context), Parser.DefaultCulture)) {
                    break;
                }
            }

            return output.ToString();
        }
    }
}