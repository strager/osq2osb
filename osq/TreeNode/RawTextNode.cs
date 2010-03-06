namespace osq.TreeNode {
    internal class RawTextNode : NodeBase {
        public RawTextNode(string content, Location location) :
            base(content, location) {
        }

        public override string Execute(ExecutionContext context) {
            return Content;
        }

        public override string ToString() {
            return Content;
        }
    }
}