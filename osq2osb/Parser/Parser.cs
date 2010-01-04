using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser {
    using TreeNode;

    public class Parser {
        private static Random rand = new Random(31337);

        private IDictionary<string, object> variables = new Dictionary<string, object>();

        private static IDictionary<string, Func<TokenNode, object>> builtinFunctions;

        public bool Debug {
            get {
                return debug;
            }

            set {
                debug = value;
            }
        }

        private bool debug = false;

        static Parser() {
            Func<object, double> num = (object o) => (System.Convert.ToDouble(o));

            builtinFunctions = new Dictionary<string, Func<TokenNode, object>>();

            builtinFunctions["int"] = (token) => {
                return (int)num(token.TokenChildren[0].Value);
            };

            builtinFunctions["sqrt"] = (token) => {
                return Math.Sqrt(num(token.TokenChildren[0].Value));
            };
            builtinFunctions["rand"] = (token) => {
                return rand.NextDouble();
            };

            builtinFunctions["pi"] = (token) => {
                return Math.PI;
            };

            builtinFunctions["sin"] = (token) => {
                return Math.Sin(num(token.TokenChildren[0].Value));
            };

            builtinFunctions["cos"] = (token) => {
                return Math.Cos(num(token.TokenChildren[0].Value));
            };

            builtinFunctions["tan"] = (token) => {
                return Math.Tan(num(token.TokenChildren[0].Value));
            };

            builtinFunctions["concat"] = (token) => {
                return new Tokenizer.Token(Tokenizer.TokenType.String, string.Join("", token.TokenChildren.Select((t) => (string)t.Value).ToArray()));
            };

            builtinFunctions["+"] = (token) => {
                return token.TokenChildren.Aggregate((double)0, (r, t) => r + num(t.Value));
            };

            builtinFunctions["-"] = (token) => {
                var children = token.TokenChildren;

                if(children.Count == 1) {
                    return -num(children[0].Value);
                }

                return num(children[0].Value) - num(children[1].Value);
            };

            builtinFunctions["*"] = (token) => {
                return token.TokenChildren.Aggregate((double)1, (r, t) => r * num(t.Value));
            };

            builtinFunctions["/"] = (token) => {
                var children = token.TokenChildren;

                return num(children[0].Value) / num(children[1].Value);
            };

            builtinFunctions["%"] = (token) => {
                var children = token.TokenChildren;

                return num(children[0].Value) % num(children[1].Value);
            };

            builtinFunctions["^"] = (token) => {
                var children = token.TokenChildren;

                return Math.Pow(num(children[0].Value), num(children[1].Value));
            };
        }

        private Location currentLocation;

        public Parser() {
        }

        public void ParseAndExecute(TextReader input, TextWriter output) {
            Location oldLocation = currentLocation;

            currentLocation = new Location();

            while(true) {
                var node = ReadNode(input);

                if(node == null) {
                    break;
                }

                node.Execute(output);
            }

            currentLocation = oldLocation;
        }

        internal NodeBase ReadNode(TextReader input) {
            if(input == null) {
                throw new ArgumentNullException("input");
            }

            int c = input.Peek();

            if(c < 0) {
                return null;
            }

            if(c == '$') {
                return ReadExpressionNode(input);
            } else if(c == '#' && currentLocation.Column == 1) {
                return ReadDirectiveNode(input);
            } else {
                return ReadTextNode(input);
            }
        }

        public TokenNode ExpressionToTokenNode(string expression) {
            return ExpressionRewriter.Rewrite(Tokenizer.Tokenize(expression), this);
        }

        TokenNode ReadExpressionNode(TextReader input) {
            var location = currentLocation.Clone();

            return ExpressionToTokenNode(ReadExpressionString(input));
        }

        string ReadExpressionString(TextReader input) {
            if(input.Read() != '$') {
                throw new InvalidDataException("Expressions must begin with $");
            }

            if(input.Read() != '{') {
                throw new InvalidDataException("Expressions must begin with ${");
            }

            StringBuilder expression = new StringBuilder();

            int c = input.Read();

            while(c >= 0 && c != '}') {
                expression.Append((char)c);

                currentLocation.AdvanceCharacter((char)c);

                c = input.Read();
            }

            // Ending } already read.

            return expression.ToString();
        }

        DirectiveNode ReadDirectiveNode(TextReader input) {
            var location = currentLocation.Clone();

            string line = input.ReadLine();
            currentLocation.AdvanceLine();

            return DirectiveNode.Create(line, input, this, location);
        }

        RawTextNode ReadTextNode(TextReader input) {
            var location = currentLocation.Clone();

            StringBuilder text = new StringBuilder();

            int c = input.Peek();

            while(c >= 0 && !(c == '#' && currentLocation.Column == 1) && c != '$') {
                text.Append((char)c);

                currentLocation.AdvanceCharacter((char)c);

                input.Read();   // Discard; already peeked.
                c = input.Peek();
            }

            return new RawTextNode(text.ToString(), this, location); ;
        }

        public void SetVariable(string name, object value) {
            variables[name] = value;

            if(Debug) {
                Console.WriteLine("Writing " + name + " = " + value.ToString());
            }
        }

        public object GetVariable(string name) {
            object value = null;

            if(variables.ContainsKey(name)) {
                value = variables[name];
            } else if(builtinFunctions.ContainsKey(name)) {
                value = builtinFunctions[name];
            } else {
                throw new IndexOutOfRangeException("Unknown variable: " + name);
            }

            if(Debug) {
                Console.WriteLine("Reading " + name + " (= " + value.ToString() + ")");
            }

            return value;
        }
    }
}
