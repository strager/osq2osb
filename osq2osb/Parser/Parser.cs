using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace osq2osb.Parser {
    using TreeNode;

    partial class Parser {
        private IDictionary<string, object> variables = new Dictionary<string, object>();

        private static IDictionary<string, Action<Stack<object>>> builtinFunctions;

        static Parser() {
            builtinFunctions = new Dictionary<string, Action<Stack<object>>>();

            builtinFunctions["int"] = (stack) => {
                stack.Push((int)System.Convert.ToDouble(stack.Pop()));
            };

            builtinFunctions["sqrt"] = (stack) => {
                stack.Push(Math.Sqrt(System.Convert.ToDouble(stack.Pop())));
            };

            builtinFunctions["pi"] = (stack) => {
                stack.Push(Math.PI);
            };

            builtinFunctions["sin"] = (stack) => {
                stack.Push(Math.Sin(System.Convert.ToDouble(stack.Pop())));
            };

            builtinFunctions["cos"] = (stack) => {
                stack.Push(Math.Cos(System.Convert.ToDouble(stack.Pop())));
            };

            builtinFunctions["tan"] = (stack) => {
                stack.Push(Math.Tan(System.Convert.ToDouble(stack.Pop())));
            };

            builtinFunctions["concat"] = (stack) => {
                string second = stack.Pop().ToString();
                string first = stack.Pop().ToString();
                stack.Push(string.Concat(first, second));
            };

            builtinFunctions["+"] = (stack) => {
                stack.Push(System.Convert.ToDouble(stack.Pop()) + System.Convert.ToDouble(stack.Pop()));
            };

            builtinFunctions["-"] = (stack) => {
                double second = System.Convert.ToDouble(stack.Pop());
                double first = System.Convert.ToDouble(stack.Pop());
                stack.Push(first - second);
            };

            builtinFunctions["*"] = (stack) => {
                stack.Push(System.Convert.ToDouble(stack.Pop()) * System.Convert.ToDouble(stack.Pop()));
            };

            builtinFunctions["/"] = (stack) => {
                double second = System.Convert.ToDouble(stack.Pop());
                double first = System.Convert.ToDouble(stack.Pop());
                stack.Push(first / second);
            };

            builtinFunctions["%"] = (stack) => {
                stack.Push(System.Convert.ToDouble(stack.Pop()) % System.Convert.ToDouble(stack.Pop()));
            };

            builtinFunctions["^"] = (stack) => {
                double second = System.Convert.ToDouble(stack.Pop());
                double first = System.Convert.ToDouble(stack.Pop());
                stack.Push(Math.Pow(first, second));
            };
        }

        public int LineNumber {
            get {
                return lineNumber;
            }
        }

        private int lineNumber;

        public Parser() {
        }

        public void ParseAndExecute(TextReader input, TextWriter output) {
            lineNumber = 0;

            string line;

            while((line = input.ReadLine()) != null) {
                var node = ParseLine(line, input);
                node.Execute(output);
                ++lineNumber;
            }
        }

        public NodeBase ParseLine(string line, TextReader input) {
            if(line == null) {
                throw new ArgumentNullException("line");
            }

            if(line.Length != 0 && line[0] == '#') {
                return DirectiveNode.Create(line, input, this);
            } else {
                return new RawTextNode(line + Environment.NewLine, this);
            }
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
            IEnumerable<Tokenizer.Token> tokens = ExpressionRewriter.InfixToPostfix(Tokenizer.Tokenize(expression));
            Stack<object> stack = new Stack<object>();

            foreach(var token in tokens) {
                switch(token.Type) {
                    case Tokenizer.TokenType.Symbol:
                    case Tokenizer.TokenType.Identifier:
                        if(builtinFunctions.ContainsKey(token.Value)) {
                            builtinFunctions[token.Value](stack);

                            break;
                        }

                        if(!variables.ContainsKey(token.Value)) {
                            throw new InvalidDataException("Identifier " + token + " not defined.");
                        }

                        var item = variables[token.Value];

                        var func = item as Action<Stack<object>>;

                        if(func != null) {
                            func(stack);
                        } else {
                            stack.Push(item);
                        }

                        break;

                    case Tokenizer.TokenType.Number:
                        stack.Push(double.Parse(token.Value));
                        break;

                    case Tokenizer.TokenType.String:
                        stack.Push(token.Value);
                        break;
                }
            }

            return stack.Pop().ToString();
        }

        public void SetVariable(string name, object value) {
            variables[name] = value;
        }

        public object GetVariable(string name) {
            return variables[name];
        }
    }
}
