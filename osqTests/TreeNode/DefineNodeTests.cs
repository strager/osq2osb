using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TreeNode {
    [TestFixture]
    class DefineNodeTests {
        [Test]
        public void TestVariableNameParsing() {
            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.Symbol, "("),
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "c"),
                    new Token(TokenType.Symbol, ")"),
                    new Token(TokenType.Identifier, "blah"),
                }),
                null
            );

            Assert.AreEqual("test", node.Variable);
        }

        [Test]
        public void TestParameterParsing() {
            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.Symbol, "("),
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "c"),
                    new Token(TokenType.Symbol, ")"),
                    new Token(TokenType.Identifier, "blah"),
                }),
                null
            );

            Assert.AreEqual(new[] { "a", "b", "c" }, node.FunctionParameters);
        }

        [Test]
        public void TestShorthandParsing() {
            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.WhiteSpace, " "),
                    new Token(TokenType.Identifier, "a"),
                }),
                null
            );

            Assert.AreEqual(new Token[] { }, node.FunctionParameters);
            Assert.AreEqual(new NodeBase[] {
                new TokenNode(new Token(TokenType.Identifier, "a"))
            }, node.ChildrenNodes);
        }

        [Test]
        public void TestLonghandParsing() {
            var nodeChildren = new NodeBase[] {
                new RawTextNode("text", null),
                new RawTextNode("text", null),
                new EndDirectiveNode(null, null, "enddef"),
                new RawTextNode("blah", null),
            };

            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                }),
                new CollectionNodeReader(nodeChildren),
                "def"
            );

            Assert.AreEqual(nodeChildren.Take(2), node.ChildrenNodes);
        }

        [Test]
        public void TestVariableIsSet() {
            var node = new DefineNode {
                Variable = "test",
                ChildrenNodes = new[] {
                    new TokenNode(new Token(TokenType.Number, 42))
                },
            };

            var context = new ExecutionContext();

            node.Execute(context);

            Assert.NotNull(context.GetVariable("test") as ExecutionContext.OsqFunction);
        }

        [Test]
        public void TestVariableCall() {
            var node = new DefineNode {
                Variable = "test",
                ChildrenNodes = new[] {
                    new TokenNode(new Token(TokenType.Number, 42))
                },
            };

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            Assert.AreEqual("42", func(null, context).ToString());
        }

        [Test]
        public void TestFunctionCall() {
            var node = new DefineNode {
                Variable = "test",
                ChildrenNodes = new[] {
                    new TokenNode(new Token(TokenType.Symbol, ","), new[] {
                        new TokenNode(new Token(TokenType.Identifier, "a")),
                        new TokenNode(new Token(TokenType.Identifier, "b")),
                        new TokenNode(new Token(TokenType.Identifier, "c")),
                    })
                },
                FunctionParameters = new[] {
                    "a", "b", "c"
                },
            };

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            var functionCall = new TokenNode(new Token(TokenType.Identifier, "test"), new[] {
                new TokenNode(new Token(TokenType.Number, 1)),
                new TokenNode(new Token(TokenType.Number, 2)),
                new TokenNode(new Token(TokenType.Number, 3)),
            });

            Assert.AreEqual(context.GetStringOf(new[] { 1, 2, 3 }), func(functionCall, context) as IEnumerable);
        }
    }
}


