using System;
using NUnit.Framework;
using osq.Tests.Helpers;

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

        [Test]
        public void MathTree() {
            CheckTree(new object[] {
                "+",
                2,
                3
            }, ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                new Token(TokenType.Number, 2),
                new Token(TokenType.Symbol, "+"),
                new Token(TokenType.Number, 3),
            })));

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
            }, ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Identifier, "rand"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, "-"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Symbol, "-"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Number, 0.5),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, "/"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Number, 4),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, ")"),
            })));

            CheckTree(new object[] {
                "a",
                new object[] {
                    ",",
                    "b",
                    "c",
                    "d"
                }
            }, ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                new Token(TokenType.Identifier, "a"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Identifier, "b"),
                new Token(TokenType.Symbol, ","),
                new Token(TokenType.Identifier, "c"),
                new Token(TokenType.Symbol, ","),
                new Token(TokenType.Identifier, "d"),
                new Token(TokenType.Symbol, ")"),
            })));

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
            }, ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                new Token(TokenType.Identifier, "int"),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Symbol, "("),
                new Token(TokenType.Identifier, "from"),
                new Token(TokenType.Symbol, "-"),
                new Token(TokenType.Number, 320),
                new Token(TokenType.Symbol, ")"),
                new Token(TokenType.Symbol, "*"),
                new Token(TokenType.Identifier, "mscale"),
                new Token(TokenType.Symbol, "+"),
                new Token(TokenType.Number, 320),
                new Token(TokenType.Symbol, ")"),
            })));
        }

        [Test]
        public void ParenthesesExceptions() {
            Assert.Throws<MissingDataException>(delegate {
                ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 2),
                    new Token(TokenType.Symbol, "+"),
                    new Token(TokenType.Symbol, "("),
                    new Token(TokenType.Number, 2),
                }));
            });

            Assert.Throws<MissingDataException>(delegate {
                ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "what"),
                    new Token(TokenType.Symbol, "("),
                    new Token(TokenType.Identifier, "unmatched"),
                    new Token(TokenType.Symbol, "+"),
                    new Token(TokenType.Number, 4),
                }));
            });

            Assert.DoesNotThrow(delegate {
                ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 2),
                    new Token(TokenType.Symbol, ")"),
                    new Token(TokenType.Symbol, "+"),
                    new Token(TokenType.Number, 2),
                    new Token(TokenType.Symbol, ")"),
                }));
            });
        }

        [Test]
        public void Exceptions() {
            Assert.Throws<MissingDataException>(delegate {
                ExpressionRewriter.Rewrite(new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 2),
                    new Token(TokenType.Symbol, "+"),
                }));
            });
        }
    }
}
