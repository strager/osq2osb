using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq2osb.Parser;
using osq2osb.Parser.TreeNode;

namespace osq2osb {
    public class ExecutionContext {
        private static Random rand = new Random(31337);

        private IDictionary<string, object> variables = new Dictionary<string, object>();

        public bool Debug {
            get {
                return debug;
            }

            set {
                debug = value;
            }
        }

        private bool debug = false;

        private void SetFunction(string name, Func<TokenNode, ExecutionContext, object> func) {
            SetVariable(name, func);
        }

        public ExecutionContext() {
            Func<object, double> num = (object o) => (System.Convert.ToDouble(o));

            SetFunction("int", (token, context) => {
                return (int)num(token.TokenChildren[0].Evaluate(context));
            });

            SetFunction("sqrt", (token, context) => {
                return Math.Sqrt(num(token.TokenChildren[0].Evaluate(context)));
            });

            SetFunction("rand", (token, context) => {
                return rand.NextDouble();
            });

            SetVariable("pi", Math.PI);

            SetFunction("sin", (token, context) => {
                return Math.Sin(num(token.TokenChildren[0].Evaluate(context)));
            });

            SetFunction("cos", (token, context) => {
                return Math.Cos(num(token.TokenChildren[0].Evaluate(context)));
            });

            SetFunction("tan", (token, context) => {
                return Math.Tan(num(token.TokenChildren[0].Evaluate(context)));
            });

            SetFunction("concat", (token, context) => {
                return new Tokenizer.Token(Tokenizer.TokenType.String, string.Join("", token.TokenChildren.Select((t) => (string)t.Evaluate(context)).ToArray()));
            });

            SetFunction("+", (token, context) => {
                return token.TokenChildren.Aggregate((double)0, (r, t) => r + num(t.Evaluate(context)));
            });

            SetFunction("-", (token, context) => {
                var children = token.TokenChildren;

                if(children.Count == 1) {
                    return -num(children[0].Evaluate(context));
                }

                return num(children[0].Evaluate(context)) - num(children[1].Evaluate(context));
            });

            SetFunction("*", (token, context) => {
                return token.TokenChildren.Aggregate((double)1, (r, t) => r * num(t.Evaluate(context)));
            });

            SetFunction("/", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) / num(children[1].Evaluate(context));
            });

            SetFunction("%", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) % num(children[1].Evaluate(context));
            });

            SetFunction("^", (token, context) => {
                var children = token.TokenChildren;

                return Math.Pow(num(children[0].Evaluate(context)), num(children[1].Evaluate(context)));
            });

            SetFunction(",", (token, context) => {
                return token.TokenChildren.Select(child => child.Evaluate(context)).ToArray();
            });
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
            } else if(variables.ContainsKey(name)) {
                value = variables[name];
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
