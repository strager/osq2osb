using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    public abstract class NodeBase {
        public IList<NodeBase> ChildrenNodes {
            get;
            set;
        }

        public string Content {
            get;
            set;
        }

        public Location Location {
            get;
            set;
        }

        public Parser Parser {
            get;
            private set;
        }

        public NodeBase(Parser parser, Location location) :
            this(null, parser, location) {
        }
        
        public NodeBase(string content, Parser parser, Location location) {
            ChildrenNodes = new List<NodeBase>();
            Content = content;
            Parser = parser;
            Location = location;
        }

        public abstract void Execute(TextWriter output);

        public void ExecuteChildren(TextWriter output) {
            foreach(var child in ChildrenNodes) {
                child.Execute(output);
            }

            if(Content != null) {
                var contentNode = new RawTextNode(Content, Parser, Location);
                contentNode.Execute(output);
            }
        }
    }
}
