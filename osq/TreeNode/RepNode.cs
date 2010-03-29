using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("rep")]
    public class RepNode : DirectiveNode {
        public TokenNode Value {
            get;
            private set;
        }

        public IList<NodeBase> ChildrenNodes {
            get;
            private set;
        }

        public RepNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Value = ExpressionRewriter.Rewrite(tokenReader);

            ChildrenNodes = new List<NodeBase>(nodeReader.TakeWhile((node) => !IsEndDirective(node, DirectiveName)));
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            object value = Value.Evaluate(context);
            double count = (double)value;

            for(int i = 0; i < count; ++i) {
                output.Append(ExecuteChildren(context));
            }

            return output.ToString();
        }

        private string ExecuteChildren(ExecutionContext context) {
            var output = new StringBuilder();

            foreach(var child in ChildrenNodes) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}