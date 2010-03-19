using System.Text.RegularExpressions;

namespace osq.TreeNode {
    [DirectiveAttribute("end([^\\s]+)")]
    internal class EndDirectiveNode : DirectiveNode {
        public string TargetDirectiveName {
            get {
                var re = new Regex(@"^end");

                return re.Replace(DirectiveName, "");
            }
        }

        public EndDirectiveNode(DirectiveInfo info) :
            base(info) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            /* Do nothing. */
            return "";
        }
    }
}