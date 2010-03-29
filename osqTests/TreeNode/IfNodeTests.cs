using System.Collections;
using System.Linq;
using NUnit.Framework;
using osq.Parser;
using osq.Tests.Helpers;
using osq.TreeNode;

namespace osq.Tests.TreeNode {
    [TestFixture]
    class IfNodeTests {
        private readonly Token trueConditionToken = new Token(TokenType.Number, 1);
        private readonly Token falseConditionToken = new Token(TokenType.Number, 0);

        private readonly NodeBase goodNode = new RawTextNode("good", null);
        private readonly NodeBase badNode = new RawTextNode("bad", null);

        private ITokenReader TrueCondition {
            get {
                return new CollectionTokenReader(new[] {
                    this.trueConditionToken
                });
            }
        }

        private ITokenReader FalseCondition {
            get {
                return new CollectionTokenReader(new[] {
                    this.falseConditionToken
                });
            }
        }

        private NodeBase GoodNode {
            get {
                return this.goodNode;
            }
        }

        private NodeBase BadNode {
            get {
                return this.badNode;
            }
        }

        [Test]
        public void IfTrue() {
            var node = new IfNode(
                TrueCondition,
                new CollectionNodeReader(new[] {
                    GoodNode,
                    new EndDirectiveNode(null, null, "endif"),
                    BadNode,
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { GoodNode }, node.GetTrueConditionSet(context).ChildrenNodes);
        }

        [Test]
        public void IfTrueElse() {
            var node = new IfNode(
                TrueCondition,
                new CollectionNodeReader(new[] {
                    GoodNode,
                    new ElseNode(null, null),
                    BadNode,
                    new EndDirectiveNode(null, null, "endif"),
                    BadNode,
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { GoodNode }, node.GetTrueConditionSet(context).ChildrenNodes);
        }

        [Test]
        public void IfFalseElse() {
            var node = new IfNode(
                FalseCondition,
                new CollectionNodeReader(new[] {
                    BadNode,
                    new ElseNode(null, null),
                    GoodNode,
                    new EndDirectiveNode(null, null, "endif"),
                    BadNode,
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { GoodNode }, node.GetTrueConditionSet(context).ChildrenNodes);
        }

        [Test]
        public void IfTrueElseIfTrue() {
            var node = new IfNode(
                TrueCondition,
                new CollectionNodeReader(new[] {
                    GoodNode,
                    new ElseIfNode(
                        TrueCondition,
                        new CollectionNodeReader(new[] {
                            BadNode,
                        })
                    ),
                    new EndDirectiveNode(null, null, "endif"),
                    BadNode,
                }),
                "if"
            );

            var context = new ExecutionContext();

            Assert.AreEqual(new[] { GoodNode }, node.GetTrueConditionSet(context).ChildrenNodes);
        }
    }
}
