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

        public IEnumerable<NodeBase> ExecutableChildren {
            get {
                foreach(var child in ChildrenNodes) {
                    yield return child;
                }

                if(Content != null) {
                    var contentNode = new RawTextNode(Content, Location);
                    yield return contentNode;
                }
            }
        }

        public string Content {
            get;
            set;
        }

        public Location Location {
            get;
            set;
        }

        public NodeBase(Location location) :
            this(null, location) {
        }
        
        public NodeBase(string content, Location location) {
            ChildrenNodes = new List<NodeBase>();
            Content = content;
            Location = location;
        }

        public abstract void Execute(TextWriter output, ExecutionContext context);

        public void ExecuteChildren(TextWriter output, ExecutionContext context) {
            foreach(var child in ExecutableChildren) {
                child.Execute(output, context);
            }
        }
    }
}
