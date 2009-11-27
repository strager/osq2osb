using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class IncludeNode : DirectiveNode {
        public override string Parameters {
            set {
                Filename = value;
                
                base.Parameters = value;
            }
        }

        public string Filename {
            get;
            private set;
        }

        public IncludeNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override void Execute(TextWriter output) {
            using(var inputFile = File.Open(Filename, FileMode.Open, FileAccess.Read)) {
                using(var reader = new StreamReader(inputFile)) {
                    Parser.ParseAndExecute(reader, output);
                }
            }
        }
    }
}
