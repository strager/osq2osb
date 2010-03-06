using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;
using osq.Parser.TreeNode;

namespace osq {
    public class ExecutionContext {
        private static readonly Random rand = new Random(31337);

        private readonly IDictionary<string, object> variables = new Dictionary<string, object>();

        private void SetFunction(string name, Func<TokenNode, ExecutionContext, object> func) {
            SetVariable(name, func);
        }

        public ICollection<string> Dependencies {
            get;
            private set;
        }

        public ExecutionContext() {
            Dependencies = new List<string>();

            Func<object, double> num = (object o) => (System.Convert.ToDouble(o));

            SetFunction("int", (token, context) => (int)num(token.TokenChildren[0].Evaluate(context)));

            SetFunction("sqrt", (token, context) => Math.Sqrt(num(token.TokenChildren[0].Evaluate(context))));

            SetFunction("rand", (token, context) => rand.NextDouble());

            SetVariable("pi", Math.PI);

            SetFunction("sin", (token, context) => Math.Sin(num(token.TokenChildren[0].Evaluate(context))));

            SetFunction("cos", (token, context) => Math.Cos(num(token.TokenChildren[0].Evaluate(context))));

            SetFunction("tan", (token, context) => Math.Tan(num(token.TokenChildren[0].Evaluate(context))));

            SetFunction("concat", (token, context) => new Token(TokenType.String, string.Join("", token.TokenChildren.Select((t) => (string)t.Evaluate(context)).ToArray())));

            SetFunction("+", (token, context) => token.TokenChildren.Aggregate((double)0, (r, t) => r + num(t.Evaluate(context))));

            SetFunction("-", (token, context) => {
                var children = token.TokenChildren;

                if(children.Count == 1) {
                    return -num(children[0].Evaluate(context));
                }

                return num(children[0].Evaluate(context)) - num(children[1].Evaluate(context));
            });

            SetFunction("*", (token, context) => token.TokenChildren.Aggregate((double)1, (r, t) => r * num(t.Evaluate(context))));

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

            SetFunction(",", (token, context) => token.TokenChildren.Select(child => child.Evaluate(context)).ToArray());

            SetFunction(":", (token, context) => {
                var children = token.TokenChildren;

                double val = (num(children[0].Evaluate(context)) * 60 + num(children[1].Evaluate(context))) * 1000;

                if(children.Count == 3) {
                    val += num(children[2].Evaluate(context));
                }

                return val;
            });

            SetFunction(">", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) > num(children[1].Evaluate(context));
            });

            SetFunction("<", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) < num(children[1].Evaluate(context));
            });

            SetFunction(">=", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) >= num(children[1].Evaluate(context));
            });

            SetFunction("<=", (token, context) => {
                var children = token.TokenChildren;

                return num(children[0].Evaluate(context)) <= num(children[1].Evaluate(context));
            });

            Func<object, object, bool> areEqual = (a, b) => {
                if(a is string || b is string) {
                    return a.ToString() == b.ToString();
                } else if(a is double || b is double) {
                    return num(a) == num(b);
                }

                throw new InvalidOperationException("Don't know how to handle equality of objects");
            };

            SetFunction("==", (token, context) => {
                var children = token.TokenChildren;

                return areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });

            SetFunction("!=", (token, context) => {
                var children = token.TokenChildren;

                return !areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });
        }

        public void SetVariable(string name, object value) {
            variables[name] = value;
        }

        public object GetVariable(string name) {
            object value = null;

            if(variables.ContainsKey(name)) {
                value = variables[name];
            } else if(variables.ContainsKey(name)) {
                value = variables[name];
            } else {
                throw new InvalidOperationException("Unknown variable: " + name);
            }

            return value;
        }
    }
}
