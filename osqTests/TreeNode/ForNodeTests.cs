using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TreeNode {
    [TestFixture]
    public class ForNodeTests {
        [Test]
        public void TestIntegers() {
            var node = new ForNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Identifier, "i"),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Number, 0),
                    new Token(TokenType.Symbol, ","),
                    new Token(TokenType.Number, 10),
                }),
                new CollectionNodeReader(new[] {
                    new TokenNode(new Token(TokenType.Identifier, "i"))
                })
            );

            var context = new ExecutionContext();

            var result = node.Execute(context);

            Assert.AreEqual("0123456789", result);
        }
    }
}
