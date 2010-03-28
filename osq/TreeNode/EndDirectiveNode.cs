using System.Text.RegularExpressions;
using osq.Parser;

namespace osq.TreeNode {
    [DirectiveAttribute("end([^\\s]+)")]
    public class EndDirectiveNode : DirectiveNode {
        public string TargetDirectiveName {
            get {
                var re = new Regex(@"^end");

                return re.Replace(DirectiveName, "");
            }
        }

        public EndDirectiveNode(ITokenReader tokenReader, INodeReader nodeReader, string directiveName = null, Location location = null) :
            base(directiveName, location) {
        }

        public override string Execute(ExecutionContext context) {
            /* Do nothing. */
            return "";
        }
    }
}