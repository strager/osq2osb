using System;
using System.Collections.Generic;
using System.Text;

namespace osq.TreeNode {
    public abstract class NodeBase {
        public IList<NodeBase> ChildrenNodes {
            get;
            private set;
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

        protected NodeBase(Location location = null) {
            ChildrenNodes = new List<NodeBase>();
            this.location = location;
        }

        public abstract string Execute(ExecutionContext context);

        public string ExecuteChildren(ExecutionContext context) {
            var output = new StringBuilder();

            foreach(var child in ChildrenNodes) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}