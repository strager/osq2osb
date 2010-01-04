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
        private void CheckTree(object expected, Parser.TreeNode.TokenNode tree) {
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

        [Test]
        public void MathEvaluation() {
            var parser = new Parser.Parser();
            var tree = parser.ExpressionToTokenNode("(2) + (17*2-30) * (5)+2 - -(8/2)*4");

            Assert.AreEqual(40, tree.Value);
        }

        [Test]
        public void MathTree() {
            var parser = new Parser.Parser();

            CheckTree(new object[] {
                    "+",
                    2,
                    3
                },
                parser.ExpressionToTokenNode("2 + 3"));

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
                parser.ExpressionToTokenNode("((rand()) - (-(0.5))) / ((4))"));

            CheckTree(new object[] {
                    "a",
                    "b",
                    "c",
                    "d"
                },
                parser.ExpressionToTokenNode("a(b, c, d)"));
        }
    }
}