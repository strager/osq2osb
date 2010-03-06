using System.Text.RegularExpressions;

namespace osq.Parser.TreeNode {
    class EndDirectiveNode : DirectiveNode {
        public string TargetDirectiveName {
            get {
                var re = new Regex(@"^end");

                return re.Replace(this.DirectiveName, "");
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
