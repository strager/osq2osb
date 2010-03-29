using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TreeNode {
    [TestFixture]
    class IfNodeTests {
        [Test]
        public void IfTrue() {
            var trueChild = new RawTextNode("test", null);

            var node = new IfNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 1),
                }),
                new CollectionNodeReader(new NodeBase[] {
                    trueChild,
                    new EndDirectiveNode(null, null, "endif"),
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { trueChild }, node.GetTrueConditionSet(context).ChildrenNodes);
        }

        [Test]
        public void IfTrueElse() {
            var trueChild = new RawTextNode("test", null);

            var node = new IfNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 1),
                }),
                new CollectionNodeReader(new NodeBase[] {
                    trueChild,
                    new ElseNode(null, null),
                    new EndDirectiveNode(null, null, "endif"),
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { trueChild }, node.GetTrueConditionSet(context).ChildrenNodes);
        }

        [Test]
        public void IfFalseElse() {
            var trueChild = new RawTextNode("test", null);

            var node = new IfNode(
                new CollectionTokenReader(new[] {
                    new Token(TokenType.Number, 0),
                }),
                new CollectionNodeReader(new NodeBase[] {
                    new ElseNode(null, null),
                    trueChild,
                    new EndDirectiveNode(null, null, "endif"),
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { trueChild }, node.GetTrueConditionSet(context).ChildrenNodes);
        }
    }
}
