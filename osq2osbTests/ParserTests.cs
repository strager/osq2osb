using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq2osb;
using osq2osb.Parser;

using NUnit.Framework;

namespace osq2osb.Tests {
    [TestFixture]
    class ParserTests {
        private static void CheckTree(object expected, Parser.TreeNode.TokenNode tree) {
            object[] array = expected as object[];

            if(array != null) {
                CheckTree(array[0], tree);

                var tokenChildren = tree.TokenChildren;

                Assert.AreEqual(array.Length - 1, tokenChildren.Count);

                for(int i = 1; i < array.Length; ++i) {
                    CheckTree(array[i], tokenChildren[i - 1]);
                }
            } else {
                Assert.AreEqual(expected, tree.Token.Value);
            }
        }

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
        public void MathTree() {
            CheckTree(new object[] {
                    "+",
                    2,
                    3
                },
                ExpressionToTokenNode("2 + 3"));

            CheckTree(new object[] {
                    "/",
                    new object[] {
                        "-",
                        "rand",
                        new object[] {
                            "-",
                            0.5
                        }
                    },
                    4
                },
                ExpressionToTokenNode("((rand()) - (-(0.5))) / ((4))"));

            CheckTree(new object[] {
                    "a",
                    new object[] {
                        ",",
                        "b",
                        "c",
                        "d"
                    }
                },
                ExpressionToTokenNode("a(b, c, d)"));

            CheckTree(new object[] {
                "int",
                new object[] {
                    "+",
                    new object[] {
                        "*",
                        new object[] {
                            "-",
                            "from",
                            320
                        },
                        "mscale"
                    },
                    320
                },
                },
                ExpressionToTokenNode("int((from - 320) * mscale + 320)"));
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