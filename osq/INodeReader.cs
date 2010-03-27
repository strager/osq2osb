using System.Collections.Generic;
using osq.TreeNode;

namespace osq {
    public interface INodeReader : IEnumerable<NodeBase> {
        Location Location {
            get;
        }

        NodeBase ReadNode();
    }
}
