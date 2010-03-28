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

        public RepNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
            Value = ExpressionRewriter.Rewrite(tokenReader);

            ChildrenNodes.AddMany(nodeReader.TakeWhile((node) => {
                var endDirective = node as EndDirectiveNode;

                if(endDirective != null && endDirective.TargetDirectiveName == DirectiveName) {
                    return false;
                }

                return true;
            }));
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
    }
}