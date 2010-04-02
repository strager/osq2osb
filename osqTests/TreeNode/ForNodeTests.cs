using System.Collections;
using System.Collections.Generic;
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
            var node = new ForNode {
                Start = new TokenNode(new Token(TokenType.Number, 0)),
                End = new TokenNode(new Token(TokenType.Number, 10)),
                Variable = "i",
                ChildrenNodes = new[] {
                    new TokenNode(new Token(TokenType.Identifier, "i"))
                }
            };

            var context = new ExecutionContext();
            var result = node.Execute(context);

            Assert.AreEqual("0123456789", result);
        }
    }
}
