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
        /// Gets or sets the name of the directive.
        /// </summary>
        /// <value>The name of the directive.</value>
        public string DirectiveName {
            get;
            private set;
        }

        protected DirectiveNode(string directiveName, Location location) :
            base(location) {
            DirectiveName = directiveName;
        }

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
                    string directiveName;
                    ITokenReader parameterReader;

                    var parseSuccesful = ParseDirectiveLine(parser, attr.NameExpression, line, startLocation, out parameterReader, out directiveName);

                    if(!parseSuccesful) {
                        continue;
                    }

                    var node = CreateInstance(type, parameterReader, parser, directiveName, startLocation);

                    if(node == null) {
                        continue;
                    }
                    
                    return node;
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
        /// <param name="parameterReader"></param>
        /// <param name="directiveName"></param>
        /// <returns>A <see cref="DirectiveInfo"/> instance containing information about the directive, or <c>null</c> if the directive does not match the expression.</returns>
        private static bool ParseDirectiveLine(Parser parser, string nameExpression, string line, Location location, out ITokenReader parameterReader, out string directiveName) {
            parameterReader = null;
            directiveName = null;

            Regex re = new Regex("^#(?<name>" + nameExpression + ")(\\s+(?<params>.*))?$", RegexOptions.ExplicitCapture);

            var match = re.Match(line);

            if(!match.Success) {
                return false;
            }

            directiveName = match.Groups["name"].Value;

            var parametersText = match.Groups["params"].Value;
            var parametersLocation = location.Clone();
            parametersLocation.AdvanceString(line.Substring(0, match.Groups["params"].Index));
            var parametersReader = new LocatedTextReaderWrapper(parametersText, parametersLocation);

            parameterReader = new TokenReader(parametersReader);

            return true;
        }

        /// <summary>
        /// Creates an instance of a directive.
        /// </summary>
        /// <param name="nodeType">Type of the directive node.</param>
        /// <param name="info">The new instance's directive information.</param>
        /// <returns>The new <see cref="DirectiveNode"/> instance.</returns>
        private static DirectiveNode CreateInstance(Type nodeType, ITokenReader tokenReader, INodeReader nodeReader, string directiveName, Location location) {
            return Activator.CreateInstance(nodeType, tokenReader, nodeReader, directiveName, location) as DirectiveNode;
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