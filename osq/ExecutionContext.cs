using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osq.Parser;
using osq.TreeNode;

namespace osq {
    /// <summary>
    /// Represents a context of execution.
    /// Stores variables and handles scope during osq script execution.
    /// </summary>
    public class ExecutionContext {
        private readonly Random rand = new Random(31337);
        private readonly NumberFormatInfo numberFormat = new CultureInfo("en-US").NumberFormat;
        
        private readonly IList<IDictionary<string, object>> variableStack = new List<IDictionary<string, object>>();
        private readonly IDictionary<string, object> builtinVariables = new Dictionary<string, object>();
        private readonly IDictionary<string, object> globalVariables = new Dictionary<string, object>();

        /// <summary>
        /// Represents an osq function.  Takes the function call token and the context and returns the output of the function.
        /// </summary>
        public delegate object OsqFunction(TokenNode input, ExecutionContext context);

        /// <summary>
        /// Sets a built-in function.
        /// </summary>
        /// <param name="name">The name of the built-in function.</param>
        /// <param name="func">The function.</param>
        public void SetBuiltinVariable(string name, OsqFunction func) {
            builtinVariables[name] = func;
        }

        /// <summary>
        /// Sets a built-in variable.
        /// </summary>
        /// <param name="name">The name of the built-in variable.</param>
        /// <param name="obj">The value.</param>
        public void SetBuiltinVariable(string name, object obj) {
            builtinVariables[name] = obj;
        }

        /// <summary>
        /// Gets or sets the file dependencies.
        /// </summary>
        /// <value>The dependencies of the executing script.</value>
        public ICollection<string> Dependencies {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        public ExecutionContext() {
            Dependencies = new List<string>();

            variableStack.Add(builtinVariables);
            variableStack.Add(globalVariables);

            SetBuiltinVariable("int", (token, context) => (int)GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context)));

