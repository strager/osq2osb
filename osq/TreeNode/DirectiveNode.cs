using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace osq.TreeNode {
    public abstract class DirectiveNode : NodeBase {
        private static readonly IDictionary<string, Type> DirectiveTypes = new Dictionary<string, Type> {
            { "def(ine)?", typeof(DefineNode) },
            { "let", typeof(LetNode) },
            { "each", typeof(EachNode) },
            { "rep", typeof(RepNode) },
            { "for", typeof(ForNode) },
            { "inc(lude)?", typeof(IncludeNode) },
            { "if", typeof(IfNode) },
            { "else", typeof(ElseNode) },
            { "el(se)?if", typeof(ElseIfNode) },
            { "local", typeof(LocalNode) },
            { "end([^\\s]+)", typeof(EndDirectiveNode) },
        };

        public class DirectiveInfo {
            public Parser Parser {
                get;
                set;
            }

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

            public DirectiveInfo(Parser parser, Location location, string directiveName, LocatedTextReaderWrapper parametersReader) {
                Parser = parser;
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
            DirectiveName = info.DirectiveName;
        }

        protected abstract bool EndsWith(NodeBase node);

        public static DirectiveNode Create(Parser parser) {
            var startLocation = parser.InputReader.Location.Clone();
            string line = parser.InputReader.ReadLine();

            foreach(var pair in DirectiveTypes) {
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
                    DirectiveInfo info = new DirectiveInfo(parser, startLocation, name, parametersReader);

                    var ctor = nodeType.GetConstructor(new[] { typeof(DirectiveInfo) });
                    Debug.Assert(ctor != null, nodeType.Name + " doesn't have DirectiveInfo ctor");

                    newNode = ctor.Invoke(new object[] { info }) as DirectiveNode;
                    Debug.Assert(newNode != null, "Problem making new " + nodeType.Name);
                }

                NodeBase curNode = newNode;

                while(!newNode.EndsWith(curNode)) {
                    curNode = parser.ReadNode();

                    if(curNode == null) {
                        throw new MissingDataException("Closing #" + name + " directive", startLocation);
                    }

                    newNode.ChildrenNodes.Add(curNode);
                }

                return newNode;
            }

            throw new BadDataException("Unknown directive " + line, startLocation);
        }

        public override string ToString() {
            return "#" + DirectiveName;
        }
    }
}