using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq;
using osq.Parser;

using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    class ParserTests {
        private static void CheckParserOutput(string expected, string input) {
            var context = new ExecutionContext();

            var output = new StringBuilder();

            using(var reader = new LocatedTextReaderWrapper(input)) {
                foreach(var node in Parser.Parser.ReadNodes(reader)) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }

        private static Parser.TreeNode.TokenNode ExpressionToTokenNode(string expression) {
            using(var reader = new LocatedTextReaderWrapper(expression, new Parser.Location())) {
                return ExpressionRewriter.Rewrite(Token.ReadTokens(reader));
            }
        }

        [Test]
        public void If() {
            CheckParserOutput("good\n", "#if 2 > 1\ngood\n#else\nbad\n#endif");
            CheckParserOutput("good\n", "#if 2 < 1\nbad\n#else\ngood\n#endif");
            CheckParserOutput("good\n", "#if 0\nbad\n#elseif 1 == 0\nbad\n#else\ngood\n#endif");
            CheckParserOutput("good\n", "#if \"a\" != \"b\"\ngood\n#else\nbad\n#endif");
            CheckParserOutput("good\n", "#if \"a\" == \"b\"\nbad\n#else\ngood\n#endif");
        }

        [Test]
        public void MathEvaluation() {
            var context = new ExecutionContext();
            var tree = ExpressionToTokenNode("(2) + (17*2-30) * (5)+2 - -(8/2)*4");

            Assert.AreEqual(40, tree.Evaluate(context));

            tree = ExpressionToTokenNode("01:23:456 + 01:23");

            Assert.AreEqual((((1 * 60) + 23) * 1000 + 456) + ((1 * 60) + 23) * 1000, tree.Evaluate(context));
        }

        [Test]
        public void Evaluation() {
            string input = "x${test - 42}.2";
            string expected = "x42.2";

            var context = new ExecutionContext();
            context.SetVariable("test", 84);

            var output = new StringBuilder();

            using(var reader = new LocatedTextReaderWrapper(input)) {
                foreach(var node in Parser.Parser.ReadNodes(reader)) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }

        [Test]
        public void VariableShorthand() {
            string input = "x$test.2";
            string expected = "x42.2";

            var context = new ExecutionContext();
            context.SetVariable("test", 42);

            var output = new StringBuilder();

            using(var reader = new LocatedTextReaderWrapper(input)) {
                foreach(var node in Parser.Parser.ReadNodes(reader)) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }
    }
}