using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    abstract class NodeBase {
        public IList<NodeBase> ChildrenNodes {
            get;
            set;
        }

        public string Content {
            get;
            set;
        }

        public NodeBase() :
            this(null) {
        }
        
        public NodeBase(string content) {
            ChildrenNodes = new List<NodeBase>();
            Content = content;
        }

        public abstract void Execute(Parser parser, TextWriter output);
    }
}
