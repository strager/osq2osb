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

        public abstract string Execute(ExecutionContext context);

        public string ExecuteChildren(ExecutionContext context) {
            var output = new StringBuilder();

            foreach(var child in ExecutableChildren) {
                output.Append(child.Execute(context));
            }

            return output.ToString();
        }
    }
}
