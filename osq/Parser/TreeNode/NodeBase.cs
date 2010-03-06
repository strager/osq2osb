using System.Collections.Generic;
using System.Text;

namespace osq.Parser.TreeNode {
    public abstract class NodeBase {
        public IList<NodeBase> ChildrenNodes {
            get;
            private set;
        }

        public IEnumerable<NodeBase> ExecutableChildren {
            get {
                foreach(var child in ChildrenNodes) {
                    yield return child;
                }

                if(Content != null) {
                    var contentNode = new RawTextNode(Content, Location);
                    yield return contentNode;
                }
            }
        }

        public string Content {
            get;
            set;
        }

        public Location Location {
            get;
            set;
        }

        protected NodeBase(Location location) :
            this(null, location) {
        }

        protected NodeBase(string content, Location location) {
            ChildrenNodes = new List<NodeBase>();
            Content = content;
            Location = location;
        }

        public abstract string Execute(ExecutionContext context);

        public string ExecuteChildren(ExecutionContext context) {
            var output = new StringBuilder();

            foreach(var child in ExecutableChildren) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}
