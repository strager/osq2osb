using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TokenNode {
    [TestFixture]
    class LetNodeTests {
        [Test]
        public void TestVariableNameParsing() {
            var node = new LetNode(
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
        public void TestVariableIsSetWithShorthand() {
            var node = new LetNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                    new Token(TokenType.Number, 42),
                }),
                null
            );

            var context = new ExecutionContext();

            node.Execute(context);

            Assert.AreEqual("42", context.GetVariable("test").ToString());
        }

        [Test]
        public void TestVariableIsSetWithLonghand() {
            var nodeChildren = new NodeBase[] {
                new RawTextNode("text", null),
                new RawTextNode("text", null),
                new EndDirectiveNode(null, null, "endlet"),
                new RawTextNode("blah", null),
            };

            var node = new LetNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "test"),
                }),
                new CollectionNodeReader(nodeChildren),
                "let"
            );

            var context = new ExecutionContext();

            node.Execute(context);

            Assert.AreEqual("texttext", context.GetVariable("test").ToString());
        }
    }
}
