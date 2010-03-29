using System;
using System.Collections.Generic;
using System.Text;

namespace osq.TreeNode {
    public abstract class NodeBase {
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
            this.location = location;
        }

        public abstract string Execute(ExecutionContext context);
    }
}