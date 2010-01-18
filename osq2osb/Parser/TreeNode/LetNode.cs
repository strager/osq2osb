using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class LetNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public LetNode(DirectiveInfo info) :
            base(info) {
            var location = info.ParametersLocation.Clone();

            using(var reader = new StringReader(info.Parameters)) {
                Tokenizer.Token token = Tokenizer.ReadToken(reader, location);

                if(token == null) {
                    throw new ParserException("Need a variable name for #let", location);
                }

                if(token.Type != Tokenizer.TokenType.Identifier) {
                    throw new ParserException("Need a variable name for #let", token.Location);
                }

                this.Variable = token.Value.ToString();

                reader.SkipWhitespace();

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
            using(var varWriter = new StringWriter()) {
                ExecuteChildren(varWriter, context);

                context.SetVariable(Variable, varWriter.ToString().Trim(Environment.NewLine.ToCharArray()));
            }
        }
    }
}
