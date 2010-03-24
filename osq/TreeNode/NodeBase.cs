using System;
using System.Collections.Generic;
using System.Text;

namespace osq.TreeNode {
    public abstract class NodeBase {
        public IList<NodeBase> ChildrenNodes {
            get;
            private set;
        }

        [Obsolete("Use ChildrenNodes instead")]
        public IEnumerable<NodeBase> ExecutableChildren {
            get {
                return ChildrenNodes;
            }
        }

        private Location location;

        public virtual Location Location {
            get {
                return this.location;
            }

            set {
                this.location = value;
            }
        }

        protected NodeBase(Location location = null) :
            this(null, location) {
        }

        protected NodeBase(string content, Location location = null) {
            ChildrenNodes = new List<NodeBase>();
            this.location = location;
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