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

        protected virtual bool IsMultiline {
            get {
                return false;
            }
        }

        protected DirectiveNode(Parser parser) :
            base(parser) {
        }

        public static DirectiveNode Create(string line, TextReader input, Parser parser) {
            foreach(var pair in directiveExpressions) {
                Regex re = pair.Key;
                Type nodeType = pair.Value;

                var match = re.Match(line);

                if(!match.Success) {
                    continue;
                }

                string name = match.Groups["name"].Value;
                string parameters = match.Groups["params"].Value;

                var ctor = nodeType.GetConstructor(new Type[] { typeof(Parser)  });
                var newNode = ctor.Invoke(new object[] { parser }) as DirectiveNode;
                newNode.Parameters = parameters;
                newNode.DirectiveName = name;

                if(newNode.IsMultiline) {
                    string endName = "end" + name;
                    IList<NodeBase> children = new List<NodeBase>();

                    while(true) {
                        string curLine = input.ReadLine();

                        if(curLine == null) {
                            throw new InvalidDataException("Unmatched #" + name + " directive");
                        }

                        NodeBase node = parser.ParseLine(curLine, input);

                        var endNode = node as EndDirectiveNode; 
                        
                        if(endNode != null) {
                            if(endNode.DirectiveName != endName) {
                                throw new InvalidDataException("Poorly balanced directives: got #" + endNode.DirectiveName + ", expected #" + endName);
                            }

                            break;
                        }

                        children.Add(node);
                    }

                    newNode.ChildrenNodes = children;
                }

                return newNode;
            }

            throw new InvalidDataException("Unknown directive: " + line);
        }
    }
}
