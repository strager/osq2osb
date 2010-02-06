using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser.TreeNode {
    abstract public class DirectiveNode : NodeBase {
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

        public class DirectiveInfo {
            public string DirectiveName {
                get;
                set;
            }

            public Location Location {
                get;
                set;
            }

            public LocatedTextReaderWrapper ParametersReader {
                get;
                set;
            }

            public DirectiveInfo(Location location, string directiveName, LocatedTextReaderWrapper parametersReader) {
                Location = location;
                DirectiveName = directiveName;
                ParametersReader = parametersReader;
            }
        }

        public string DirectiveName {
            get;
            private set;
        }

        protected DirectiveNode(DirectiveInfo info) :
            base(info.Location) {
            this.DirectiveName = info.DirectiveName;
        }

        protected abstract bool EndsWith(NodeBase node);

        public static DirectiveNode Create(LocatedTextReaderWrapper input) {
            var startLocation = input.Location.Clone();
            string line = input.ReadLine();

            foreach(var pair in directiveTypes) {
                string type = pair.Key;
                Type nodeType = pair.Value;

                Regex re = new Regex("^#(?<name>" + type + ")(\\s+(?<params>.*))?$", RegexOptions.ExplicitCapture);

                var match = re.Match(line);

                if(!match.Success) {
                    continue;
                }

                string name = match.Groups["name"].Value;

                DirectiveNode newNode;

                string parametersText = match.Groups["params"].Value;
                var parametersLocation = startLocation.Clone();
                parametersLocation.AdvanceString(line.Substring(0, match.Groups["params"].Index));

                using(var parametersReader = new LocatedTextReaderWrapper(parametersText, parametersLocation)) {
                    DirectiveInfo info = new DirectiveInfo(startLocation, name, parametersReader);

                    var ctor = nodeType.GetConstructor(new Type[] { typeof(DirectiveInfo) });
                    System.Diagnostics.Debug.Assert(ctor != null, nodeType.Name + " doesn't have DirectiveInfo ctor");

                    newNode = ctor.Invoke(new object[] { info }) as DirectiveNode;
                    System.Diagnostics.Debug.Assert(newNode != null, "Problem making new " + nodeType.Name);
                }

                NodeBase curNode = newNode;

                while(!newNode.EndsWith(curNode)) {
                    curNode = Parser.ReadNode(input);
                    
                    if(curNode == null) {
                        throw new ParserException("Unmatched #" + name + " directive", startLocation);
                    }

                    newNode.ChildrenNodes.Add(curNode);
                }

                return newNode;
            }

            throw new ParserException("Unknown directive: " + line, startLocation);
        }

        public override string ToString() {
            return "#" + DirectiveName;
        }
    }
}
