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

        public virtual int LineNumber {
            get {
                return LineNumber;
            }
        }

        public Parser Parser {
            get;
            private set;
        }

        private int lineNumber;

        public NodeBase(Parser parser) :
            this(null, parser) {
        }
        
        public NodeBase(string content, Parser parser) {
            ChildrenNodes = new List<NodeBase>();
            Content = content;
            Parser = parser;
            lineNumber = parser.LineNumber;
        }

        public abstract void Execute(TextWriter output);
    }
}
