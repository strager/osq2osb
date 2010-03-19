using System;
using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    public class ExpressionRewriterTests {
        private static void CheckTree(object expected, TreeNode.TokenNode tree) {
            object[] array = expected as object[];

            if(array != null) {
                CheckTree(array[0], tree);

                var tokenChildren = tree.GetChildrenTokens();

                Assert.AreEqual(array.Length - 1, tokenChildren.Count);

                for(int i = 1; i < array.Length; ++i) {
                    CheckTree(array[i], tokenChildren[i - 1]);
                }
            } else {
                Assert.AreEqual(expected, tree.Token.Value);
            }
        }

        private static TreeNode.TokenNode ExpressionToTokenNode(string expression) {
            using(var reader = new LocatedTextReaderWrapper(expression, new Location())) {
                return ExpressionRewriter.Rewrite(Token.ReadTokens(reader));
            }
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
        public void ParenthesesExceptions() {
            Assert.Throws<MissingDataException>(delegate {
                ExpressionToTokenNode("2 + (2");
            });

            Assert.Throws<MissingDataException>(delegate {
                ExpressionToTokenNode("what(unmatched + 4");
            });

            Assert.DoesNotThrow(delegate {
                ExpressionToTokenNode("2) + 2)");
            });
        }

        [Test]
        public void Exceptions() {
            Assert.Throws<MissingDataException>(delegate {
                ExpressionToTokenNode("2 +");
            });
        }
    }
}
