using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TreeNode {
    [TestFixture]
    class IfNodeTests {
        private ITokenReader GetTrueCondition() {
            return new CollectionTokenReader(new[] {
                new Token(TokenType.Number, 1),
            });
        }

        private ITokenReader GetFalseCondition() {
            return new CollectionTokenReader(new[] {
                new Token(TokenType.Number, 0),
            });
        }

        [Test]
        public void IfTrue() {
            var trueChild = new RawTextNode("test", null);

            var node = new IfNode(
                GetTrueCondition(),
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
                GetTrueCondition(),
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
                GetFalseCondition(),
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

        [Test]
        public void IfTrueElseIfTrue() {
            var trueChild = new RawTextNode("test", null);

            var node = new IfNode(
                GetTrueCondition(),
                new CollectionNodeReader(new NodeBase[] {
                    trueChild,
                    new ElseIfNode(
                        GetTrueCondition(),
                        new CollectionNodeReader(new NodeBase[] {
                        })
                    ),
                    new EndDirectiveNode(null, null, "endif"),
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { trueChild }, node.GetTrueConditionSet(context).ChildrenNodes);
        }
    }
}
