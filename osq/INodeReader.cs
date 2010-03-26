using osq.TreeNode;

namespace osq {
    public interface INodeReader {
        Location Location {
            get;
        }

        NodeBase ReadNode();
    }
}
