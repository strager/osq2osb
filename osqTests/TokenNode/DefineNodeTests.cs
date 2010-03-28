using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TokenNode {
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
        public void TestShorthand() {
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
                new TreeNode.TokenNode(new Token(TokenType.Identifier, "a"))
            }, node.ChildrenNodes);
        }

        [Test]
        public void TestLonghand() {
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
        public void TestVariableIsSetWithShorthand() {
            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.Number, 42),
                }),
                null
            );

            var context = new ExecutionContext();

            node.Execute(context);

            Assert.NotNull(context.GetVariable("test") as ExecutionContext.OsqFunction);
        }

        [Test]
        public void TestVariableIsSetWithLonghand() {
            var nodeChildren = new NodeBase[] {
                new RawTextNode("text", null),
                new RawTextNode("text", null),
                new EndDirectiveNode(null, null, "endlet"),
                new RawTextNode("blah", null),
            };

            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                }),
                new CollectionNodeReader(nodeChildren),
                "let"
            );

            var context = new ExecutionContext();

            node.Execute(context);

            Assert.NotNull(context.GetVariable("test") as ExecutionContext.OsqFunction);
        }

        [Test]
        public void TestVariableCallWithShorthand() {
            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.Number, 42),
                }),
                null
            );

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            Assert.AreEqual("42", func(null, context).ToString());
        }

        [Test]
        public void TestVariableCallWithLonghand() {
            var nodeChildren = new NodeBase[] {
                new RawTextNode("text", null),
                new RawTextNode("text", null),
                new EndDirectiveNode(null, null, "endlet"),
                new RawTextNode("blah", null),
            };

            var node = new DefineNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                }),
                new CollectionNodeReader(nodeChildren),
                "let"
            );

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            Assert.AreEqual("texttext", func(null, context).ToString());
        }

        [Test]
        public void TestFunctionCallWithShorthand() {
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
                    new Token(TokenType.Identifier, "a"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "b"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Identifier, "c"),
                }),
                null
            );

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            var functionCall = new TreeNode.TokenNode(new Token(TokenType.Identifier, "test"));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 1)));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 2)));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 3)));

            Assert.AreEqual(context.GetStringOf(new[] { 1, 2, 3 }), func(functionCall, context) as IEnumerable);
        }

        [Test]
        public void TestFunctionCallWithLonghand() {
            var functionContents = new TreeNode.TokenNode(new Token(TokenType.Symbol, ","));
            functionContents.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Identifier, "a")));
            functionContents.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Identifier, "b")));
            functionContents.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Identifier, "c")));

            var nodeChildren = new NodeBase[] {
                functionContents,
                new EndDirectiveNode(null, null, "endlet"),
                new RawTextNode("blah", null),
            };

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
                }),
                new CollectionNodeReader(nodeChildren),
                "let"
            );

            var context = new ExecutionContext();

            node.Execute(context);

            var func = context.GetVariable("test") as ExecutionContext.OsqFunction;

            var functionCall = new TreeNode.TokenNode(new Token(TokenType.Identifier, "test"));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 1)));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 2)));
            functionCall.ChildrenNodes.Add(new TreeNode.TokenNode(new Token(TokenType.Number, 3)));

            var ret = func(functionCall, context);

            Assert.AreEqual(context.GetStringOf(new[] { 1, 2, 3 }), ret as IEnumerable);
        }

    }
}


