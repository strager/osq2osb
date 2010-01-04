using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser.TreeNode {
    abstract class DirectiveNode : NodeBase {
        private static IDictionary<string, Type> directiveTypes = new Dictionary<string, Type>();

        static DirectiveNode() {
            directiveTypes["def(ine)?"] = typeof(DefineNode);
            directiveTypes["let"] = typeof(LetNode);
            directiveTypes["each"] = typeof(EachNode);
            directiveTypes["rep"] = typeof(RepNode);
            directiveTypes["for"] = typeof(ForNode);
            directiveTypes["inc(lude)?"] = typeof(IncludeNode);
            directiveTypes["if"] = typeof(IfNode);
            directiveTypes["else"] = typeof(ElseNode);
            directiveTypes["el(se)?if"] = typeof(ElseIfNode);
            directiveTypes["end([^\\s]+)"] = typeof(EndDirectiveNode);
        }

        public virtual string Parameters {
            get;
            set;
        }

        public string DirectiveName {
            get;
            set;
        }

        protected DirectiveNode(Parser parser, Location location) :
            base(parser, location) {
        }

        protected abstract bool EndsWith(NodeBase node);

        public static DirectiveNode Create(string line, TextReader input, Parser parser, Location location) {
            foreach(var pair in directiveTypes) {
                string type = pair.Key;
                Type nodeType = pair.Value;

                Regex re = new Regex("^#(?<name>" + type + ")\\s*(?<params>.*)$", RegexOptions.ExplicitCapture);

                var match = re.Match(line);

                if(!match.Success) {
                    continue;
                }

                string name = match.Groups["name"].Value;
                string parameters = match.Groups["params"].Value;

                var ctor = nodeType.GetConstructor(new Type[] { typeof(Parser), typeof(Location) });
                var newNode = ctor.Invoke(new object[] { parser, location }) as DirectiveNode;
                newNode.Parameters = parameters;
                newNode.DirectiveName = name;

                NodeBase curNode = newNode;

                while(!newNode.EndsWith(curNode)) {
                    curNode = parser.ReadNode(input);
                    
                    if(curNode == null) {
                        throw new ParserException("Unmatched #" + name + " directive", parser, location);
                    }

                    newNode.ChildrenNodes.Add(curNode);
                }

                return newNode;
            }

            throw new ParserException("Unknown directive: " + line, parser, location);
        }
    }
}
