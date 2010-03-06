using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq;
using osq.TreeNode;

namespace osqReverser {
    class ConvertedNode {
        public string OriginalScript {
            get;
            set;
        }

        public NodeBase Node {
            get;
            set;
        }

        public string NodeOutput {
            get;
            set;
        }
    }
}
