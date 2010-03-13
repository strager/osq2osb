using System;
using System.Collections.Generic;
using System.Linq;
using osq.TreeNode;

namespace osq {
    public class ExecutionContext {
        private static readonly Random Rand = new Random(31337);

        private readonly IList<IDictionary<string, object>> variableStack = new List<IDictionary<string, object>>();
        private readonly IDictionary<string, object> builtinVariables = new Dictionary<string, object>();
        private readonly IDictionary<string, object> globalVariables = new Dictionary<string, object>();

        private void SetFunction(string name, Func<TokenNode, ExecutionContext, object> func) {
            SetVariable(name, func);
        }

        public ICollection<string> Dependencies {
            get;
            private set;
        }

        public ExecutionContext() {
            Dependencies = new List<string>();

            variableStack.Add(builtinVariables);

            Func<object, double> num = (o) => (Convert.ToDouble(o, Parser.DefaultCulture));

            SetFunction("int", (token, context) => (int)num(token.TokenChildren[0].Evaluate(context)));

            SetFunction("sqrt", (token, context) => Math.Sqrt(num(token.TokenChildren[0].Evaluate(context))));

            SetFunction("rand", (token, context) => Rand.NextDouble());

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

                throw new DataTypeException("Don't know how to handle equality of objects");
            };

            SetFunction("==", (token, context) => {
                var children = token.TokenChildren;

                return areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });

            SetFunction("!=", (token, context) => {
                var children = token.TokenChildren;

                return !areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });

            variableStack.Add(globalVariables);
        }

        public void PushScope() {
            variableStack.Add(new Dictionary<string, object>());
        }

        public void PopScope() {
            if(variableStack.Count <= 2) {
                throw new InvalidOperationException("Cannot pop global scope");
            }

            variableStack.RemoveAt(variableStack.Count - 1);
        }

        public void SetLocalVariable(string name, object value) {
            var localScope = variableStack.Reverse().First();

            localScope[name] = value;
        }

        private IDictionary<string, object> FindVariableScope(string name) {
            return variableStack.Reverse().FirstOrDefault((scope) => scope.ContainsKey(name));
        }

        public void SetVariable(string name, object value) {
            var scope = FindVariableScope(name) ?? this.globalVariables;

            scope[name] = value;
        }

        public object GetVariable(string name) {
            var scope = FindVariableScope(name);

            if(scope == null) {
                throw new UnknownVariableException(name);
            }

            return scope[name];
        }

        public bool VariableExists(string name) {
            return FindVariableScope(name) != null;
        }
    }
}