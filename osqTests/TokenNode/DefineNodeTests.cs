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
    }
}


