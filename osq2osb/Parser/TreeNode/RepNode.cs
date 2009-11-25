using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class RepNode : DirectiveNode {
        public override string Parameters {
            set {
                try {
                    Count = int.Parse(value);
                } catch(FormatException e) {
                    throw new ArgumentException("Bad form for #" + DirectiveName + " directive", e);
                }

                base.Parameters = value;
            }
        }

        public int Count {
            get;
            private set;
        }

        protected override bool IsMultiline {
            get {
                return true;
            }
        }

        public override void Execute(Parser parser, TextWriter output) {
            for(int i = 0; i < Count; ++i) {
                foreach(var child in ChildrenNodes) {
                    child.Execute(parser, output);
                }

                if(Content != null) {
                    var contentNode = new RawTextNode(Content);
                    contentNode.Execute(parser, output);
                }
            }
        }
    }
}
