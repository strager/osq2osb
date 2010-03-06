using System;
using System.IO;
using System.Text;

namespace osq.TreeNode {
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
            var node = Parser.ExpressionToTokenNode(info.ParametersReader);

            if(!node.Token.IsSymbol(",")) {
                throw new InvalidDataException("Expected comma-separated list").AtLocation(Location);
            }

            var children = node.TokenChildren;

            if(children.Count < 3 || children.Count > 4) {
                throw new InvalidDataException("#for directive requires 3 to 4 parameters").AtLocation(Location);
            }

            if(children[0].Token.TokenType != TokenType.Identifier) {
                throw new InvalidDataException("Identifier expected").AtLocation(children[0].Location);
            }

            Variable = children[0].Token.ToString();
            Start = children[1];
            End = children[2];
            Step = children.Count > 3 ? children[3] : null;
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                return true;
            }

            return false;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            double counter = Convert.ToDouble(Start.Evaluate(context));

            while(true) {
                context.SetVariable(Variable, counter);

                output.Append(ExecuteChildren(context));

                counter = Convert.ToDouble(context.GetVariable(Variable));
                counter += Step == null ? 1.0 : Convert.ToDouble(Step.Evaluate(context));

                if(counter >= Convert.ToDouble(End.Evaluate(context))) {
                    break;
                }
            }

            return output.ToString();
        }
    }
}