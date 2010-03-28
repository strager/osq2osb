using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;
using osq.TreeNode;

namespace osq.Tests.Helpers {
    public class CollectionNodeReader : INodeReader {
        private IList<NodeBase> nodes;
        private int curNode;

        public Location Location {
            get {
                return null;
            }
        }

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

        public IEnumerable<NodeBase> ReadNodes() {
            NodeBase node;
            
            while((node = ReadNode()) != null) {
                yield return node;
            }
        }

        IEnumerator<NodeBase> IEnumerable<NodeBase>.GetEnumerator() {
            return ReadNodes().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ReadNodes().GetEnumerator();
        }
    }
}
