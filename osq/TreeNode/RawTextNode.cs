namespace osq.TreeNode {
    internal class RawTextNode : NodeBase {
        public string Content {
            get;
            private set;
        }

        public RawTextNode(string content, Location location) :
            base(location) {
            Content = content;
        }

        public override string Execute(ExecutionContext context) {
            return Content;
        }

        public override string ToString() {
            return Content;
        }
    }
}