            SetBuiltinVariable("sqrt", (token, context) => Math.Sqrt(GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context))));

            SetBuiltinVariable("rand", (token, context) => this.rand.NextDouble());

            SetBuiltinVariable("pi", Math.PI);

            SetBuiltinVariable("sin", (token, context) => Math.Sin(GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context))));

            SetBuiltinVariable("cos", (token, context) => Math.Cos(GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context))));

            SetBuiltinVariable("tan", (token, context) => Math.Tan(GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context))));

            SetBuiltinVariable("atan", (token, context) => Math.Atan(GetDoubleFrom(token.ChildrenTokenNodes[0].Evaluate(context))));

            SetBuiltinVariable("atan2", (token, context) => {
                var args = token.ChildrenTokenNodes[0].Evaluate(context) as IEnumerable<object>;
                return Math.Atan2(context.GetDoubleFrom(args.ElementAt(0)), context.GetDoubleFrom(args.ElementAt(1)));
            });

            SetBuiltinVariable("concat", (token, context) => new Token(TokenType.String, string.Join("", token.ChildrenTokenNodes.Select((t) => (string)t.Evaluate(context)).ToArray())));

            SetBuiltinVariable("min", (token, context) => {
                var args = token.ChildrenTokenNodes[0].Evaluate(context) as IEnumerable<object>;
                return args.Min();
            });

            SetBuiltinVariable("max", (token, context) => {
                var args = token.ChildrenTokenNodes[0].Evaluate(context) as IEnumerable<object>;
                return args.Max();
            });

            SetBuiltinVariable("+", (token, context) => token.ChildrenTokenNodes.Aggregate((double)0, (r, t) => r + GetDoubleFrom(t.Evaluate(context))));

            SetBuiltinVariable("-", (token, context) => {
                var children = token.ChildrenTokenNodes;

                if(children.Count == 1) {
                    return -GetDoubleFrom(children[0].Evaluate(context));
                }

                return GetDoubleFrom(children[0].Evaluate(context)) - GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable("*", (token, context) => token.ChildrenTokenNodes.Aggregate((double)1, (r, t) => r * GetDoubleFrom(t.Evaluate(context))));

            SetBuiltinVariable("/", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) / GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable("%", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) % GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable("^", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return Math.Pow(GetDoubleFrom(children[0].Evaluate(context)), GetDoubleFrom(children[1].Evaluate(context)));
            });

            SetBuiltinVariable(",", (token, context) => token.ChildrenTokenNodes.Select(child => child.Evaluate(context)).ToArray());

            SetBuiltinVariable(":", (token, context) => {
                var children = token.ChildrenTokenNodes;

                double val = (GetDoubleFrom(children[0].Evaluate(context)) * 60 + GetDoubleFrom(children[1].Evaluate(context))) * 1000;

                if(children.Count == 3) {
                    val += GetDoubleFrom(children[2].Evaluate(context));
                }

                return val;
            });

            SetBuiltinVariable(">", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) > GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable("<", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) < GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable(">=", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) >= GetDoubleFrom(children[1].Evaluate(context));
            });

            SetBuiltinVariable("<=", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return GetDoubleFrom(children[0].Evaluate(context)) <= GetDoubleFrom(children[1].Evaluate(context));
            });

            Func<object, object, bool> areEqual = (a, b) => {
                if(a is string || b is string) {
                    return a.ToString() == b.ToString();
                } else if(a is double || b is double) {
                    return GetDoubleFrom(a) == GetDoubleFrom(b);
                }

                throw new DataTypeException("Don't know how to handle equality of objects");
            };

            SetBuiltinVariable("==", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });

            SetBuiltinVariable("!=", (token, context) => {
                var children = token.ChildrenTokenNodes;

                return !areEqual(children[0].Evaluate(context), children[1].Evaluate(context));
            });

            SetBuiltinVariable("format", (token, context) => {
                var children = token.ChildrenTokenNodes;

                {
                    var child = children.First();

                    if(child.Token.IsSymbol(",")) {
                        children = child.ChildrenTokenNodes;
                    }
                }

                var format = children.First().Evaluate(this).ToString();
                var args = children.Skip(1).Select((tokenNode) => tokenNode.Evaluate(this)).ToArray();

                return string.Format(numberFormat, format, args);
            });
        }

        public bool GetBoolFrom(object value) {
            if(value == null) {
                return false;
            }

            string asString = value as string;

            if(asString != null) {
                return asString != "";
            }

            if(value is int) {
                return (int)value != 0;
            }

            if(value is double) {
                double d = (double)value;   // Double D is the smart one.

                if(double.IsInfinity(d)) {
                    return true;
                }

                if(double.IsNaN(d)) {
                    return false;
                }

                return d != 0;
            }
            
            if(value is string) {
                return !string.IsNullOrEmpty((string)value);
            }
            
            if(value is bool) {
                return (bool)value;
            }

            IEnumerable asEnumerable = value as IEnumerable;

            if(asEnumerable != null) {
                return asEnumerable.Cast<object>().Any();
            }

            throw new DataTypeException("Condition returns unknown data type");
        }

        public bool GetBoolFrom(TokenNode tokenNode) {
            if(tokenNode == null) {
                throw new ArgumentNullException("tokenNode");
            }

            return GetBoolFrom(tokenNode.Evaluate(this));
        }

        public string GetStringOf(object value) {
            string asString = value as string;

            if(asString != null) {
                return asString;
            }

            if(value is int) {
                return ((int)value).ToString(numberFormat);
            }

            if(value is double) {
                return NumberHelpers.ToFloatingPointString((double)value, numberFormat);
            }

            if(value is bool) {
                return ((bool)value) ? "1" : "0";
            }

            IEnumerable asEnumerable = value as IEnumerable;

            if(asEnumerable != null) {
                return "(" + string.Join(",", asEnumerable.Cast<object>().Select(GetStringOf).ToArray()) + ")";
            }

            throw new DataTypeException("Don't know how to stringify value");
        }

        public double GetDoubleFrom(object obj) {
            return Convert.ToDouble(obj, numberFormat);
        }

        /// <summary>
        /// Creates a new scope and pushes it on the stack.
        /// </summary>
        public void PushScope() {
            variableStack.Add(new Dictionary<string, object>());
        }

        /// <summary>
        /// Pops the top scope of the stack and discards it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempted to pop the global scope.</exception>
        public void PopScope() {
            if(variableStack.Count <= 2) {
                throw new InvalidOperationException("Cannot pop global scope");
            }

            variableStack.RemoveAt(variableStack.Count - 1);
        }

        /// <summary>
        /// Sets a local variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value.</param>
        public void SetLocalVariable(string name, object value) {
            var localScope = variableStack.Reverse().First();

            localScope[name] = value;
        }

        /// <summary>
        /// Finds the highest variable scope of the given variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The scope of the given variable.</returns>
        private IDictionary<string, object> FindVariableScope(string name) {
            return variableStack.Reverse().FirstOrDefault((scope) => scope.ContainsKey(name));
        }

        /// <summary>
        /// Sets a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The new value.</param>
        public void SetVariable(string name, object value) {
            var scope = FindVariableScope(name) ?? this.globalVariables;

            scope[name] = value;
        }

        /// <summary>
        /// Gets the value of a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The value of the variable.</returns>
        /// <exception cref="UnknownVariableException">Variable not set.</exception>
        public object GetVariable(string name) {
            var scope = FindVariableScope(name);

            if(scope == null) {
                throw new UnknownVariableException(name);
            }

            return scope[name];
        }

        /// <summary>
        /// Determines whether a variable is set.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// 	<c>true</c> if the variable is set; otherwise, <c>false</c>.
        /// </returns>
        public bool IsVariableSet(string name) {
            return FindVariableScope(name) != null;
        }
    }
}