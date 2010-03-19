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

                var info = ParseDirectiveLine(parser, type, line, startLocation);

                if(info == null) {
                    continue;
                }

                using(info.ParametersReader) {
                    var node = CreateInstance(nodeType, info);

                    node.ReadSubNodes(parser);

                    return node;
                }
            }

            throw new BadDataException("Unknown directive " + line, startLocation);
        }

        private static DirectiveInfo ParseDirectiveLine(Parser parser, string type, string line, Location location) {
            Regex re = new Regex("^#(?<name>" + type + ")(\\s+(?<params>.*))?$", RegexOptions.ExplicitCapture);

            var match = re.Match(line);

            if(!match.Success) {
                return null;
            }

            var name = match.Groups["name"].Value;
            var parametersText = match.Groups["params"].Value;

            var parametersLocation = location.Clone();
            parametersLocation.AdvanceString(line.Substring(0, match.Groups["params"].Index));

            var parametersReader = new LocatedTextReaderWrapper(parametersText, parametersLocation);

            return new DirectiveInfo(parser, location, name, parametersReader);
        }

        private static DirectiveNode CreateInstance(Type nodeType, DirectiveInfo info) {
            var ctor = nodeType.GetConstructor(new[] { typeof(DirectiveInfo) });
            Debug.Assert(ctor != null, nodeType.Name + " doesn't have DirectiveInfo ctor");

            var node = ctor.Invoke(new[] { info }) as DirectiveNode;
            Debug.Assert(node != null, "Problem making new " + nodeType.Name);

            return node;
        }

        private void ReadSubNodes(Parser parser) {
            NodeBase curNode = this;

            while(!this.EndsWith(curNode)) {
                curNode = parser.ReadNode();

                if(curNode == null) {
                    throw new MissingDataException("Closing #" + this.DirectiveName + " directive", this.Location);
                }

                this.ChildrenNodes.Add(curNode);
            }
        }

        public override string ToString() {
            return "#" + DirectiveName;
        }
    }
}