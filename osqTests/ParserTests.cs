using System.Text;
using NUnit.Framework;
using osq.Parser;
using osq.TreeNode;

namespace osq.Tests {
    [TestFixture]
    class ParserTests {
        private static void CheckParserOutput(string expected, string input) {
            var context = new ExecutionContext();

            var output = new StringBuilder();

            using(var reader = new LocatedTextReaderWrapper(input)) {
                var parser = new Parser.Parser(reader);

                foreach(var node in parser.ReadNodes()) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }

        private static TokenNode ExpressionToTokenNode(string expression) {
            using(var reader = new LocatedTextReaderWrapper(expression, new Location())) {
                var tokenReader = new TokenReader(reader);

                return ExpressionRewriter.Rewrite(tokenReader);
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
                var parser = new Parser.Parser(reader);

                foreach(var node in parser.ReadNodes()) {
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
                var parser = new Parser.Parser(reader);

                foreach(var node in parser.ReadNodes()) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }

        [Test]
        public void NotVariableShorthand() {
            string input = "x$test.2";
            string expected = "x$test.2";

            var context = new ExecutionContext();

            var output = new StringBuilder();

            using(var reader = new LocatedTextReaderWrapper(input)) {
                var parser = new Parser.Parser(reader);

                parser.Options.AllowVariableShorthand = false;

                foreach(var node in parser.ReadNodes()) {
                    output.Append(node.Execute(context));
                }

                Assert.AreEqual(expected, output.ToString());
            }
        }

        [Test]
        public void VariableScope() {
            string input = "" +
                "#let global 10\n" +
                "#def f1(global) global\n" +
                "#def f2(x) global\n" +
                "${global}\n" +
                "${f1(42)}\n" +
                "${f2(42)}\n";

            string expected = "10\n42\n10\n";

            CheckParserOutput(expected, input);
        }

        [Test]
        public void LocalVariables() {
            string input = "" +
                "#let x 4\n" +
                "#def func\n" +
                "${x}\n" +
                "#let x 5\n" +
                "#local x\n" +
                "#let x 6\n" +
                "${x}\n" +
                "#enddef\n" +
                "${func}\n" +
                "${x}\n";

            string expected = "4\n6\n5\n";

            CheckParserOutput(expected, input);
        }
    }
}