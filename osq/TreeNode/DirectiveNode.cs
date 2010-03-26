using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace osq.TreeNode {
    /// <summary>
    /// Represents an osq directive.
    /// </summary>
    public abstract class DirectiveNode : NodeBase {
        /// <summary>
        /// Contains information about a directive.
        /// </summary>
        public class DirectiveInfo {
            /// <summary>
            /// Gets or sets the parser the directive is from.
            /// </summary>
            /// <value>The parser the directive is from.</value>
            public Parser Parser {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the name of the directive.
            /// </summary>
            /// <value>The name of the directive.</value>
            public string DirectiveName {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the location of the directive.
            /// </summary>
            /// <value>The location of the directive.</value>
            public Location Location {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a reader for the directive's parameters.
            /// </summary>
            /// <value>A reader for the directive's parameters.</value>
            public LocatedTextReaderWrapper ParametersReader {
                get;
                set;
            }
        }

        /// <summary>
        /// Gets or sets the name of the directive.
        /// </summary>
        /// <value>The name of the directive.</value>
        public string DirectiveName {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveNode"/> class.
        /// </summary>
        /// <param name="info">The directive's information.</param>
        protected DirectiveNode(DirectiveInfo info) :
            base(/*info.Location*/) {
            //DirectiveName = info.DirectiveName;
        }

        protected abstract bool EndsWith(NodeBase node);

        /// <summary>
        /// Reads a directive node.
        /// </summary>
        /// <param name="parser">The parser to read the directive node from.</param>
        /// <returns>The directive node.</returns>
        /// <exception cref="BadDataException">Read an invalid directive.</exception>
        /// <exception cref="MissingDataException">Closing directive node was missing.</exception>
        public static DirectiveNode Create(Parser parser) {
            var startLocation = parser.InputReader.Location.Clone();
            string line = parser.InputReader.ReadLine();

            foreach(var type in GetDirectiveTypes()) {
                foreach(var attr in type.GetCustomAttributes(typeof(DirectiveAttribute), true).OfType<DirectiveAttribute>()) {
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

        /// <summary>
        /// Parses a line containing a directive reference.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="nameExpression">The regular expression to match the name of the directive.</param>
        /// <param name="line">The line containing the directive.</param>
        /// <param name="location">The location of the directive.</param>
        /// <returns>A <see cref="DirectiveInfo"/> instance containing information about the directive, or <c>null</c> if the directive does not match the expression.</returns>
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

            return new DirectiveInfo {
                Parser = parser,
                Location = location,
                DirectiveName = name,
                ParametersReader = parametersReader
            };
        }

        /// <summary>
        /// Creates an instance of a directive.
        /// </summary>
        /// <param name="nodeType">Type of the directive node.</param>
        /// <param name="info">The new instance's directive information.</param>
        /// <returns>The new <see cref="DirectiveNode"/> instance.</returns>
        private static DirectiveNode CreateInstance(Type nodeType, DirectiveInfo info) {
            return Activator.CreateInstance(nodeType, info) as DirectiveNode;
        }

        /// <summary>
        /// Gets all the directive types available.
        /// </summary>
        /// <returns>Directive types.</returns>
        private static IEnumerable<Type> GetDirectiveTypes() {
            return GetDirectiveTypes(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Gets the directive types of a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly in which to search for directive types.</param>
        /// <returns>Directive types in <paramref name="assembly"/>.</returns>
        private static IEnumerable<Type> GetDirectiveTypes(Assembly assembly) {
            return assembly.GetTypes().Where((type) => !type.IsAbstract && type.IsSubclassOf(typeof(DirectiveNode)));
        }

        /// <summary>
        /// Reads the sub-nodes required by this node.
        /// </summary>
        /// <param name="parser">The parser to read sub-nodes from.</param>
        /// <exception cref="MissingDataException">Closing directive node was not available.</exception>
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() {
            return "#" + DirectiveName;
        }
    }
}