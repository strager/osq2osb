using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser.TreeNode {
    abstract class DirectiveNode : NodeBase {
        private static IDictionary<Regex, Type> directiveExpressions = new Dictionary<Regex, Type>();

        static DirectiveNode() {
            directiveExpressions[new Regex(@"^#(?<name>def(ine)?)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(DefineNode);
            directiveExpressions[new Regex(@"^#(?<name>let)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(LetNode);
            directiveExpressions[new Regex(@"^#(?<name>each)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(EachNode);
            directiveExpressions[new Regex(@"^#(?<name>rep)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(RepNode);
            directiveExpressions[new Regex(@"^#(?<name>for)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(ForNode);
            directiveExpressions[new Regex(@"^#(?<name>inc(lude)?)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(IncludeNode);
            directiveExpressions[new Regex(@"^#(?<name>if)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(IfNode);
            directiveExpressions[new Regex(@"^#(?<name>else)\b(?<params>).*$", RegexOptions.ExplicitCapture)] = typeof(ElseNode);
            directiveExpressions[new Regex(@"^#(?<name>el(se)?if)\s+(?<params>.*)$", RegexOptions.ExplicitCapture)] = typeof(ElseIfNode);
            directiveExpressions[new Regex(@"^#(?<name>end\w+)\b(?<params>).*$", RegexOptions.ExplicitCapture)] = typeof(EndDirectiveNode);
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
            foreach(var pair in directiveExpressions) {
                Regex re = pair.Key;
                Type nodeType = pair.Value;

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
                    string curLine = input.ReadLine();

                    if(curLine == null) {
                        throw new ParserException("Unmatched #" + name + " directive", parser, location);
                    }

                    curNode = parser.ParseLine(curLine, input);

                    newNode.ChildrenNodes.Add(curNode);
                }

                return newNode;
            }

            throw new ParserException("Unknown directive: " + line, parser, location);
        }
    }
}
