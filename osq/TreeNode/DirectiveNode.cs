using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace osq.TreeNode {
    public abstract class DirectiveNode : NodeBase {
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

            foreach(var type in GetDirectiveTypes()) {
                var attrs = type.GetCustomAttributes(typeof(DirectiveAttribute), true).OfType<DirectiveAttribute>();

                foreach(var attr in attrs) {
                    var info = ParseDirectiveLine(parser, attr.NameExpression, line, startLocation);

                    if(info == null) {
                        continue;
                    }

                    using(info.ParametersReader) {
                        var node = CreateInstance(type, info);

                        if(node == null) {
                            continue;
                        }

                        node.ReadSubNodes(parser);

                        return node;
                    }
                }
            }

            throw new BadDataException("Unknown directive " + line, startLocation);
        }

        private static DirectiveInfo ParseDirectiveLine(Parser parser, string nameExpression, string line, Location location) {
            Regex re = new Regex("^#(?<name>" + nameExpression + ")(\\s+(?<params>.*))?$", RegexOptions.ExplicitCapture);

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
            return Activator.CreateInstance(nodeType, info) as DirectiveNode;
        }

        private static IEnumerable<Type> GetDirectiveTypes() {
            return GetDirectiveTypes(Assembly.GetCallingAssembly());
        }

        private static IEnumerable<Type> GetDirectiveTypes(Assembly assembly) {
            return assembly.GetTypes().Where((type) => !type.IsAbstract && type.IsSubclassOf(typeof(DirectiveNode)));
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