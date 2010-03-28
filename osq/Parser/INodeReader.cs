using System.Collections.Generic;
using osq.TreeNode;

namespace osq.Parser {
    public interface INodeReader : IEnumerable<NodeBase> {
        Location Location {
            get;
        }

        NodeBase ReadNode();
    }
}


