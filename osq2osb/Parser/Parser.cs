using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser {
    using TreeNode;

    public class Parser {
        private IDictionary<string, object> variables = new Dictionary<string, object>();

        private static IDictionary<string, Func<TokenNode, object>> builtinFunctions;

        static Parser() {
            builtinFunctions = new Dictionary<string, Func<TokenNode, object>>();

            builtinFunctions["int"] = (token) => {
                return (int)(double)token.TokenChildren[0].Value;
            };

            builtinFunctions["sqrt"] = (token) => {
                return Math.Sqrt((double)token.TokenChildren[0].Value);
            };

            builtinFunctions["pi"] = (token) => {
                return Math.PI;
            };

            builtinFunctions["sin"] = (token) => {
                return Math.Sin((double)token.TokenChildren[0].Value);
            };

            builtinFunctions["cos"] = (token) => {
                return Math.Cos((double)token.TokenChildren[0].Value);
            };

            builtinFunctions["tan"] = (token) => {
                return Math.Tan((double)token.TokenChildren[0].Value);
            };

            builtinFunctions["concat"] = (token) => {
                return new Tokenizer.Token(Tokenizer.TokenType.String, string.Join("", token.TokenChildren.Select((t) => (string)t.Value).ToArray()));
            };

            builtinFunctions["+"] = (token) => {
                return token.TokenChildren.Aggregate((double)0, (r, t) => r + (double)t.Value);
            };

            builtinFunctions["-"] = (token) => {
                var children = token.TokenChildren;

                return (double)children[0].Value - (double)children[1].Value;
            };

            builtinFunctions["*"] = (token) => {
                return token.TokenChildren.Aggregate((double)1, (r, t) => r * (double)t.Value);
            };

            builtinFunctions["/"] = (token) => {
                var children = token.TokenChildren;

                return (double)children[0].Value / (double)children[1].Value;
            };

            builtinFunctions["%"] = (token) => {
                var children = token.TokenChildren;

                return (double)children[0].Value % (double)children[1].Value;
            };

            builtinFunctions["^"] = (token) => {
                var children = token.TokenChildren;

                return Math.Pow((double)children[0].Value, (double)children[1].Value);
            };
        }

        private Location currentLocation;

        public Parser() {
        }

        public void ParseAndExecute(TextReader input, TextWriter output) {
            Location oldLocation = currentLocation;

            currentLocation = new Location();
            currentLocation.LineNumber = 0;

            string line;

            while((line = input.ReadLine()) != null) {
                var node = ParseLine(line, input);
                node.Execute(output);
            }

            currentLocation = oldLocation;
        }

        public NodeBase ParseLine(string line, TextReader input) {
            if(line == null) {
                throw new ArgumentNullException("line");
            }

            ++currentLocation.LineNumber;

            NodeBase node;

            if(line.Length != 0 && line[0] == '#') {
                node = DirectiveNode.Create(line, input, this, currentLocation.Clone());
            } else {
                node = new RawTextNode(line + Environment.NewLine, this, currentLocation.Clone());
            }

            return node;
        }

        public string ReplaceExpressions(string input) {
            StringBuilder output = new StringBuilder();

            int i = 0;

            while(i < input.Length) {
                int expressionStart = input.IndexOf("${", i);

                if(expressionStart < 0) {
                    output.Append(input.Substring(i));
                    break;
                }

                /* Output stuff before expression. */
                output.Append(input.Substring(i, expressionStart - i));

                /* Output evaluated expression. */
                int expressionEnd = input.IndexOf("}", expressionStart, StringComparison.Ordinal);

                if(expressionEnd < 0) {
                    throw new InvalidDataException("${ has no matching }");
                }

                string expression = input.Substring(expressionStart + "${".Length, expressionEnd - expressionStart - "${".Length);

                output.Append(EvaluateExpression(expression));

                i = expressionEnd + "}".Length;
            }

            return output.ToString();
        }

        public string EvaluateExpression(string expression) {
            TokenNode token = ExpressionRewriter.Rewrite(Tokenizer.Tokenize(expression), this);
            token.Print();
            return token.Value.ToString();
        }

        public void SetVariable(string name, object value) {
            variables[name] = value;
        }

        public object GetVariable(string name) {
            if(variables.ContainsKey(name)) {
                return variables[name];
            }

            if(builtinFunctions.ContainsKey(name)) {
                return builtinFunctions[name];
            }

            throw new IndexOutOfRangeException("Unknown variable: " + name);
        }
    }
}
