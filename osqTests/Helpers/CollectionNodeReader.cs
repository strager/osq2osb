using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.TreeNode;

namespace osq.Tests.Helpers {
    public class CollectionNodeReader : INodeReader {
        private IList<NodeBase> nodes;
        private int curNode;

        public CollectionNodeReader(IEnumerable<NodeBase> nodes) {
            this.nodes = nodes.ToList();
            this.curNode = 0;
        }

        public NodeBase ReadNode() {
            if(this.curNode >= this.nodes.Count) {
                return null;
            }

            return this.nodes[this.curNode++];
        }
    }
}
