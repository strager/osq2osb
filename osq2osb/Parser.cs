using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb {
    partial class Parser {
        private IDictionary<string, Variant> variables = new Dictionary<string, Variant>();

        private static IDictionary<string, Action<Stack<Variant>>> builtinFunctions;

        static Parser() {
            builtinFunctions = new Dictionary<string, Action<Stack<Variant>>>();

            builtinFunctions["int"] = (stack) => {
                stack.Push(new Variant((int)stack.Pop().AsNumber));
            };

            builtinFunctions["sqrt"] = (stack) => {
                stack.Push(new Variant(Math.Sqrt(stack.Pop().AsNumber)));
            };

            builtinFunctions["pi"] = (stack) => {
                stack.Push(new Variant(Math.PI));
            };

            builtinFunctions["sin"] = (stack) => {
                stack.Push(new Variant(Math.Sin(stack.Pop().AsNumber)));
            };

            builtinFunctions["cos"] = (stack) => {
                stack.Push(new Variant(Math.Cos(stack.Pop().AsNumber)));
            };

            builtinFunctions["tan"] = (stack) => {
                stack.Push(new Variant(Math.Tan(stack.Pop().AsNumber)));
            };

            builtinFunctions["+"] = (stack) => {
                stack.Push(new Variant(stack.Pop().AsNumber + stack.Pop().AsNumber));
            };

            builtinFunctions["-"] = (stack) => {
                var second = stack.Pop().AsNumber;
                var first = stack.Pop().AsNumber;
                stack.Push(new Variant(first - second));
            };

            builtinFunctions["*"] = (stack) => {
                stack.Push(new Variant(stack.Pop().AsNumber * stack.Pop().AsNumber));
            };

            builtinFunctions["/"] = (stack) => {
                var second = stack.Pop().AsNumber;
                var first = stack.Pop().AsNumber;
                stack.Push(new Variant(first / second));
            };

            builtinFunctions["%"] = (stack) => {
                stack.Push(new Variant((int)stack.Pop().AsNumber % (int)stack.Pop().AsNumber));
            };

            builtinFunctions["^"] = (stack) => {
                var second = stack.Pop().AsNumber;
                var first = stack.Pop().AsNumber;
                stack.Push(new Variant(Math.Pow(first, second)));
            };
        }

        public Parser() {
        }

        public Parser(Parser parent) {
            foreach(var pair in parent.variables) {
                variables.Add(pair);
            }
        }

        public void Parse(TextReader input, TextWriter output) {
            string line;

            while((line = input.ReadLine()) != null) {
                if(line.Length != 0 && line[0] == '#') {
                    DirectiveHandlers.Handle(line, input, output, this);
                } else {
                    line = ReplaceExpressions(line);
                    output.WriteLine(line);
                }
            }
        }

        protected string ReplaceExpressions(string input) {
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
                int expressionEnd = input.IndexOf("}", expressionStart);

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
            IEnumerable<string> tokens = ExpressionRewriter.InfixToPostfix(ExpressionRewriter.Tokenize(expression));
            Stack<Variant> stack = new Stack<Variant>();

            foreach(var token in tokens) {
                switch(ExpressionRewriter.GetTokenType(token)) {
                    case ExpressionRewriter.TokenType.Symbol:
                    case ExpressionRewriter.TokenType.Identifier:
                        if(builtinFunctions.ContainsKey(token)) {
                            builtinFunctions[token](stack);

                            break;
                        }

                        if(!variables.ContainsKey(token)) {
                            throw new InvalidDataException("Identifier " + token + " not defined.");
                        }

                        var item = variables[token];

                        switch(item.VariantType) {
                            case Variant.Type.Function:
                                var subParser = new Parser(this);

                                foreach(string parameter in item.ParameterList.Reverse()) {
                                    subParser.variables[parameter] = stack.Pop();
                                }

                                using(var reader = new StringReader(item.AsFunctionBody))
                                using(var writer = new StringWriter()) {
                                    subParser.Parse(reader, writer);

                                    stack.Push(new Variant(writer.GetStringBuilder().ToString().Trim(Environment.NewLine.ToCharArray())));
                                }

                                break;

                            default:
                                stack.Push(item);
                                break;
                        }

                        break;

                    case ExpressionRewriter.TokenType.Number:
                        stack.Push(new Variant(token, Variant.Type.Number));
                        break;
                }
            }

            return stack.Pop().AsString;
        }

        public void SetVariable(string name, Variant value) {
            if(!variables.ContainsKey(name)) {
                variables[name] = new Variant();
            }

            variables[name].CopyFrom(value);
        }

        public Variant GetVariable(string name) {
            var var = new Variant();

            var.CopyFrom(variables[name]);

            return var;
        }
    }
}
