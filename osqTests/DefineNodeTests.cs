using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests {
    // TODO Make ITokenReader and INodeReader.
    // Make implementations for a real token/node reader (currently Token and Parser),
    // and for testy ones (reading from IEnumerables).
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
                new TokenNode(new Token(TokenType.Identifier, "a"))
            }, node.ChildrenNodes);
        }

    }
}
